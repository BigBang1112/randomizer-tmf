using GBX.NET.Engines.Game;
using GBX.NET;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic.Exceptions;
using System.Diagnostics;
using System.Net;
using TmEssentials;
using System.IO.Abstractions;

namespace RandomizerTMF.Logic.Services;

public interface IMapDownloader
{
    Task<bool> PrepareNewMapAsync(Session currentSession, CancellationToken cancellationToken);
}

public class MapDownloader : IMapDownloader
{
    private readonly IRandomizerEvents events;
    private readonly IRandomizerConfig config;
    private readonly IFilePathManager filePathManager;
    private readonly IDiscordRichPresence discord;
    private readonly IValidator validator;
    private readonly HttpClient http;
    private readonly IRandomGenerator random;
    private readonly IDelayService delayService;
    private readonly IFileSystem fileSystem;
    private readonly IGbxService gbxService;
    private readonly ILogger logger;

    private readonly int requestMaxAttempts = 10;
    private int requestAttempt;

    public MapDownloader(IRandomizerEvents events,
                         IRandomizerConfig config,
                         IFilePathManager filePathManager,
                         IDiscordRichPresence discord,
                         IValidator validator,
                         HttpClient http,
                         IRandomGenerator random,
                         IDelayService delayService,
                         IFileSystem fileSystem,
                         IGbxService gbxService,
                         ILogger logger)
    {
        this.events = events;
        this.config = config;
        this.filePathManager = filePathManager;
        this.discord = discord; // After PrepareNewMapAsync refactor no longer needed
        this.validator = validator;
        this.http = http;
        this.random = random;
        this.delayService = delayService;
        this.fileSystem = fileSystem;
        this.gbxService = gbxService;
        this.logger = logger;
    }

    private void Status(string status)
    {
        events.OnStatus(status);
    }

    /// <summary>
    /// Requests, downloads, and allocates the map.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidSessionException"></exception>
    /// <exception cref="MapValidationException"></exception>
    /// <returns>True if map has been prepared successfully, false if soft error/problem appeared (it's possible to ask for another track).</returns>
    public async Task<bool> PrepareNewMapAsync(Session currentSession, CancellationToken cancellationToken)
    {
        using var randomResponse = await FetchRandomTrackAsync(cancellationToken);

        // Following code gathers the track ID from the HEAD response (and ensures everything makes sense)

        var requestUri = GetRequestUriFromResponse(randomResponse);

        if (requestUri is null)
        {
            logger.LogWarning("RequestUri of /trackrandom is null");
            return false;
        }

        var trackId = GetTrackIdFromUri(requestUri);

        if (trackId is null) // Cannot really happen
        {
            logger.LogWarning("TrackId of Uri redirect is null");
            return false;
        }

        // With the ID, it is possible to immediately download the track Gbx and process it with GBX.NET

        using var trackGbxResponse = await DownloadMapByTrackIdAsync(requestUri.Host, trackId, cancellationToken);

        var map = await GetMapFromResponseAsync(trackGbxResponse, cancellationToken);

        if (map is null)
        {
            logger.LogWarning("Map object from /trackgbx is null");
            return false;
        }

        // Map validation ensures that the player won't receive duplicate maps
        // + ensures some additional filters like "No Unlimiter", which cannot be filtered on TMX
        await ValidateMapAsync(map, cancellationToken);

        // The map is saved to the defined DownloadedDirectoryPath using the FileName provided in ContentDisposition

        var mapSavePath = await SaveMapAsync(trackGbxResponse, map.MapUid, cancellationToken);

        var tmxLink = requestUri.ToString();

        currentSession.Map = new SessionMap(map, randomResponse.Headers.Date ?? DateTimeOffset.Now, tmxLink) // The map should be ready to be played now
        {
            FilePath = mapSavePath
        };

        var mapName = TextFormatter.Deformat(map.MapName);

        currentSession.Data?.Maps.Add(new()
        {
            Name = mapName,
            Uid = map.MapUid,
            TmxLink = tmxLink,
        });

        discord.SessionMap(mapName, $"https://{requestUri.Host}/trackshow/{trackId}/image/1", map.Collection);

        return true;
    }

