namespace RandomizerTMF.Logic;

public class SessionDataReplay
{
    public required string FileName { get; init; }
    public required TimeSpan Timestamp { get; init; }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(FileName);
        writer.Write(Timestamp.Ticks);
    }

    public static SessionDataReplay Deserialize(BinaryReader reader)
    {
        return new SessionDataReplay
        {
            FileName = reader.ReadString(),
            Timestamp = TimeSpan.FromTicks(reader.ReadInt64())
        };
    }
}
