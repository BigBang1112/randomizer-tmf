using GBX.NET.Engines.Game;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic.Exceptions;
using RandomizerTMF.Logic.Services;
using System.Diagnostics;
using System.IO.Abstractions;
using TmEssentials;

namespace RandomizerTMF.Logic;

public interface ISession
{
    Dictionary<string, SessionMap> AuthorMaps { get; }
    SessionData? Data { get; }
    Dictionary<string, SessionMap> GoldMaps { get; }
    StreamWriter? LogWriter { get; set; }
    SessionMap? Map { get; set; }
    Dictionary<string, SessionMap> SkippedMaps { get; }
    CancellationTokenSource? SkipTokenSource { get; }
    Task? Task { get; }
    CancellationTokenSource TokenSource { get; }
    Stopwatch Watch { get; }

    bool ReloadMap();
    Task SkipMapAsync();
    void Start();
    void Stop();
}

public class Session : ISession
{
    private readonly IRandomizerEvents events;
    private readonly IMapDownloader mapDownloader;
    private readonly IValidator validator;
    private readonly IRandomizerConfig config;
    private readonly ITMForever game;
    private readonly ILogger logger;
    private readonly IFileSystem fileSystem;

    private bool isActualSkipCancellation;
    private DateTime watchTemporaryStopTimestamp;

    public Stopwatch Watch { get; } = new();
    public CancellationTokenSource TokenSource { get; } = new();

    public Task? Task { get; internal set; }

    public CancellationTokenSource? SkipTokenSource { get; private set; }

    public SessionMap? Map { get; set; }

    public SessionData? Data { get; private set; }

    // This "trilogy" handles the storage of played maps. If the player didn't receive at least gold and didn't skip it, it is not counted in the progress.
    // It may (?) be better to wrap the CGameCtnChallenge into "CompletedMap" and have status of it being "gold", "author", or "skipped", and better handle that to make it script-friendly.
    public Dictionary<string, SessionMap> GoldMaps { get; } = new();
    public Dictionary<string, SessionMap> AuthorMaps { get; } = new();
    public Dictionary<string, SessionMap> SkippedMaps { get; } = new();

    public StreamWriter? LogWriter { get; set; }

    public Session(IRandomizerEvents events,
                   IMapDownloader mapDownloader,
                   IValidator validator,
                   IRandomizerConfig config,
                   ITMForever game,
                   ILogger logger,
                   IFileSystem fileSystem)
    {
        this.events = events;
        this.mapDownloader = mapDownloader;
        this.validator = validator;
        this.config = config;
        this.game = game;
        this.logger = logger;
        this.fileSystem = fileSystem;
    }

    private void Status(string status)
    {
        events.OnStatus(status);
    }

    public void Start()
    {
        Task = Task.Run(() => RunSessionSafeAsync(TokenSource.Token), TokenSource.Token);
    }

