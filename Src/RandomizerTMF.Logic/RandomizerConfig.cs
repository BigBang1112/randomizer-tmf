﻿using Microsoft.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public class RandomizerConfig
{
    private readonly ILogger? logger;

    public string? GameDirectory { get; set; }
    public string? DownloadedMapsDirectory { get; set; } = Constants.DownloadedMapsDirectory;
    
    [YamlMember(Order = 998)]
    public ModulesConfig Modules { get; set; } = new();

    [YamlMember(Order = 999)]
    public RandomizerRules Rules { get; set; } = new();
    
    /// <summary>
    /// {0} is the map name, {1} is the replay score (example: 9'59''59 in Race/Puzzle or 999_9'59''59 in Platform/Stunts), {2} is the player login.
    /// </summary>
    public string? ReplayFileFormat { get; set; } = Constants.DefaultReplayFileFormat;

    public int ReplayParseFailRetries { get; set; } = 10;
    public int ReplayParseFailDelayMs { get; set; } = 50;

    public RandomizerConfig()
    {

    }

    public RandomizerConfig(ILogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// This method should be ran only at the start of the randomizer engine.
    /// </summary>
    /// <returns></returns>
    public static RandomizerConfig GetOrCreate(ILogger logger)
    {
        var config = default(RandomizerConfig);

        if (File.Exists(Constants.ConfigYml))
        {
            logger.LogInformation("Config file found, loading...");

            try
            {
                using var reader = new StreamReader(Constants.ConfigYml);
                config = Yaml.Deserializer.Deserialize<RandomizerConfig>(reader);
            }
            catch (YamlException ex)
            {
                logger.LogWarning(ex.InnerException, "Error while deserializing the config file ({configPath}; [{start}] - [{end}]).", Constants.ConfigYml, ex.Start, ex.End);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error while deserializing the config file ({configPath}).", Constants.ConfigYml);
            }
        }

        if (config is null)
        {
            logger.LogInformation("Config file not found or is corrupted, creating a new one...");
            config = new RandomizerConfig(logger);
        }

        config.Save();

        return config;
    }

    public void Save()
    {
        logger?.LogInformation("Saving the config file...");

        File.WriteAllText(Constants.ConfigYml, Yaml.Serializer.Serialize(this));

        logger?.LogInformation("Config file saved.");
    }
}
