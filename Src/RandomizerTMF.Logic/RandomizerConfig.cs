namespace RandomizerTMF.Logic;

public class RandomizerConfig
{
    public string? GameDirectory { get; set; }
    public string? DownloadedMapsDirectory { get; set; } = Constants.DownloadedMapsDirectory;
    public ModulesConfig Modules { get; set; } = new();
    public RandomizerRules Rules { get; set; } = new();
}
