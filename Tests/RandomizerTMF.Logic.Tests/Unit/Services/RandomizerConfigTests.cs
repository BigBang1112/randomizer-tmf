using Microsoft.Extensions.Logging;
using Moq;
using RandomizerTMF.Logic.Services;
using System.IO.Abstractions.TestingHelpers;

namespace RandomizerTMF.Logic.Tests.Unit.Services;

public class RandomizerConfigTests
{
    [Fact]
    public void GetOrCreate_NoConfig_Create()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var fileSystem = new MockFileSystem();
        var defaultConfig = new RandomizerConfig();

        // Act
        var config = RandomizerConfig.GetOrCreate(mockLogger.Object, fileSystem);

        // Assert
        Assert.True(fileSystem.FileExists("Config.yml"));
        Assert.Equal(expected: defaultConfig.GameDirectory, actual: config.GameDirectory);
        Assert.Equal(expected: defaultConfig.DownloadedMapsDirectory, actual: config.DownloadedMapsDirectory);
        Assert.Equal(expected: defaultConfig.ReplayParseFailRetries, actual: config.ReplayParseFailRetries);
        Assert.Equal(expected: defaultConfig.ReplayParseFailDelayMs, actual: config.ReplayParseFailDelayMs);
        Assert.Equal(expected: defaultConfig.ReplayFileFormat, actual: config.ReplayFileFormat);
    }
    
    [Fact]
    public void GetOrCreate_ExistingConfig_Read()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "Config.yml", new MockFileData(@"GameDirectory: C:\GameDirectory") }
        });

        // Act
        var config = RandomizerConfig.GetOrCreate(mockLogger.Object, fileSystem);

        // Assert
        Assert.True(fileSystem.FileExists("Config.yml"));
        Assert.Equal(expected: @"C:\GameDirectory", actual: config.GameDirectory);
    }

    [Theory]
    [InlineData(@"GameDirectory: [C:\GameDirectory")]
    [InlineData("ReplayParseFailRetries: number")]
    public void GetOrCreate_CorruptedConfig_Overwrite(string configContent)
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "Config.yml", new MockFileData(configContent) }
        });
        var lastWriteTime = fileSystem.File.GetLastWriteTime("Config.yml");
        var defaultConfig = new RandomizerConfig();

        // Act
        var config = RandomizerConfig.GetOrCreate(mockLogger.Object, fileSystem);

        // Assert
        Assert.True(fileSystem.FileExists("Config.yml"));
        Assert.NotEqual(expected: lastWriteTime, fileSystem.File.GetLastWriteTime("Config.yml"));
        Assert.Equal(expected: defaultConfig.GameDirectory, actual: config.GameDirectory);
        Assert.Equal(expected: defaultConfig.DownloadedMapsDirectory, actual: config.DownloadedMapsDirectory);
        Assert.Equal(expected: defaultConfig.ReplayParseFailRetries, actual: config.ReplayParseFailRetries);
        Assert.Equal(expected: defaultConfig.ReplayParseFailDelayMs, actual: config.ReplayParseFailDelayMs);
        Assert.Equal(expected: defaultConfig.ReplayFileFormat, actual: config.ReplayFileFormat);
    }
}
