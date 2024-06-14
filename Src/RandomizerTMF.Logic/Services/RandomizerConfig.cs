using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic.Services;

public interface IRandomizerConfig
{
    string? DownloadedMapsDirectory { get; set; }
    string? GameDirectory { get; set; }
    ModulesConfig Modules { get; set; }
    string? ReplayFileFormat { get; set; }
    int ReplayParseFailDelayMs { get; set; }
    int ReplayParseFailRetries { get; set; }
    RandomizerRules Rules { get; set; }
    bool DisableAutosaveDetailScan { get; set; }
    bool DisableAutoSkip { get; set; }
    DiscordRichPresenceConfig DiscordRichPresence { get; set; }
    bool TopSessions { get; set; }

    void Save();
}

public class RandomizerConfig : IRandomizerConfig
{
    private readonly ILogger? logger;
    private readonly IFileSystem? fileSystem;

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
    public bool DisableAutosaveDetailScan { get; set; }
    public bool DisableAutoSkip { get; set; }

    public DiscordRichPresenceConfig DiscordRichPresence { get; set; } = new();

    public bool TopSessions { get; set; }

    public RandomizerConfig()
    {

    }

    public RandomizerConfig(ILogger logger, IFileSystem fileSystem)
    {
        this.logger = logger;
        this.fileSystem = fileSystem;
    }

    /// <summary>
    /// This method should be ran only at the start of the randomizer engine.
    /// </summary>
    /// <returns></returns>
    public static RandomizerConfig GetOrCreate(ILogger logger, IFileSystem fileSystem)
    {
        var config = default(RandomizerConfig);

        if (fileSystem.File.Exists(Constants.ConfigYml) == true)
        {
            logger.LogInformation("Config file found, loading...");

            try
            {
                using var reader = fileSystem.File.OpenText(Constants.ConfigYml);
                config = Yaml.Deserializer.Deserialize<RandomizerConfig>(reader);
                typeof(RandomizerConfig).GetField(nameof(logger), BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(config, logger);
                typeof(RandomizerConfig).GetField(nameof(fileSystem), BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(config, fileSystem);
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
            config = new RandomizerConfig(logger, fileSystem);
        }

        config.Save();

        return config;
    }

    public void Save()
    {
        logger?.LogInformation("Saving the config file...");

        fileSystem?.File.WriteAllText(Constants.ConfigYml, Yaml.Serializer.Serialize(this));

        logger?.LogInformation("Config file saved.");
    }
}
