using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using TmEssentials;

namespace RandomizerTMF.Logic;

public static class RandomizerEngine
{
    private static string? userDataDirectoryPath;

    public static RandomizerConfig Config { get; }
    public static RandomizerRules Rules { get; } = new();

    public static string? TmForeverExeFilePath { get; set; }
    public static string? TmUnlimiterExeFilePath { get; set; }

    public static string? UserDataDirectoryPath
    {
        get => userDataDirectoryPath;
        set
        {
            userDataDirectoryPath = value;
            
            AutosavesDirectoryPath = userDataDirectoryPath is null ? null : Path.Combine(userDataDirectoryPath, "Tracks", "Replays", "Autosaves");
            DownloadedDirectoryPath = userDataDirectoryPath is null ? null : Path.Combine(userDataDirectoryPath, "Tracks", "Challenges", "Downloaded", string.IsNullOrWhiteSpace(Config.DownloadedMapsDirectory) ? Constants.DownloadedMapsDirectory : Config.DownloadedMapsDirectory);

            AutosaveWatcher.Path = AutosavesDirectoryPath ?? "";
        }
    }

    public static string? AutosavesDirectoryPath { get; private set; }
    public static string? DownloadedDirectoryPath { get; private set; }

    public static Task? CurrentSession { get; private set; }
    public static CancellationTokenSource? CurrentSessionTokenSource { get; private set; }
    public static CancellationTokenSource? SkipTokenSource { get; private set; }
    public static CGameCtnChallenge? CurrentSessionMap { get; private set; }
    public static Stopwatch? CurrentSessionWatch { get; private set; }

    public static bool HasSessionRunning => CurrentSession is not null;
    public static bool HasAutosavesScanned { get; private set; }

    public static ConcurrentDictionary<string, CGameCtnReplayRecord> Autosaves { get; } = new();
    public static ConcurrentDictionary<string, string> AutosavePaths { get; } = new();
    public static ConcurrentDictionary<string, AutosaveDetails> AutosaveDetails { get; } = new();

    public static FileSystemWatcher AutosaveWatcher { get; }

    public static event Action? MapStarted;
    public static event Action? MapEnded;
    public static event Action<string> Status;

    static RandomizerEngine()
    {
        GBX.NET.Lzo.SetLzo(typeof(GBX.NET.LZO.MiniLZO));
        
        Config = GetOrCreateConfig();

        AutosaveWatcher = new FileSystemWatcher
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.Replay.gbx"
        };

        AutosaveWatcher.Created += AutosaveCreatedOrChanged;
        AutosaveWatcher.Changed += AutosaveCreatedOrChanged;

