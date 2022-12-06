using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public class SessionData
{
    public DateTimeOffset StartedAt { get; set; }

    [YamlIgnore]
    public string StartedAtText { get; }

    public List<SessionDataMap> Maps { get; set; } = new();

    public int AuthorMedalCount => Maps.Count(m => string.Equals(m.Result, Constants.AuthorMedal));
    public int GoldMedalCount => Maps.Count(m => string.Equals(m.Result, Constants.GoldMedal));
    public int SkippedCount => Maps.Count(m => string.Equals(m.Result, Constants.Skipped));

    public SessionData() : this(DateTimeOffset.Now)
    {
        
    }

    public SessionData(DateTimeOffset startedAt)
    {
        StartedAt = startedAt;
        StartedAtText = startedAt.ToString("yyyy-MM-dd HH_mm_ss");
    }
}
