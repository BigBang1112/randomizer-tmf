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

    public Dictionary<ESite, HashSet<uint>> BannedMaps { get; init; } = [];

    public void Serialize(BinaryWriter writer, int version)
    {
        writer.Write(TimeLimit.Ticks);
        writer.Write(NoUnlimiter);
        RequestRules.Serialize(writer, version);

        if (version < 1)
		{
			return;
		}

        writer.Write(BannedMaps.Count);
        foreach (var (site, ids) in BannedMaps)
		{
			writer.Write((int)site);
			writer.Write(ids.Count);
			foreach (var id in ids)
			{
				writer.Write(id);
			}
		}
    }

    public void Deserialize(BinaryReader r, int version)
    {
        TimeLimit = TimeSpan.FromTicks(r.ReadInt64());
        NoUnlimiter = r.ReadBoolean();
        RequestRules.Deserialize(r, version);

        if (version < 1)
		{
			return;
		}

        var count = r.ReadInt32();
        BannedMaps.Clear();
        for (var i = 0; i < count; i++)
		{
			var site = (ESite)r.ReadInt32();
            var mapCount = r.ReadInt32();
			var ids = new HashSet<uint>(mapCount);
			for (var j = 0; j < mapCount; j++)
			{
				ids.Add(r.ReadUInt32());
			}
			BannedMaps.Add(site, ids);
		}
    }
}