    /// <summary>
    /// Runs the session in a way it won't ever throw an exception. Clears the session after its end as well.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task RunSessionSafeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await RunSessionAsync(cancellationToken);
        }
        catch (TaskCanceledException)
        {
            Status("Session ended.");
        }
        catch (InvalidSessionException)
        {
            Status("Session ended. No maps found.");
        }
        catch (Exception ex)
        {
            Status("Session ended due to error.");
            logger.LogError(ex, "Error during session.");
        }
        finally
        {
            Stop();
        }
    }

    /// <summary>
    /// Does the actual work during a running session. That this method ends means the session also ends. It does NOT clean up the session after its end.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="TaskCanceledException"></exception>
    private async Task RunSessionAsync(CancellationToken cancellationToken)
    {
        if (config.GameDirectory is null)
        {
            throw new UnreachableException("Game directory is null");
        }

        validator.ValidateRules();

        Data = SessionData.Initialize(config, logger, fileSystem);

        LogWriter = fileSystem.File.CreateText(Path.Combine(Data.DirectoryPath, Constants.SessionLog));
        LogWriter.AutoFlush = true;

        if (logger is LoggerToFile loggerToFile) // Maybe needs to be nicer
        {
            loggerToFile.SetSessionWriter(LogWriter);
        }

        while (true)
        {
            // This try block is used to handle map requests and their HTTP errors, mostly.

            try
            {
                if (!await mapDownloader.PrepareNewMapAsync(this, cancellationToken))
                {
                    await Task.Delay(500, cancellationToken);
                    continue;
                }

                Data?.Save(); // May not be super necessary?
            }
            catch (HttpRequestException)
            {
                Status("Failed to fetch a track. Retrying...");
                await Task.Delay(1000, cancellationToken);
                continue;
            }
            catch (MapValidationException)
            {
                logger.LogInformation("Map has not passed the validator, attempting another one...");
                await Task.Delay(500, cancellationToken);
                continue;
            }
            catch (InvalidSessionException)
            {
                logger.LogWarning("Session is invalid.");
                throw;
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("Session terminated during map request.");
                throw;
            }
            catch (Exception ex)
            {
                Status("Error! Check the log for more details.");
                logger.LogError(ex, "An error occurred during map request.");

                await Task.Delay(1000, cancellationToken);

                continue;
            }

            await PlayMapAsync(cancellationToken);

            // Map is no longer tracked at this point
        }
    }

    /// <summary>
    /// Handles the play loop of a map. Throws cancellation exception on session end (not the map end).
    /// </summary>
    /// <exception cref="TaskCanceledException"></exception>
    internal async Task PlayMapAsync(CancellationToken cancellationToken)
    {
        // Hacky last moment validations

        if (Map is null)
        {
            throw new UnreachableException("Map is null");
        }

        if (Map.FilePath is null)
        {
            throw new UnreachableException("Map.FilePath is null");
        }

        // Map starts here

        Status("Starting the map...");

        game.OpenFile(Map.FilePath);

        if (Watch.ElapsedTicks == 0)
        {
            events.OnFirstMapStarted();
        }

        SkipTokenSource = StartTrackingMap();

        events.OnMapStarted(Map); // This has to be called after SkipTokenSource is set

        Status("Playing the map...");

        // This loop either softly stops when the map is skipped by the player
        // or hardly stops when author medal is received / time limit is reached, End Session was clicked or an exception was thrown in general

        // SkipTokenSource is used within the session to skip a map, while TokenSource handles the whole session cancellation

        while (!SkipTokenSource.IsCancellationRequested)
        {
            if (Watch.Elapsed >= config.Rules.TimeLimit) // Time limit reached case
            {
                if (TokenSource is null)
                {
                    throw new UnreachableException("CurrentSessionTokenSource is null");
                }

                // Will cause the Task.Delay below to throw a cancellation exception
                // Code outside of the while loop wont be reached
                TokenSource.Cancel();
            }

            await Task.Delay(20, cancellationToken);
        }

        Watch.Stop(); // Time is paused until the next map starts
        watchTemporaryStopTimestamp = DateTime.UtcNow;

        if (isActualSkipCancellation) // When its a manual skip and not an automated skip by author medal receive
        {
            Status("Skipping the map...");

            // Apply the rules related to manual map skip
            // This part has a scripting potential too if properly implemented

            SkipManually(Map);

            isActualSkipCancellation = false;
        }

        Status("Ending the map...");

        StopTrackingMap();
    }

    private CancellationTokenSource StartTrackingMap()
    {
        if (Watch.ElapsedTicks > 0)
        {
            events.OnTimeResume(DateTime.UtcNow - watchTemporaryStopTimestamp);
        }

        Watch.Start();

        events.AutosaveCreatedOrChanged += AutosaveCreatedOrChangedSafe;

        return new CancellationTokenSource();
    }

    internal void StopTrackingMap()
    {
        events.AutosaveCreatedOrChanged -= AutosaveCreatedOrChangedSafe;
        SkipTokenSource = null;
        Map = null;
        events.OnMapEnded();
    }

    internal void SkipManually(SessionMap map)
    {
        // If the player didn't receive a gold/author medal, the skip is counted
        if (GoldMaps.ContainsKey(map.MapUid) == false && AuthorMaps.ContainsKey(map.MapUid) == false)
        {
            SkippedMaps.TryAdd(map.MapUid, map);
            map.LastTimestamp = Watch.Elapsed;
            Data?.SetMapResult(map, Constants.Skipped);
        }

        // In other words, if the player received a gold/author medal, the skip is forgiven

        // MapSkip event is thrown to update the UI
        events.OnMapSkip();
    }

    private void AutosaveCreatedOrChangedSafe(string fullPath, CGameCtnReplayRecord replay)
    {
        try
        {
            _ = AutosaveCreatedOrChanged(fullPath, replay);
        }
        catch (Exception ex)
        {
            Status("Error when checking the autosave...");
            logger.LogError(ex, "Error when checking the autosave {autosavePath}.", fullPath);
        }
    }

    internal bool AutosaveCreatedOrChanged(string fullPath, CGameCtnReplayRecord replay)
    {
        // Current session map autosave update section

        if (replay.MapInfo is null)
        {
            logger.LogWarning("Found autosave {autosavePath} with missing map info.", fullPath);
            return false;
        }

        if (Map is null)
        {
            logger.LogWarning("Found autosave {autosavePath} for map {mapUid} while no session is running.", fullPath, replay.MapInfo.Id);
            return false;
        }

        if (Map.MapInfo != replay.MapInfo)
        {
            logger.LogWarning("Found autosave {autosavePath} for map {mapUid} while the current session map is {currentSessionMapUid}.", fullPath, replay.MapInfo.Id, Map.MapInfo.Id);
            return false;
        }

        Status("Copying the autosave...");

        Data?.UpdateFromAutosave(fullPath, Map, replay, Watch.Elapsed);

        Status("Checking the autosave...");

        // New autosave from the current map, save it into session for progression reasons

        // The following part has a scriptable potential
        // There are different medal rules for each gamemode (and where to look for validating)

        EvaluateAutosave(fullPath, replay);

        Status("Playing the map...");

        return true;
    }

    internal void EvaluateAutosave(string fullPath, CGameCtnReplayRecord replay)
    {
        _ = Map is null || replay.MapInfo is null ? throw new UnreachableException("Map or Map.MapInfo is null") : "";

        if (Map.ChallengeParameters.AuthorTime is null)
        {
            logger.LogWarning("Found autosave {autosavePath} for map {mapName} ({mapUid}) that has no author time.",
                fullPath,
                TextFormatter.Deformat(Map.Map.MapName).Trim(),
                replay.MapInfo.Id);

            if (!config.DisableAutoSkip)
            {
                SkipTokenSource?.Cancel();
            }

            return;
        }

        var ghost = replay.GetGhosts(alsoInClips: false).First();

        if (Map.IsAuthorMedal(ghost))
        {
            AuthorMedalReceived(Map);

            if (!config.DisableAutoSkip)
            {
                SkipTokenSource?.Cancel();
            }

            return;
        }

        if (Map.IsGoldMedal(ghost))
        {
            GoldMedalReceived(Map);
        }
    }

    private void GoldMedalReceived(SessionMap map)
    {
        GoldMaps.TryAdd(map.MapUid, map);
        map.LastTimestamp = Watch.Elapsed;
        Data?.SetMapResult(map, Constants.GoldMedal);

        events.OnMedalUpdate();
    }

    private void AuthorMedalReceived(SessionMap map)
    {
        GoldMaps.Remove(map.MapUid);
        AuthorMaps.TryAdd(map.MapUid, map);
        map.LastTimestamp = Watch.Elapsed;
        Data?.SetMapResult(map, Constants.AuthorMedal);

        if (config.Rules.RequestRules.SurvivalMode)
        {
            config.Rules.TimeLimit += config.Rules.RequestRules.SurvivalBonusTime!.Value;
        }

        events.OnMedalUpdate();
    }

    public void Stop()
    {
        StopTrackingMap();
        Watch.Stop();
        Data?.SetReadOnlySessionYml();
        LogWriter?.Dispose();

        // making sure the UI does not change after RMS
        config.Rules.TimeLimit = config.Rules.OriginalTimeLimit;
    }

    public Task SkipMapAsync()
    {
        Status("Requested to skip the map...");

        // this is where skip limits make their effects

        // checking if the player meets the requirements for free skipping or gold skipping
        if (AuthorMaps.ContainsKey(Map!.MapUid) == false)
        {
            if (GoldMaps.ContainsKey(Map!.MapUid) == false)
            {
                if (config.Rules.RequestRules.FreeSkipLimit <= SkippedMaps.Count && config.Rules.RequestRules.FreeSkipLimit is not null)
                {
                    Status("Skip denied: free skip limit reached.");

                    return Task.CompletedTask;
                }
            }
            else
            {
                if (config.Rules.RequestRules.GoldSkipLimit <= (GoldMaps.Count - 1) && config.Rules.RequestRules.GoldSkipLimit is not null)
                {
                    Status("Skip denied: gold skip limit reached.");

                    return Task.CompletedTask;
                }
            }
            
        }

        isActualSkipCancellation = true;
        SkipTokenSource?.Cancel();

        return Task.CompletedTask;
    }

    public bool ReloadMap()
    {
        logger.LogInformation("Reloading the map...");

        if (Map?.FilePath is null)
        {
            return false;
        }

        game.OpenFile(Map.FilePath);

        return true;
    }
}
