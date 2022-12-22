using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Exceptions;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic.Exceptions;
using RandomizerTMF.Logic.TypeConverters;
using System.Collections.Concurrent;
using System.Diagnostics;
using TmEssentials;
using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public static partial class RandomizerEngine
{
    private static string? userDataDirectoryPath;
    private static bool hasAutosavesScanned;
    
    public static ISerializer YamlSerializer { get; } = new SerializerBuilder()
        .WithTypeConverter(new DateOnlyConverter())
        .WithTypeConverter(new DateTimeOffsetConverter())
        .WithTypeConverter(new TimeInt32Converter())
        .Build();

    public static IDeserializer YamlDeserializer { get; } = new DeserializerBuilder()
        .WithTypeConverter(new DateOnlyConverter())
        .WithTypeConverter(new DateTimeOffsetConverter())
        .WithTypeConverter(new TimeInt32Converter())
        .IgnoreUnmatchedProperties()
        .Build();

    public static RandomizerConfig Config { get; }
    public static Dictionary<string, HashSet<string>> OfficialBlocks { get; }

    public static HttpClient Http { get; }

    public static string? TmForeverExeFilePath { get; set; }
    public static string? TmUnlimiterExeFilePath { get; set; }

    /// <summary>
    /// General directory of the user data. It also sets the <see cref="AutosavesDirectoryPath"/>, <see cref="DownloadedDirectoryPath"/>, and <see cref="AutosaveWatcher"/> path with it.
    /// </summary>
    public static string? UserDataDirectoryPath
    {
        get => userDataDirectoryPath;
        set
        {
            userDataDirectoryPath = value;

            AutosavesDirectoryPath = userDataDirectoryPath is null ? null : Path.Combine(userDataDirectoryPath, Constants.Tracks, Constants.Replays, Constants.Autosaves);
            DownloadedDirectoryPath = userDataDirectoryPath is null ? null : Path.Combine(userDataDirectoryPath, Constants.Tracks, Constants.Challenges, Constants.Downloaded, string.IsNullOrWhiteSpace(Config.DownloadedMapsDirectory) ? Constants.DownloadedMapsDirectory : Config.DownloadedMapsDirectory);

            AutosaveWatcher.Path = AutosavesDirectoryPath ?? "";
        }
    }

    public static string? AutosavesDirectoryPath { get; private set; }
    public static string? DownloadedDirectoryPath { get; private set; }
    public static string SessionsDirectoryPath => Constants.Sessions;

    public static CurrentSession? CurrentSession { get; private set; }
    
    public static bool HasSessionRunning => CurrentSession is not null;

    /// <summary>
    /// If the map UIDs of the autosaves have been fully stored to the <see cref="AutosaveHeaders"/> and <see cref="AutosavePaths"/> dictionaries.
    /// This property is required to be true in order to start a new session. It also handles the state of <see cref="AutosaveWatcher"/>, to catch other autosaves that might be created while the program is running.
    /// </summary>
    public static bool HasAutosavesScanned
    {
        get => hasAutosavesScanned;
        private set
        {
            hasAutosavesScanned = value;
            AutosaveWatcher.EnableRaisingEvents = value;
        }
    }

    public static ConcurrentDictionary<string, AutosaveHeader> AutosaveHeaders { get; } = new();
    public static ConcurrentDictionary<string, AutosaveDetails> AutosaveDetails { get; } = new();

    public static FileSystemWatcher AutosaveWatcher { get; }


    public static ILogger Logger { get; private set; }
    public static StreamWriter LogWriter { get; private set; }
    public static bool SessionEnding { get; private set; }

    public static string? Version { get; } = typeof(RandomizerEngine).Assembly.GetName().Version?.ToString(3);

    public static event Action<string>? Status;
    public static event Action? MapStarted;
    public static event Action? MapEnded;
    public static event Action? MapSkip;
    public static event Action? MedalUpdate;

    static RandomizerEngine()
    {
        LogWriter = new StreamWriter(Constants.RandomizerTmfLog, append: true)
        {
            AutoFlush = true
        };

        Logger = new LoggerToFile(LogWriter);
        
        Logger.LogInformation("Starting Randomizer Engine...");

        Directory.CreateDirectory(SessionsDirectoryPath);
        
        Logger.LogInformation("Predefining LZO algorithm...");

        GBX.NET.Lzo.SetLzo(typeof(GBX.NET.LZO.MiniLZO));

        Logger.LogInformation("Loading config...");

        Config = GetOrCreateConfig();

        Logger.LogInformation("Loading official blocks...");

        OfficialBlocks = GetOfficialBlocks();

        Logger.LogInformation("Preparing HTTP client...");

        var socketHandler = new SocketsHttpHandler()
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(1),
        };
        
        Http = new HttpClient(socketHandler);
        Http.DefaultRequestHeaders.UserAgent.TryParseAdd($"Randomizer TMF {Version}");

        Logger.LogInformation("Preparing general events...");

        AutosaveWatcher = new FileSystemWatcher
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.Replay.gbx"
        };

        AutosaveWatcher.Changed += AutosaveCreatedOrChanged;
        
        Logger.LogInformation("Randomizer TMF initialized.");
    }

    public static void OnStatus(string status)
    {
        // Status kind of logging
        // Unlike regular logs, these are shown to the user in a module, while also written to the log file in its own way
        Logger.LogInformation("STATUS: {status}", status);

        Status?.Invoke(status);
    }

    public static void OnMapStarted() => MapStarted?.Invoke();
    public static void OnMapEnded() => MapEnded?.Invoke();
    public static void OnMapSkip() => MapSkip?.Invoke();
    public static void OnMedalUpdate() => MedalUpdate?.Invoke();

    private static DateTime lastAutosaveUpdate = DateTime.MinValue;

    private static void AutosaveCreatedOrChanged(object sender, FileSystemEventArgs e)
    {
        // Hack to fix the issue of this event sometimes running twice
        var lastWriteTime = File.GetLastWriteTime(e.FullPath);

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

                Logger.LogInformation("Analyzing a new file {autosavePath} in autosaves folder...", e.FullPath);

                if (GameBox.ParseNode(e.FullPath) is not CGameCtnReplayRecord r)
                {
                    Logger.LogWarning("Found file {file} that is not a replay.", e.FullPath);
                    return;
                }

                if (r.MapInfo is null)
                {
                    Logger.LogWarning("Found replay {file} that has no map info.", e.FullPath);
                    return;
                }

                AutosaveHeaders.TryAdd(r.MapInfo.Id, new AutosaveHeader(Path.GetFileName(e.FullPath), r));

                replay = r;
            }
            catch (Exception ex)
            {
                retryCounter++;
                
                Logger.LogError(ex, "Error while analyzing a new file {autosavePath} in autosaves folder (retry {counter}/{maxRetries}).",
                    e.FullPath, retryCounter, Config.ReplayParseFailRetries);

                if (retryCounter >= Config.ReplayParseFailRetries)
                {
                    return;
                }

                Thread.Sleep(Config.ReplayParseFailDelayMs);

                continue;
            }

            break;
        }

        if (CurrentSession is null)
        {
            Logger.LogInformation("No session is running, ending here.");
            return;
        }

        CurrentSession.AutosaveCreatedOrChanged(e.FullPath, replay);
    }

    /// <summary>
    /// This method should be ran only at the start of the randomizer engine.
    /// </summary>
    /// <returns></returns>
    private static RandomizerConfig GetOrCreateConfig()
    {
        var config = default(RandomizerConfig);

        if (File.Exists(Constants.ConfigYml))
        {
            Logger.LogInformation("Config file found, loading...");

            try
            {
                using var reader = new StreamReader(Constants.ConfigYml);
                config = YamlDeserializer.Deserialize<RandomizerConfig>(reader);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error while deserializing the config file ({configPath}).", Constants.ConfigYml);
            }
        }

        if (config is null)
        {
            Logger.LogInformation("Config file not found or is corrupted, creating a new one...");
            config = new RandomizerConfig();
        }

        SaveConfig(config);

        return config;
    }
    
    private static Dictionary<string, HashSet<string>> GetOfficialBlocks()
    {
        using var reader = new StreamReader(Constants.OfficialBlocksYml);
        return YamlDeserializer.Deserialize<Dictionary<string, HashSet<string>>>(reader);
    }

    public static GameDirInspectResult UpdateGameDirectory(string gameDirectoryPath)
    {
        Config.GameDirectory = gameDirectoryPath;

        var nadeoIniFilePath = Path.Combine(gameDirectoryPath, Constants.NadeoIni);
        var tmForeverExeFilePath = Path.Combine(gameDirectoryPath, Constants.TmForeverExe);
        var tmUnlimiterExeFilePath = Path.Combine(gameDirectoryPath, Constants.TmInifinityExe);

        var nadeoIniException = default(Exception);
        var tmForeverExeException = default(Exception);
        var tmUnlimiterExeException = default(Exception);

        try
        {
            var nadeoIni = NadeoIni.Parse(nadeoIniFilePath);
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var newUserDataDirectoryPath = Path.Combine(myDocuments, nadeoIni.UserSubDir);

            if (UserDataDirectoryPath != newUserDataDirectoryPath)
            {
                UserDataDirectoryPath = newUserDataDirectoryPath;

                ResetAutosaves();
            }
        }
        catch (Exception ex)
        {
            nadeoIniException = ex;
        }

        try
        {
            using var fs = File.OpenRead(tmForeverExeFilePath);
            TmForeverExeFilePath = tmForeverExeFilePath;
        }
        catch (Exception ex)
        {
            tmForeverExeException = ex;
        }

        try
        {
            using var fs = File.OpenRead(tmUnlimiterExeFilePath);
            TmUnlimiterExeFilePath = tmUnlimiterExeFilePath;
        }
        catch (Exception ex)
        {
            tmUnlimiterExeException = ex;
        }

        return new GameDirInspectResult(nadeoIniException, tmForeverExeException, tmUnlimiterExeException);
    }

    /// <summary>
    /// Cleans up the autosave storage and, most importantly, restricts the engine from functionalities that require the autosaves (<see cref="HasAutosavesScanned"/>).
    /// </summary>
    public static void ResetAutosaves()
    {
        AutosaveHeaders.Clear();
        AutosaveDetails.Clear();
        HasAutosavesScanned = false;
    }

    public static void SaveConfig()
    {
        SaveConfig(Config);
    }

    private static void SaveConfig(RandomizerConfig config)
    {
        Logger.LogInformation("Saving the config file...");
        
        File.WriteAllText(Constants.ConfigYml, YamlSerializer.Serialize(config));

        Logger.LogInformation("Config file saved.");
    }

    /// <summary>
    /// Scans the autosaves, which is required before running the session, to avoid already played maps.
    /// </summary>
    /// <returns>True if anything changed.</returns>
    /// <exception cref="ImportantPropertyNullException"></exception>
    public static bool ScanAutosaves()
    {
        if (AutosavesDirectoryPath is null)
        {
            throw new ImportantPropertyNullException("Cannot scan autosaves without a valid user data directory path.");
        }

        var anythingChanged = false;

        foreach (var file in Directory.EnumerateFiles(AutosavesDirectoryPath).AsParallel())
        {
            try
            {
                if (GameBox.ParseNodeHeader(file) is not CGameCtnReplayRecord replay || replay.MapInfo is null)
                {
                    continue;
                }

                var mapUid = replay.MapInfo.Id;

                if (AutosaveHeaders.ContainsKey(mapUid))
                {
                    continue;
                }

                AutosaveHeaders.TryAdd(mapUid, new AutosaveHeader(Path.GetFileName(file), replay));

                anythingChanged = true;
            }
            catch (NotAGbxException)
            {
                // do nothing
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Gbx error found in the Autosaves folder when reading the header.");
            }
        }

        HasAutosavesScanned = true;

        return anythingChanged;
    }

    /// <summary>
    /// Scans autosave details (mostly the map inside the replay and getting its info) that are then used to display more detailed information about a list of maps. Only some replay properties are stored to value memory.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public static void ScanDetailsFromAutosaves()
    {
        if (AutosavesDirectoryPath is null)
        {
            throw new Exception("Cannot update autosaves without a valid user data directory path.");
        }

        foreach (var autosave in AutosaveHeaders.Keys)
        {
            try
            {
                UpdateAutosaveDetail(autosave);
            }
            catch (Exception ex)
            {
                // this happens in async context, logger may not be safe for that, some errors may get lost
                Debug.WriteLine("Exception during autosave detail scan: " + ex);
            }
        }
    }

    private static void UpdateAutosaveDetail(string autosaveFileName)
    {
        var autosavePath = Path.Combine(AutosavesDirectoryPath!, AutosaveHeaders[autosaveFileName].FilePath); // Forgive because it's a private method

        if (GameBox.ParseNode(autosavePath) is not CGameCtnReplayRecord { Time: not null } replay)
        {
            return;
        }
        
        if (replay.Challenge is null)
        {
            return;
        }

        var mapName = TextFormatter.Deformat(replay.Challenge.MapName);
        var mapEnv = (string)replay.Challenge.Collection;
        var mapBronzeTime = replay.Challenge.TMObjective_BronzeTime ?? throw new ImportantPropertyNullException("Bronze time is null.");
        var mapSilverTime = replay.Challenge.TMObjective_SilverTime ?? throw new ImportantPropertyNullException("Silver time is null.");
        var mapGoldTime = replay.Challenge.TMObjective_GoldTime ?? throw new ImportantPropertyNullException("Gold time is null.");
        var mapAuthorTime = replay.Challenge.TMObjective_AuthorTime ?? throw new ImportantPropertyNullException("Author time is null.");
        var mapAuthorScore = replay.Challenge.AuthorScore ?? throw new ImportantPropertyNullException("AuthorScore is null.");
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

        var ghost = replay.GetGhosts().FirstOrDefault();

        AutosaveDetails[autosaveFileName] = new(
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

    /// <summary>
    /// Starts the randomizer session by creating a new <see cref="Session"/> that will handle randomization on different thread from the UI thread.
    /// </summary>
    public static void StartSession()
    {
        if (Config.GameDirectory is null)
        {
            return;
        }

        CurrentSession = new CurrentSession(
            new MapDownloader(Config, Http, Logger),
            Config,
            new TMForever(Config, Logger),
            Http,
            Logger);
        CurrentSession.Start();
    }

    /// <summary>
    /// Does the cleanup of the session so that the new one can be instantiated without issues.
    /// </summary>
    private static void ClearCurrentSession()
    {
        CurrentSession?.Stop();
        CurrentSession = null;

        Logger = new LoggerToFile(LogWriter);
    }

    /// <summary>
    /// MANUAL end of session.
    /// </summary>
    /// <returns></returns>
    public static async Task EndSessionAsync()
    {
        if (SessionEnding)
        {
            return;
        }

        SessionEnding = true;

        Status("Ending the session...");

        if (CurrentSession is null)
        {
            return;
        }

        CurrentSession.TokenSource.Cancel();

        try
        {
            if (CurrentSession.Task is not null)
            {
                await CurrentSession.Task; // Kindly waits until the session considers it was cancelled. ClearCurrentSession is called within it.
            }
        }
        catch (TaskCanceledException)
        {
            ClearCurrentSession(); // Just an ensure
        }

        SessionEnding = false;
    }

    public static void Exit()
    {
        Logger.LogInformation("Exiting...");
        FlushLog();
        Environment.Exit(0);
    }

    public static void FlushLog()
    {
        LogWriter.Flush();
    }

    public static async Task FlushLogAsync()
    {
        await LogWriter.FlushAsync();
    }
}
