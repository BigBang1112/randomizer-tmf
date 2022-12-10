namespace RandomizerTMF.Logic;

/// <summary>
/// Serialized session map object
/// </summary>
public class SessionDataMap : ISessionMap
{
    public required string Name { get; init; }
    public required string Uid { get; init; }
    public required string TmxLink { get; init; }
    public List<SessionDataReplay> Replays { get; set; } = new();
    public string? Result { get; set; }
    public TimeSpan? LastTimestamp { get; set; }
}
