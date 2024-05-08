using TmEssentials;

namespace RandomizerTMF.Logic;

public class RandomizerRules
{
    public TimeSpan TimeLimit { get; set; } = TimeSpan.FromHours(1);
    public bool NoUnlimiter { get; set; } = true;
    
    public RequestRules RequestRules { get; init; } = new()
    {
        Site = ESite.TMNF,
        PrimaryType = EPrimaryType.Race,
        AuthorTimeMax = TimeInt32.FromMinutes(3)
    };

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(TimeLimit.Ticks);
        writer.Write(NoUnlimiter);
        RequestRules.Serialize(writer);
    }
}
