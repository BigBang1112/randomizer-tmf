﻿using GBX.NET.Engines.Game;
using GBX.NET;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using GBX.NET.Exceptions;
using RandomizerTMF.Logic.Exceptions;
using System.Diagnostics;
using TmEssentials;
using System.IO.Abstractions;

namespace RandomizerTMF.Logic.Services;

public interface IAutosaveScanner
{
    ConcurrentDictionary<string, AutosaveDetails> AutosaveDetails { get; }
    ConcurrentDictionary<string, AutosaveHeader> AutosaveHeaders { get; }
    bool HasAutosavesScanned { get; }

    void ResetAutosaves();
    bool ScanAutosaves();
    void ScanDetailsFromAutosaves();
}

public class AutosaveScanner : IAutosaveScanner
{
    private readonly IRandomizerEvents events;
    private readonly IFileSystemWatcher watcher;
    private readonly IFilePathManager filePathManager;
    private readonly IRandomizerConfig config;
    private readonly IFileSystem fileSystem;
    private readonly IGbxService gbx;
    private readonly ILogger logger;

    private bool hasAutosavesScanned;

    /// <summary>
    /// If the map UIDs of the autosaves have been fully stored to the <see cref="AutosaveHeaders"/> and <see cref="AutosavePaths"/> dictionaries.
    /// This property is required to be true in order to start a new session. It also handles the state of <see cref="AutosaveWatcher"/>, to catch other autosaves that might be created while the program is running.
    /// </summary>
    public bool HasAutosavesScanned
    {
        get => hasAutosavesScanned;
        internal set
        {
            hasAutosavesScanned = value;
            watcher.EnableRaisingEvents = value;
        }
    }

    public ConcurrentDictionary<string, AutosaveHeader> AutosaveHeaders { get; } = new();
    public ConcurrentDictionary<string, AutosaveDetails> AutosaveDetails { get; } = new();
    

    public AutosaveScanner(IRandomizerEvents events,
                           IFileSystemWatcher watcher,
                           IFilePathManager filePathManager,
                           IRandomizerConfig config,
                           IFileSystem fileSystem,
                           IGbxService gbx,
                           ILogger logger)
    {
        this.events = events;
        this.watcher = watcher;
        this.filePathManager = filePathManager;
        this.config = config;
        this.fileSystem = fileSystem;
        this.gbx = gbx;
        this.logger = logger;

        filePathManager.UserDataDirectoryPathUpdated += UserDataDirectoryPathUpdated;
        watcher.Changed += OnAutosaveCreatedOrChanged;
    }

    internal void UserDataDirectoryPathUpdated()
    {
        watcher.Path = filePathManager.AutosavesDirectoryPath ?? "";
        ResetAutosaves();
    }

    private static DateTime lastAutosaveUpdate = DateTime.MinValue;

    internal void OnAutosaveCreatedOrChanged(object sender, FileSystemEventArgs e)
    {
        // Hack to fix the issue of this event sometimes running twice
        var lastWriteTime = fileSystem.File.GetLastWriteTime(e.FullPath);

        if (lastWriteTime == lastAutosaveUpdate)
        {
            return;
        }

        lastAutosaveUpdate = lastWriteTime;
        //

        var retryCounter = 0;

        CGameCtnReplayRecord replay;

        while (true)
        {
            try
            {
                // Any kind of autosave update section

                logger.LogInformation("Analyzing a new file {autosavePath} in autosaves folder...", e.FullPath);

                using var stream = fileSystem.File.OpenRead(e.FullPath);

                if (Gbx.ParseNode(stream) is not CGameCtnReplayRecord r)
                {
                    logger.LogWarning("Found file {file} that is not a replay.", e.FullPath);
                    return;
                }

                if (r.MapInfo is null)
                {
                    logger.LogWarning("Found replay {file} that has no map info.", e.FullPath);
                    return;
                }

                AutosaveHeaders.TryAdd(r.MapInfo.Id, new AutosaveHeader(Path.GetFileName(e.FullPath), r));

                replay = r;
            }
            catch (Exception ex)
            {
                retryCounter++;

                logger.LogError(ex, "Error while analyzing a new file {autosavePath} in autosaves folder (retry {counter}/{maxRetries}).",
                    e.FullPath, retryCounter, config.ReplayParseFailRetries);

                if (retryCounter >= config.ReplayParseFailRetries)
                {
                    return;
                }

                Thread.Sleep(config.ReplayParseFailDelayMs);

                continue;
            }

            break;
        }

        events.OnAutosaveCreatedOrChanged(e.FullPath, replay);
    }

