using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public class SessionData
{
    public DateTimeOffset StartedAt { get; }

    [YamlIgnore]
    public string StartedAtText { get; }

    public List<SessionDataMap> Maps { get; } = new();

    public SessionData(DateTimeOffset startedAt)
    {
        StartedAt = startedAt;
        StartedAtText = startedAt.ToString("yyyy-MM-dd HH_mm_ss");
    }
}
