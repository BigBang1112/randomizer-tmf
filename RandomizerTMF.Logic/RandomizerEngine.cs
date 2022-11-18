using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using System.Collections.Concurrent;
using System.Diagnostics;
using TmEssentials;

namespace RandomizerTMF.Logic;

public static class RandomizerEngine
{
    private static string? userDataDirectoryPath;

    public static RandomizerConfig Config { get; }

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

    public static bool HasSessionRunning => CurrentSession is not null;
    public static bool HasAutosavesScanned { get; private set; }

    public static ConcurrentDictionary<string, CGameCtnReplayRecord> Autosaves { get; } = new();
    public static ConcurrentDictionary<string, string> AutosavePaths { get; } = new();
    public static ConcurrentDictionary<string, AutosaveDetails> AutosaveDetails { get; } = new();

    public static FileSystemWatcher AutosaveWatcher { get; }

    public static event Action? MapStarted;
    public static event Action? MapEnded;

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
    }

    private static void AutosaveCreatedOrChanged(object sender, FileSystemEventArgs e)
    {
        
    }

    private static RandomizerConfig GetOrCreateConfig()
    {
        if (File.Exists(Constants.ConfigYml))
        {
            using var reader = new StreamReader(Constants.ConfigYml);
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            return deserializer.Deserialize<RandomizerConfig>(reader);
        }

        var config = new RandomizerConfig();
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

    public static Task StartSessionAsync(Rules rules)
    {
        if (Config.GameDirectory is null)
        {
            return Task.CompletedTask;
        }

        CurrentSessionTokenSource = new CancellationTokenSource();
        CurrentSession = Task.Run(() => RunSessionAsync(rules, CurrentSessionTokenSource.Token), CurrentSessionTokenSource.Token);
        
        return Task.CompletedTask;
    }

    private static async Task RunSessionAsync(Rules rules, CancellationToken cancellationToken)
    {
        if (Config.GameDirectory is null)
        {
            throw new Exception("Game directory is null");
        }
        
        ///////// validate rules

        using var http = new HttpClient();
        
        while (true)
        {
            var mapSavePath = default(string);

            try
            {
                /////////// create urls based on rules

                using var randomResponse = await http.HeadAsync("https://tmuf.exchange/trackrandom", cancellationToken);

                randomResponse.EnsureSuccessStatusCode();

                var trackId = randomResponse.RequestMessage?.RequestUri?.Segments.Last();

                if (trackId is null)
                {
                    throw new Exception("Track ID is not available");
                }

                using var trackGbxResponse = await http.GetAsync($"https://tmuf.exchange/trackgbx/{trackId}", cancellationToken);

                using var stream = await trackGbxResponse.Content.ReadAsStreamAsync(cancellationToken);

                if (GameBox.ParseNode(stream) is not CGameCtnChallenge map)
                {
                    throw new Exception();
                }

                ////////////// validate the map

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
                continue;
            }

            if (mapSavePath is null)
            {
                continue;
            }

            OpenFileIngame(mapSavePath);

            AutosaveWatcher.EnableRaisingEvents = true;
            SkipTokenSource = new CancellationTokenSource();
            MapStarted?.Invoke();

            while (true)
            {
                await Task.Delay(50, cancellationToken);

                try
                {
                    await Task.Delay(50, SkipTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            StopTrackingCurrentSessionMap();
        }
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
        CurrentSessionTokenSource?.Cancel();

        if (CurrentSession is not null)
        {
            try
            {
                await CurrentSession;
            }
            catch (TaskCanceledException)
            {
                StopTrackingCurrentSessionMap();
                CurrentSession = null;
            }
        }
    }

    public static Task SkipMapAsync()
    {
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
