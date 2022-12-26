using GBX.NET.Engines.Game;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic.Exceptions;
using System.Collections.Immutable;
using System.Diagnostics;
using TmEssentials;

namespace RandomizerTMF.Logic;

public class Session
{
    private readonly IRandomizerEngine engine;
    private readonly IRandomizerEvents events;
    private readonly IMapDownloader mapDownloader;
    private readonly IValidator validator;
    private readonly IRandomizerConfig config;
    private readonly ITMForever game;
    private readonly HttpClient http;
    private readonly ILogger logger;

    private bool isActualSkipCancellation;
    
    public Stopwatch Watch { get; } = new();
    public CancellationTokenSource TokenSource { get; } = new();

    public Task? Task { get; internal set; }

    public CancellationTokenSource? SkipTokenSource { get; private set; }

    public SessionMap? Map { get; set; }

    public SessionData? Data { get; set; }

    // This "trilogy" handles the storage of played maps. If the player didn't receive at least gold and didn't skip it, it is not counted in the progress.
    // It may (?) be better to wrap the CGameCtnChallenge into "CompletedMap" and have status of it being "gold", "author", or "skipped", and better handle that to make it script-friendly.
    public Dictionary<string, SessionMap> GoldMaps { get; } = new();
    public Dictionary<string, SessionMap> AuthorMaps { get; } = new();
    public Dictionary<string, SessionMap> SkippedMaps { get; } = new();
    
    public StreamWriter? LogWriter { get; set; }

    public Session(IRandomizerEngine engine,
                   IRandomizerEvents events,
                   IMapDownloader mapDownloader,
                   IValidator validator,
                   IRandomizerConfig config,
                   ITMForever game,
                   HttpClient http,
                   ILogger logger)
    {
        this.engine = engine;
        this.events = events;
        this.mapDownloader = mapDownloader;
        this.validator = validator;
        this.config = config;
        this.game = game;
        this.http = http;
        this.logger = logger;
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

        validator.ValidateRules(config.Rules);
        
        Data = SessionData.Initialize(config, logger);

        LogWriter = new StreamWriter(Path.Combine(Data.DirectoryPath, Constants.SessionLog))
        {
            AutoFlush = true
        };

        if (logger is LoggerToFile loggerToFile) // Maybe needs to be nicer
        {
            loggerToFile.SetSessionWriter(LogWriter);
        }

        while (true)
        {
            // This try block is used to handle map requests and their HTTP errors, mostly.

            try
            {
                await mapDownloader.PrepareNewMapAsync(this, cancellationToken);
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
        }
    }

    /// <summary>
    /// Handles the play loop of a map. Throws cancellation exception on session end (not the map end).
    /// </summary>
    /// <exception cref="TaskCanceledException"></exception>
    private async Task PlayMapAsync(CancellationToken cancellationToken)
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

        Watch.Start();

        SkipTokenSource = new CancellationTokenSource();

        events.OnMapStarted();

        Status("Playing the map...");

        // This loop either softly stops when the map is skipped by the player
        // or hardly stops when author medal is received / time limit is reached, End Session was clicked or an exception was thrown in general

        // SkipTokenSource is used within the session to skip a map, while CurrentSessionTokenSource handles the whole session cancellation

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

        // Map is no longer tracked at this point
    }

    public void StopTrackingMap()
    {
        SkipTokenSource = null;
        Map = null;
        events.OnMapEnded();
    }
    
    private void SkipManually(SessionMap map)
    {
        // If the player didn't receive at least a gold medal, the skip is counted (author medal automatically skips the map)
        if (GoldMaps.ContainsKey(map.MapUid) == false)
        {
            SkippedMaps.TryAdd(map.MapUid, map);
            map.LastTimestamp = Watch.Elapsed;
            Data?.SetMapResult(map, Constants.Skipped);
        }

        // In other words, if the player received at least a gold medal, the skip is forgiven

        // MapSkip event is thrown to update the UI
        events.OnMapSkip();
    }

    internal void AutosaveCreatedOrChanged(string fullPath, CGameCtnReplayRecord replay)
    {
        try
        {
            // Current session map autosave update section

            if (replay.MapInfo is null)
            {
                logger.LogWarning("Found autosave {autosavePath} with missing map info.", fullPath);
                return;
            }

            if (Map is null)
            {
                logger.LogWarning("Found autosave {autosavePath} for map {mapUid} while no session is running.", fullPath, replay.MapInfo.Id);
                return;
            }

            if (Map.MapInfo != replay.MapInfo)
            {
                logger.LogWarning("Found autosave {autosavePath} for map {mapUid} while the current session map is {currentSessionMapUid}.", fullPath, replay.MapInfo.Id, Map.MapInfo.Id);
                return;
            }
            
            Status("Copying the autosave...");

            Data?.UpdateFromAutosave(fullPath, Map, replay, Watch.Elapsed);

            Status("Checking the autosave...");

            // New autosave from the current map, save it into session for progression reasons

            // The following part has a scriptable potential
            // There are different medal rules for each gamemode (and where to look for validating)
            // So that's why the code looks like this for the time being

            if (Map.ChallengeParameters?.AuthorTime is null)
            {
                logger.LogWarning("Found autosave {autosavePath} for map {mapName} ({mapUid}) that has no author time.",
                    fullPath,
                    TextFormatter.Deformat(Map.Map.MapName).Trim(),
                    replay.MapInfo.Id);

                SkipTokenSource?.Cancel();
            }
            else
            {
                var ghost = replay.GetGhosts().First();

                if ((Map.Mode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle && replay.Time <= Map.ChallengeParameters.AuthorTime)
                 || (Map.Mode is CGameCtnChallenge.PlayMode.Platform && ((Map.ChallengeParameters.AuthorScore > 0 && ghost.Respawns <= Map.ChallengeParameters.AuthorScore) || (ghost.Respawns == 0 && replay.Time <= Map.ChallengeParameters.AuthorTime)))
                 || (Map.Mode is CGameCtnChallenge.PlayMode.Stunts && ghost.StuntScore >= Map.ChallengeParameters.AuthorScore))
                {
                    AuthorMedalReceived(Map);

                    SkipTokenSource?.Cancel();
                }
                else if ((Map.Mode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle && replay.Time <= Map.ChallengeParameters.GoldTime)
                      || (Map.Mode is CGameCtnChallenge.PlayMode.Platform && ghost.Respawns <= Map.ChallengeParameters.GoldTime.GetValueOrDefault().TotalMilliseconds)
                      || (Map.Mode is CGameCtnChallenge.PlayMode.Stunts && ghost.StuntScore >= Map.ChallengeParameters.GoldTime.GetValueOrDefault().TotalMilliseconds))
                {
                    GoldMedalReceived(Map);
                }
            }

            Status("Playing the map...");
        }
        catch (Exception ex)
        {
            Status("Error when checking the autosave...");
            logger.LogError(ex, "Error when checking the autosave {autosavePath}.", fullPath);
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

        events.OnMedalUpdate();
    }

    public void Stop()
    {
        StopTrackingMap();
        Watch.Stop();
        Data?.SetReadOnlySessionYml();
        LogWriter?.Dispose();
    }

    public Task SkipMapAsync()
    {
        Status("Requested to skip the map...");
        isActualSkipCancellation = true;
        SkipTokenSource?.Cancel();
        return Task.CompletedTask;
    }

    public void ReloadMap()
    {
        logger.LogInformation("Reloading the map...");

        if (Map?.FilePath is not null)
        {
            game.OpenFile(Map.FilePath);
        }
    }
}