        Status += RandomizerEngineStatus;
    }

    private static void RandomizerEngineStatus(string obj)
    {
        // logging
    }

    private static void AutosaveCreatedOrChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            if (CurrentSessionMap is null)
            {
                // log warning
                return;
            }

            if (GameBox.ParseNode(e.FullPath) is not CGameCtnReplayRecord replay)
            {
                // log warning
                return;
            }

            if (CurrentSessionMap.MapInfo != replay.MapInfo)
            {
                // log warning
                return;
            }

            SkipTokenSource?.Cancel();
        }
        catch
        {
            
        }
    }

    private static RandomizerConfig GetOrCreateConfig()
    {
        var config = default(RandomizerConfig);

        if (File.Exists(Constants.ConfigYml))
        {
            using var reader = new StreamReader(Constants.ConfigYml);
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            config = deserializer.Deserialize<RandomizerConfig>(reader);
        }

        config ??= new RandomizerConfig();
        SaveConfig(config);

        return config;
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
            var newUserDataDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nadeoIni.UserSubDir);

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

    public static void ResetAutosaves()
    {
        Autosaves.Clear();
        AutosavePaths.Clear();
        AutosaveDetails.Clear();
        HasAutosavesScanned = false;
    }

    public static void SaveConfig()
    {
        SaveConfig(Config);
    }

    private static void SaveConfig(RandomizerConfig config)
    {
        var serializer = new YamlDotNet.Serialization.Serializer();

        File.WriteAllText(Constants.ConfigYml, serializer.Serialize(config));
    }

    public static bool ScanAutosaves()
    {
        if (AutosavesDirectoryPath is null)
        {
            throw new Exception("Cannot scan autosaves without a valid user data directory path.");
        }

        var anythingChanged = false;
        
        foreach (var file in Directory.EnumerateFiles(AutosavesDirectoryPath).AsParallel())
        {
            if (GameBox.ParseNodeHeader(file) is not CGameCtnReplayRecord replay || replay.MapInfo is null)
            {
                continue;
            }

            var mapUid = replay.MapInfo.Id;

            if (Autosaves.ContainsKey(mapUid))
            {
                continue;
            }

            Autosaves.TryAdd(mapUid, replay);
            AutosavePaths.TryAdd(mapUid, Path.GetFileName(file));

            anythingChanged = true;
        }

        HasAutosavesScanned = true;

        return anythingChanged;
    }

    public static void ScanDetailsFromAutosaves()
    {
        if (AutosavesDirectoryPath is null)
        {
            throw new Exception("Cannot update autosaves without a valid user data directory path.");
        }
        
        foreach (var autosave in Autosaves.Keys)
        {
            UpdateAutosaveDetail(autosave);
        }
    }

    private static void UpdateAutosaveDetail(string autosaveFileName)
    {
        var autosavePath = Path.Combine(AutosavesDirectoryPath!, AutosavePaths[autosaveFileName]);

        if (GameBox.ParseNode(autosavePath) is not CGameCtnReplayRecord { Time: not null } replay)
        {
            return;
        }

        try
        {
            if (replay.Challenge is null)
            {
                return;
            }

            var mapName = TextFormatter.Deformat(replay.Challenge.MapName);
            var mapEnv = replay.Challenge.Collection;
            var mapBronzeTime = replay.Challenge.TMObjective_BronzeTime ?? throw new Exception("Bronze time is null.");
            var mapSilverTime = replay.Challenge.TMObjective_SilverTime ?? throw new Exception("Silver time is null.");
            var mapGoldTime = replay.Challenge.TMObjective_GoldTime ?? throw new Exception("Gold time is null.");
            var mapAuthorTime = replay.Challenge.TMObjective_AuthorTime ?? throw new Exception("Author time is null.");
            var mapMode = replay.Challenge.Mode;

            AutosaveDetails[autosaveFileName] = new(replay.Time.Value, mapName, mapEnv, mapBronzeTime, mapSilverTime, mapGoldTime, mapAuthorTime, mapMode);
        }
        catch
        {
            
        }
    }

    public static Task StartSessionAsync(RandomizerRules rules)
    {
        if (Config.GameDirectory is null)
        {
            return Task.CompletedTask;
        }

        CurrentSessionWatch = new Stopwatch();
        CurrentSessionTokenSource = new CancellationTokenSource();
        CurrentSession = Task.Run(() => RunSessionAsync(rules, CurrentSessionTokenSource.Token), CurrentSessionTokenSource.Token);
        
        return Task.CompletedTask;
    }

    private static async Task RunSessionAsync(RandomizerRules rules, CancellationToken cancellationToken)
    {
        if (Config.GameDirectory is null)
        {
            throw new Exception("Game directory is null");
        }
        
        ///////// validate rules

        using var http = new HttpClient();

        try
        {
            while (true)
            {
                await PlayNewMapAsync(rules, http, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            ClearCurrentSession();
        }
    }

    private static void ClearCurrentSession()
    {
        StopTrackingCurrentSessionMap();

        CurrentSessionWatch?.Stop();
        CurrentSessionWatch = null;

        CurrentSession = null;
        CurrentSessionTokenSource = null;
    }

    private static async Task PlayNewMapAsync(RandomizerRules rules, HttpClient http, CancellationToken cancellationToken)
    {
        var mapSavePath = default(string);

        try
        {
            /////////// create urls based on rules
            
            Status("Fetching random track...");

            using var randomResponse = await http.HeadAsync("https://tmuf.exchange/trackrandom", cancellationToken);

            randomResponse.EnsureSuccessStatusCode();

            var trackId = randomResponse.RequestMessage?.RequestUri?.Segments.Last();

            if (trackId is null)
            {
                // log warning
                return;
            }

            Status($"Downloading track {trackId}...");

            using var trackGbxResponse = await http.GetAsync($"https://tmuf.exchange/trackgbx/{trackId}", cancellationToken);

            using var stream = await trackGbxResponse.Content.ReadAsStreamAsync(cancellationToken);
            
            Status("Parsing the map...");

            if (GameBox.ParseNode(stream) is not CGameCtnChallenge map)
            {
                // log warning
                return;
            }
            
            Status("Validating the map...");

            ////////////// validate the map
            
            Status("Saving the map...");

            var downloadedDir = DownloadedDirectoryPath;

            if (downloadedDir is null)
            {
                throw new Exception("Cannot update autosaves without a valid user data directory path.");
            }

            Directory.CreateDirectory(downloadedDir);

            var fileName = trackGbxResponse.Content.Headers.ContentDisposition?.FileName?.Trim('\"') ?? $"{map.MapUid}.Challenge.Gbx";

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            
            mapSavePath = Path.Combine(downloadedDir, fileName);

            File.WriteAllBytes(mapSavePath, await trackGbxResponse.Content.ReadAsByteArrayAsync(cancellationToken));

            CurrentSessionMap = map;
        }
        catch (HttpRequestException)
        {
            await Task.Delay(1000, cancellationToken);
            return;
        }

        if (mapSavePath is null)
        {
            return;
        }

        if (CurrentSessionWatch is null)
        {
            throw new UnreachableException("CurrentSessionWatch is null");
        }

        Status("Starting the map...");

        CurrentSessionWatch.Start();

        OpenFileIngame(mapSavePath);

        AutosaveWatcher.EnableRaisingEvents = true;
        SkipTokenSource = new CancellationTokenSource();
        MapStarted?.Invoke();
        
        Status("Playing the map...");

        while (!SkipTokenSource.IsCancellationRequested)
        {
            if (CurrentSessionWatch.Elapsed >= Rules.TimeLimit)
            {
                if (CurrentSessionTokenSource is null)
                {
                    throw new UnreachableException("CurrentSessionTokenSource is null");
                }

                CurrentSessionTokenSource.Cancel();
                cancellationToken.ThrowIfCancellationRequested();
            }

            await Task.Delay(20, cancellationToken);
        }

        CurrentSessionWatch.Stop();

        Status("Ending the map...");

        StopTrackingCurrentSessionMap();
    }

    private static void StopTrackingCurrentSessionMap()
    {
        SkipTokenSource = null;
        AutosaveWatcher.EnableRaisingEvents = false;
        CurrentSessionMap = null;
        MapEnded?.Invoke();
    }
        
    public static async Task EndSessionAsync()
    {
        Status("Ending the session...");

        CurrentSessionTokenSource?.Cancel();

        if (CurrentSession is null)
        {
            return;
        }
        
        try
        {
            await CurrentSession;
        }
        catch (TaskCanceledException)
        {
            ClearCurrentSession();
        }
    }

    public static Task SkipMapAsync()
    {
        Status("Skipping the map...");
        SkipTokenSource?.Cancel();
        return Task.CompletedTask;
    }

    public static void OpenFileIngame(string filePath)
    {
        if (Config.GameDirectory is null)
        {
            throw new Exception("Game directory is null");
        }

        var startInfo = new ProcessStartInfo(Path.Combine(Config.GameDirectory, Constants.TmForeverExe), $"/useexedir /singleinst /file=\"{filePath}\"");

        var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();
    }

    public static void OpenAutosaveIngame(string fileName)
    {
        if (AutosavesDirectoryPath is null)
        {
            throw new Exception("Cannot open an autosave ingame without a valid user data directory path.");
        }

        OpenFileIngame(Path.Combine(AutosavesDirectoryPath, fileName));
    }

    public static void Exit()
    {
        Environment.Exit(0);
    }
}