    internal async Task<string> SaveMapAsync(HttpResponseMessage trackGbxResponse, string mapUidFallback, CancellationToken cancellationToken)
    {
        Status("Saving the map...");

        if (filePathManager.DownloadedDirectoryPath is null)
        {
            throw new UnreachableException("Cannot update autosaves without a valid user data directory path.");
        }

        logger.LogDebug("Ensuring {dir} exists...", filePathManager.DownloadedDirectoryPath);
        fileSystem.Directory.CreateDirectory(filePathManager.DownloadedDirectoryPath); // Ensures the directory really exists

        logger.LogDebug("Preparing the file name...");

        // Extracts the file name, and if it fails, it uses the MapUid as a fallback
        var fileName = trackGbxResponse.Content.Headers.ContentDisposition?.FileName?.Trim('\"') ?? $"{mapUidFallback}.Challenge.Gbx";

        // Validates the file name and fixes it if needed
        fileName = FilePathManager.ClearFileName(fileName);

        var mapSavePath = Path.Combine(filePathManager.DownloadedDirectoryPath, fileName);

        logger.LogInformation("Saving the map as {fileName}...", mapSavePath);

        // WriteAllBytesAsync is used instead of GameBox.Save to ensure 1:1 data of the original map
        var trackData = await trackGbxResponse.Content.ReadAsByteArrayAsync(cancellationToken);

        await fileSystem.File.WriteAllBytesAsync(mapSavePath, trackData, cancellationToken);

        logger.LogInformation("Map saved successfully!");

        return mapSavePath;
    }

    internal async Task<HttpResponseMessage> FetchRandomTrackAsync(CancellationToken cancellationToken)
    {
        Status("Fetching random track...");

        // Randomized URL is constructed with the ToUrl() method.
        var requestUrl = config.Rules.RequestRules.ToUrl(random);

        logger.LogDebug("Requesting generated URL: {url}", requestUrl);
        var randomResponse = await http.HeadAsync(requestUrl, cancellationToken);

        if (randomResponse.StatusCode == HttpStatusCode.NotFound)
        {
            // The session is ALWAYS invalid if there's no map that can be found.
            // This DOES NOT relate to the lack of maps left that the user hasn't played.

            logger.LogWarning("No map fulfills the randomization filter.");

            throw new InvalidSessionException();
        }

        randomResponse.EnsureSuccessStatusCode(); // Handles server issues, should normally retry

        return randomResponse;
    }

    internal Uri? GetRequestUriFromResponse(HttpResponseMessage response)
    {
        if (response.RequestMessage is null)
        {
            logger.LogWarning("Response from the HEAD request does not contain information about the request message. This is odd...");
            return null;
        }

        if (response.RequestMessage.RequestUri is null)
        {
            logger.LogWarning("Response from the HEAD request does not contain information about the request URI. This is odd...");
            return null;
        }

        return response.RequestMessage.RequestUri;
    }

    internal string? GetTrackIdFromUri(Uri uri)
    {
        var trackId = uri.Segments.LastOrDefault();

        if (trackId is null)
        {
            logger.LogWarning("Request URI does not contain any segments. This is very odd...");
            return null;
        }

        return trackId;
    }

    internal async Task<HttpResponseMessage> DownloadMapByTrackIdAsync(string host, string trackId, CancellationToken cancellationToken)
    {
        Status($"Downloading track {trackId}...");

        var trackGbxUrl = $"https://{host}/trackgbx/{trackId}";

        logger.LogDebug("Downloading track on {trackGbxUrl}...", trackGbxUrl);

        var trackGbxResponse = await http.GetAsync(trackGbxUrl, cancellationToken);

        trackGbxResponse.EnsureSuccessStatusCode();

        return trackGbxResponse;
    }

    private async Task<CGameCtnChallenge?> GetMapFromResponseAsync(HttpResponseMessage trackGbxResponse, CancellationToken cancellationToken)
    {
        using var stream = await trackGbxResponse.Content.ReadAsStreamAsync(cancellationToken);

        // The map is gonna be parsed as it is downloading throughout

        Status("Parsing the map...");

        if (gbxService.Parse(stream) is CGameCtnChallenge map)
        {
            return map;
        }

        logger.LogWarning("Downloaded file is not a valid Gbx map file!");

        return null;
    }

    internal async Task ValidateMapAsync(CGameCtnChallenge map, CancellationToken cancellationToken)
    {
        Status("Validating the map...");

        if (validator.ValidateMap(map, out string? invalidBlock))
        {
            requestAttempt = 0;
            return;
        }

        // Attempts another track if invalid
        requestAttempt++;

        if (invalidBlock is not null)
        {
            Status($"{invalidBlock} in {map.Collection}");
            logger.LogInformation("Map is invalid because {invalidBlock} is not valid for the {env} environment.", invalidBlock, map.Collection);
            await delayService.Delay(500, cancellationToken);
        }

        Status($"Map is invalid (attempt {requestAttempt}/{requestMaxAttempts}).");

        if (requestAttempt >= requestMaxAttempts)
        {
            logger.LogWarning("Map is invalid after {MaxAttempts} attempts. Cancelling the session...", requestMaxAttempts);
            requestAttempt = 0;
            throw new InvalidSessionException();
        }

        throw new MapValidationException();
    }
}
