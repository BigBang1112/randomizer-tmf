using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public class SessionData
{
    public DateTimeOffset StartedAt { get; set; }
    public RandomizerRules Rules { get; set; }

    [YamlIgnore]
    public string StartedAtText { get; }

    public List<SessionDataMap> Maps { get; set; } = new();

    public SessionData() : this(DateTimeOffset.Now, new())
    {
        
    }

    public SessionData(DateTimeOffset startedAt, RandomizerRules rules)
    {
        StartedAt = startedAt;
        StartedAtText = startedAt.ToString("yyyy-MM-dd HH_mm_ss");
        Rules = rules;
    }
}
