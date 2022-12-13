using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public class SessionData
{
    public string? Version { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public RandomizerRules Rules { get; set; }

    [YamlIgnore]
    public string StartedAtText => StartedAt.ToString("yyyy-MM-dd HH_mm_ss");

    public List<SessionDataMap> Maps { get; set; } = new();

    public SessionData() : this(null, DateTimeOffset.Now, new())
    {
        
    }

    public SessionData(string? version, DateTimeOffset startedAt, RandomizerRules rules)
    {
        Version = version;
        StartedAt = startedAt;
        Rules = rules;
    }
}
