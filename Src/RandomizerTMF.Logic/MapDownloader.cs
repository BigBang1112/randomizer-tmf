using GBX.NET.Engines.Game;
using GBX.NET;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic.Exceptions;
using System.Diagnostics;
using System.Net;
using TmEssentials;
using System.Threading;

namespace RandomizerTMF.Logic;

public class MapDownloader
{
    private readonly RandomizerConfig config;
    private readonly HttpClient http;
    private readonly ILogger logger;

    private static readonly int requestMaxAttempts = 10;
    private static int requestAttempt;

    public MapDownloader(RandomizerConfig config, HttpClient http, ILogger logger)
    {
        this.config = config;
        this.http = http;
        this.logger = logger;
    }

    private void Status(string status)
    {
        
    }
    
    /// <summary>
    /// Requests, downloads, and allocates the map.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidSessionException"></exception>
    /// <exception cref="MapValidationException"></exception>
    /// <returns>True if map has been prepared successfully, false if soft error/problem appeared (it's possible to ask for another track).</returns>
    public async Task<bool> PrepareNewMapAsync(CurrentSession currentSession, CancellationToken cancellationToken)
    {
        var randomResponse = await FetchRandomTrackAsync(cancellationToken);

        // Following code gathers the track ID from the HEAD response (and ensures everything makes sense)

        var requestUri = GetRequestUriFromResponse(randomResponse);

        if (requestUri is null)
        {
            return false;
        }

        var trackId = GetTrackIdFromUri(requestUri);

        if (trackId is null)
        {
            return false;
        }

        // With the ID, it is possible to immediately download the track Gbx and process it with GBX.NET

        var trackGbxResponse = await DownloadMapByTrackIdAsync(requestUri.Host, trackId, cancellationToken);

        var map = await GetMapFromResponseAsync(trackGbxResponse, cancellationToken);

        if (map is null)
        {
            return false;
        }

        // Map validation ensures that the player won't receive duplicate maps
        // + ensures some additional filters like "No Unlimiter", which cannot be filtered on TMX
        await ValidateMapAsync(map, cancellationToken);

        // The map is saved to the defined DownloadedDirectoryPath using the FileName provided in ContentDisposition

        await SaveMapAsync(trackGbxResponse, map.MapUid, cancellationToken);

        var tmxLink = requestUri.ToString();

        currentSession.Map = new SessionMap(map, randomResponse.Headers.Date ?? DateTimeOffset.Now, tmxLink); // The map should be ready to be played now

        currentSession.Data?.Maps.Add(new()
        {
            Name = TextFormatter.Deformat(map.MapName),
            Uid = map.MapUid,
            TmxLink = tmxLink
        });

        return true;
    }

    private async Task SaveMapAsync(HttpResponseMessage trackGbxResponse, string mapUidFallback, CancellationToken cancellationToken)
    {
        Status("Saving the map...");

        if (FilePathManager.DownloadedDirectoryPath is null)
        {
            throw new UnreachableException("Cannot update autosaves without a valid user data directory path.");
        }

        logger.LogDebug("Ensuring {dir} exists...", FilePathManager.DownloadedDirectoryPath);
        Directory.CreateDirectory(FilePathManager.DownloadedDirectoryPath); // Ensures the directory really exists

        logger.LogDebug("Preparing the file name...");

        // Extracts the file name, and if it fails, it uses the MapUid as a fallback
        var fileName = trackGbxResponse.Content.Headers.ContentDisposition?.FileName?.Trim('\"') ?? $"{mapUidFallback}.Challenge.Gbx";

        // Validates the file name and fixes it if needed
        fileName = FilePathManager.ClearFileName(fileName);

        var mapSavePath = Path.Combine(FilePathManager.DownloadedDirectoryPath, fileName);

        logger.LogInformation("Saving the map as {fileName}...", mapSavePath);

        // WriteAllBytesAsync is used instead of GameBox.Save to ensure 1:1 data of the original map
        var trackData = await trackGbxResponse.Content.ReadAsByteArrayAsync(cancellationToken);

        await File.WriteAllBytesAsync(mapSavePath, trackData, cancellationToken);

        logger.LogInformation("Map saved successfully!");
    }

    private async Task<HttpResponseMessage> FetchRandomTrackAsync(CancellationToken cancellationToken)
    {
        Status("Fetching random track...");

        // Randomized URL is constructed with the ToUrl() method.
        var requestUrl = config.Rules.RequestRules.ToUrl();

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

    private Uri? GetRequestUriFromResponse(HttpResponseMessage response)
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

    private string? GetTrackIdFromUri(Uri uri)
    {
        var trackId = uri.Segments.LastOrDefault();

        if (trackId is null)
        {
            logger.LogWarning("Request URI does not contain any segments. This is very odd...");
            return null;
        }

        return trackId;
    }

    private async Task<HttpResponseMessage> DownloadMapByTrackIdAsync(string host, string trackId, CancellationToken cancellationToken)
    {
        Status($"Downloading track {trackId}...");

        var trackGbxUrl = $"https://{host}/trackgbx/{trackId}";

        logger.LogDebug("Downloading track on {trackGbxUrl}...", trackGbxUrl);
        using var trackGbxResponse = await http.GetAsync(trackGbxUrl, cancellationToken);
        trackGbxResponse.EnsureSuccessStatusCode();

        return trackGbxResponse;
    }

    private async Task<CGameCtnChallenge?> GetMapFromResponseAsync(HttpResponseMessage trackGbxResponse, CancellationToken cancellationToken)
    {
        using var stream = await trackGbxResponse.Content.ReadAsStreamAsync(cancellationToken);

        // The map is gonna be parsed as it is downloading throughout

        Status("Parsing the map...");

        if (GameBox.ParseNode(stream) is CGameCtnChallenge map)
        {
            return map;
        }

        logger.LogWarning("Downloaded file is not a valid Gbx map file!");

        return null;
    }

    private async Task ValidateMapAsync(CGameCtnChallenge map, CancellationToken cancellationToken)
    {
        Status("Validating the map...");

        requestAttempt = 0;

        if (Validator.ValidateMap(config, map, out string? invalidBlock))
        {
            return;
        }
        
        // Attempts another track if invalid
        requestAttempt++;

        if (invalidBlock is not null)
        {
            Status($"{invalidBlock} in {map.Collection}");
            logger.LogInformation("Map is invalid because {invalidBlock} is not valid for the {env} environment.", invalidBlock, map.Collection);
            await Task.Delay(500, cancellationToken);
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
