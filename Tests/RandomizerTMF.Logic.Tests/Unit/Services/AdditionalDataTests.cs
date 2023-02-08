using GBX.NET;
using Microsoft.Extensions.Logging;
using Moq;
using RandomizerTMF.Logic.Services;
using System.IO.Abstractions.TestingHelpers;

namespace RandomizerTMF.Logic.Tests.Unit.Services;

public class AdditionalDataTests
{
    [Fact]
    public void Constructor()
    {
        var logger = new Mock<ILogger>();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "OfficialBlocks.yml", new MockFileData("Rally:\n- SomeBlock") },
            { "MapSizes.yml", new MockFileData("Rally:\n- [1, 2, 3]") }
        });
        var additionalData = new AdditionalData(logger.Object, fileSystem);

        var officialBlocks = additionalData.OfficialBlocks;
        var mapSizes = additionalData.MapSizes;

        Assert.NotEmpty(officialBlocks);
        Assert.NotEmpty(mapSizes);
        Assert.Equal(expected: "SomeBlock", actual: officialBlocks["Rally"].First());
        Assert.Equal(expected: new Int3(1, 2, 3), actual: mapSizes["Rally"].First());
    }
}
