namespace RandomizerTMF.Logic;

public static class RandomizerEngine
{
    public static RandomizerConfig Config { get; }

    public static string? UserDataDirectoryPath { get; set; }
    public static string? TmForeverExeFilePath { get; set; }
    public static string? TmUnlimiterExeFilePath { get; set; }

    static RandomizerEngine()
    {
        Config = GetOrCreateConfig();
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
            UserDataDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), nadeoIni.UserSubDir);
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
        var serializer = new YamlDotNet.Serialization.Serializer();

        File.WriteAllText(Constants.ConfigYml, serializer.Serialize(config));
    }

    public static void Exit()
    {
        Environment.Exit(0);
    }
}
