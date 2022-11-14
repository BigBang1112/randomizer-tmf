namespace RandomizerTMF.Logic;

public static class RandomizerEngine
{
    private const string configYml = "config.yml";
    private const string nadeoIni = "Nadeo.ini";
    private const string tmForeverExe = "TmForever.exe";
    private const string tmInifinityExe = "TmInfinity.exe";

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
        if (File.Exists(configYml))
        {
            using var reader = new StreamReader(configYml);
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            return deserializer.Deserialize<RandomizerConfig>(reader);
        }
        
        var config = new RandomizerConfig();
        var serializer = new YamlDotNet.Serialization.Serializer();
        
        File.WriteAllText(configYml, serializer.Serialize(config));

        return config;
    }

    public static GameDirInspectResult UpdateGameDirectory(string gameDirectoryPath)
    {
        var nadeoIniFilePath = Path.Combine(gameDirectoryPath, nadeoIni);
        var tmForeverExeFilePath = Path.Combine(gameDirectoryPath, tmForeverExe);
        var tmUnlimiterExeFilePath = Path.Combine(gameDirectoryPath, tmInifinityExe);

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
            var tmForeverFileInfo = new FileInfo(tmForeverExeFilePath);

            if (tmForeverFileInfo.Exists)
            {
                TmForeverExeFilePath = tmForeverExeFilePath;
            }
            else
            {
                throw new FileNotFoundException($"{tmForeverExe} not found");
            }
        }
        catch (Exception ex)
        {
            tmForeverExeException = ex;
        }
        
        try
        {
            var tmUnlimiterFileInfo = new FileInfo(tmUnlimiterExeFilePath);

            if (tmUnlimiterFileInfo.Exists)
            {
                TmUnlimiterExeFilePath = tmUnlimiterExeFilePath;
            }
            else
            {
                throw new FileNotFoundException($"{tmInifinityExe} not found");
            }
        }
        catch (Exception ex)
        {
            tmUnlimiterExeException = ex;
        }

        return new GameDirInspectResult(nadeoIniException, tmForeverExeException, tmUnlimiterExeException);
    }

    public static void Exit()
    {
        Environment.Exit(0);
    }
}
