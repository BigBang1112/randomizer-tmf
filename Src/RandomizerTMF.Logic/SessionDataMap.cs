namespace RandomizerTMF.Logic;

/// <summary>
/// Serialized session map object
/// </summary>
public class SessionDataMap : ISessionMap
{
    public required string Name { get; init; }
    public required string Uid { get; init; }
    public required string TmxLink { get; init; }
    public List<SessionDataReplay> Replays { get; set; } = [];
    public string? Result { get; set; }
    public TimeSpan? LastTimestamp { get; set; }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(Name);
        writer.Write(Uid);
        writer.Write(TmxLink);
        writer.Write(Replays.Count);
        foreach (var replay in Replays)
        {
            replay.Serialize(writer);
        }
        writer.Write(Result ?? "");
        writer.Write(LastTimestamp?.Ticks ?? -1);
    }

    public static SessionDataMap Deserialize(BinaryReader reader)
    {
        var map = new SessionDataMap
        {
            Name = reader.ReadString(),
            Uid = reader.ReadString(),
            TmxLink = reader.ReadString()
        };

        var replayCount = reader.ReadInt32();
        for (var i = 0; i < replayCount; i++)
        {
            map.Replays.Add(SessionDataReplay.Deserialize(reader));
        }

        map.Result = reader.ReadString();
        var lastTimestampTicks = reader.ReadInt64();
        map.LastTimestamp = lastTimestampTicks == -1 ? null : TimeSpan.FromTicks(lastTimestampTicks);

        return map;
    }
}
