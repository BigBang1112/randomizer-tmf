using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public class SessionData
{
    public DateTimeOffset StartedAt { get; set; }

    [YamlIgnore]
    public string StartedAtText { get; }

    public List<SessionDataMap> Maps { get; set; } = new();

    public SessionData() : this(DateTimeOffset.Now)
    {
        
    }

    public SessionData(DateTimeOffset startedAt)
    {
        StartedAt = startedAt;
        StartedAtText = startedAt.ToString("yyyy-MM-dd HH_mm_ss");
    }
}
