using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public class RandomizerConfig
{
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
}
