namespace RandomizerTMF.Logic;

public static class RandomizerEngine
{
    private const string configYml = "config.yml";

    public static RandomizerConfig Config { get; }

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

    public static void Exit()
    {
        Environment.Exit(0);
    }
}
