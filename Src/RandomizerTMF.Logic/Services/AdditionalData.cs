using GBX.NET;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace RandomizerTMF.Logic.Services;

public interface IAdditionalData
{
    Dictionary<string, HashSet<Int3>> MapSizes { get; }
    Dictionary<string, HashSet<string>> OfficialBlocks { get; }
}

public class AdditionalData : IAdditionalData
{
    private readonly IFileSystem fileSystem;

    public Dictionary<string, HashSet<string>> OfficialBlocks { get; }
    public Dictionary<string, HashSet<Int3>> MapSizes { get; }

    public AdditionalData(ILogger logger, IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
        
        logger.LogInformation("Loading official blocks...");
        OfficialBlocks = GetOfficialBlocks();

        logger.LogInformation("Loading map sizes...");
        MapSizes = GetMapSizes();
    }

    private Dictionary<string, HashSet<string>> GetOfficialBlocks()
    {
        using var reader = fileSystem.File.OpenText(Constants.OfficialBlocksYml);
        return Yaml.Deserializer.Deserialize<Dictionary<string, HashSet<string>>>(reader);
    }

    private Dictionary<string, HashSet<Int3>> GetMapSizes()
    {
        using var reader = fileSystem.File.OpenText(Constants.MapSizesYml);
        return Yaml.Deserializer.Deserialize<Dictionary<string, HashSet<Int3>>>(reader);
    }
}