    /// <summary>
    /// Scans the autosaves, which is required before running the session, to avoid already played maps.
    /// </summary>
    /// <returns>True if anything changed.</returns>
    /// <exception cref="ImportantPropertyNullException"></exception>
    public bool ScanAutosaves()
    {
        if (filePathManager.AutosavesDirectoryPath is null)
        {
            throw new ImportantPropertyNullException("Cannot scan autosaves without a valid user data directory path.");
        }

        var anythingChanged = false;

        foreach (var fileName in fileSystem.Directory.EnumerateFiles(filePathManager.AutosavesDirectoryPath).AsParallel())
        {
            try
            {
                if (ProcessAutosaveHeader(fileName))
                {
                    anythingChanged = true;
                }
            }
            catch (NotAGbxException)
            {
                // do nothing
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Gbx error found in the Autosaves folder when reading the header.");
            }
        }

        HasAutosavesScanned = true;

        return anythingChanged;
    }

    internal bool ProcessAutosaveHeader(string fileName)
    {
        using var stream = fileSystem.File.OpenRead(fileName);

        if (gbx.ParseHeader(stream) is not CGameCtnReplayRecord replay || replay.MapInfo is null)
        {
            return false;
        }

        return AutosaveHeaders.TryAdd(replay.MapInfo.Id, new AutosaveHeader(Path.GetFileName(fileName), replay));
    }

    /// <summary>
    /// Scans autosave details (mostly the map inside the replay and getting its info) that are then used to display more detailed information about a list of maps. Only some replay properties are stored to value memory.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void ScanDetailsFromAutosaves()
    {
        foreach (var autosaveMapUid in AutosaveHeaders.Keys)
        {
            try
            {
                UpdateAutosaveDetail(autosaveMapUid);
            }
            catch (Exception ex)
            {
                // this happens in async context, logger may not be safe for that, some errors may get lost
                Debug.WriteLine("Exception during autosave detail scan: " + ex);
            }
        }
    }

    internal void UpdateAutosaveDetail(string autosaveMapUid)
    {
        if (filePathManager.AutosavesDirectoryPath is null)
        {
            throw new ImportantPropertyNullException("Cannot update autosave details without a valid autosaves directory.");
        }
        
        var autosavePath = Path.Combine(filePathManager.AutosavesDirectoryPath, AutosaveHeaders[autosaveMapUid].FilePath);

        using var stream = fileSystem.File.OpenRead(autosavePath);

        try
        {
            if (gbx.Parse(stream) is not CGameCtnReplayRecord { Time: not null } replay)
            {
                return;
            }

            if (replay.Challenge is null)
            {
                return;
            }

            var mapName = TextFormatter.Deformat(replay.Challenge.MapName);
            var mapEnv = (string?)replay.Challenge.Collection;
            var mapBronzeTime = replay.Challenge.BronzeTime ?? throw new ImportantPropertyNullException("Bronze time is null.");
            var mapSilverTime = replay.Challenge.SilverTime ?? throw new ImportantPropertyNullException("Silver time is null.");
            var mapGoldTime = replay.Challenge.GoldTime ?? throw new ImportantPropertyNullException("Gold time is null.");
            var mapAuthorTime = replay.Challenge.AuthorTime ?? throw new ImportantPropertyNullException("Author time is null.");
            var mapAuthorScore = replay.Challenge.AuthorScore;
            var mapMode = replay.Challenge.Mode;
            var mapCarPure = replay.Challenge.PlayerModel?.Id;
            var mapCar = string.IsNullOrEmpty(mapCarPure) ? $"{mapEnv}Car" : mapCarPure;

            mapCar = mapCar switch
            {
                Constants.AlpineCar => Constants.SnowCar,
                Constants.American or Constants.SpeedCar => Constants.DesertCar,
                Constants.Rally => Constants.RallyCar,
                Constants.SportCar => Constants.IslandCar,
                _ => mapCar
            };

            var ghost = replay.GetGhosts(alsoInClips: false).FirstOrDefault();

            AutosaveDetails[autosaveMapUid] = new(
                replay.Time.Value,
                Score: ghost?.StuntScore,
                Respawns: ghost?.Respawns,
                mapName,
                mapEnv,
                mapCar,
                mapBronzeTime,
                mapSilverTime,
                mapGoldTime,
                mapAuthorTime,
                mapAuthorScore,
                mapMode);
        }
        catch (Exception ex)
        {
            throw new AutosaveScannerException($"Error while updating autosave details for {Path.GetFileName(autosavePath)}.", ex);
        }
    }

    /// <summary>
    /// Cleans up the autosave storage and, most importantly, restricts the engine from functionalities that require the autosaves (<see cref="HasAutosavesScanned"/>).
    /// </summary>
    public void ResetAutosaves()
    {
        AutosaveHeaders.Clear();
        AutosaveDetails.Clear();
        HasAutosavesScanned = false;
    }
}
