﻿namespace RandomizerTMF.Logic;

public class SessionDataReplay
{
    public required string FileName { get; init; }
    public required TimeSpan Timestamp { get; init; }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(FileName);
        writer.Write(Timestamp.Ticks);
    }
}
