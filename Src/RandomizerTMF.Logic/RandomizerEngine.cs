using Microsoft.Extensions.Logging;

namespace RandomizerTMF.Logic;

public static partial class RandomizerEngine
{
    public static RandomizerConfig Config { get; }
    public static Dictionary<string, HashSet<string>> OfficialBlocks { get; }

    public static HttpClient Http { get; }

    public static string? TmForeverExeFilePath { get; set; }
    public static string? TmUnlimiterExeFilePath { get; set; }

    public static Session? CurrentSession { get; private set; }
    
    public static bool HasSessionRunning => CurrentSession is not null;
    
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

        Directory.CreateDirectory(FilePathManager.SessionsDirectoryPath);
        
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
                config = Yaml.Deserializer.Deserialize<RandomizerConfig>(reader);
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
        return Yaml.Deserializer.Deserialize<Dictionary<string, HashSet<string>>>(reader);
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

            if (FilePathManager.UserDataDirectoryPath != newUserDataDirectoryPath)
            {
                FilePathManager.UserDataDirectoryPath = newUserDataDirectoryPath;

                AutosaveScanner.ResetAutosaves();
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

    public static void SaveConfig()
    {
        SaveConfig(Config);
    }

    private static void SaveConfig(RandomizerConfig config)
    {
        Logger.LogInformation("Saving the config file...");
        
        File.WriteAllText(Constants.ConfigYml, Yaml.Serializer.Serialize(config));

        Logger.LogInformation("Config file saved.");
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

        CurrentSession = new Session(
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

        OnStatus("Ending the session...");

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
