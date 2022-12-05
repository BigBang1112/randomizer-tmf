namespace RandomizerTMF.Logic;

public class RandomizerRules
{
    public TimeSpan TimeLimit { get; set; } = TimeSpan.FromHours(1);
    public bool NoUnlimiter { get; set; } = true;
    
    public RequestRules RequestRules { get; init; } = new()
    {
        Site = ESite.TMNF,
        PrimaryType = EPrimaryType.Race
    };
}
