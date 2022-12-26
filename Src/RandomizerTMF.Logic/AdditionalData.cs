using GBX.NET;
using Microsoft.Extensions.Logging;

namespace RandomizerTMF.Logic;

public interface IAdditionalData
{
    Dictionary<string, HashSet<Int3>> MapSizes { get; }
    Dictionary<string, HashSet<string>> OfficialBlocks { get; }
}

public class AdditionalData : IAdditionalData
{
    public Dictionary<string, HashSet<string>> OfficialBlocks { get; }
    public Dictionary<string, HashSet<Int3>> MapSizes { get; }

    public AdditionalData(ILogger logger)
    {
        logger.LogInformation("Loading official blocks...");
        OfficialBlocks = GetOfficialBlocks();

        logger.LogInformation("Loading map sizes...");
        MapSizes = GetMapSizes();
    }

    internal Dictionary<string, HashSet<string>> GetOfficialBlocks()
    {
        using var reader = new StreamReader(Constants.OfficialBlocksYml);
        return Yaml.Deserializer.Deserialize<Dictionary<string, HashSet<string>>>(reader);
    }

    internal Dictionary<string, HashSet<Int3>> GetMapSizes()
    {
        using var reader = new StreamReader(Constants.MapSizesYml);
        return Yaml.Deserializer.Deserialize<Dictionary<string, HashSet<Int3>>>(reader);
    }
}
