using GBX.NET;
using GBX.NET.Engines.Game;
using Microsoft.Extensions.Logging;
using Moq;
using RandomizerTMF.Logic.Exceptions;
using RandomizerTMF.Logic.Services;
using System.Collections.Immutable;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using TmEssentials;
using YamlDotNet.Core;

namespace RandomizerTMF.Logic.Tests.Unit.Services;

public class AutosaveScannerTests
{
    private readonly char slash = Path.DirectorySeparatorChar;

    [Fact]
    public void HasAutosavesScanned_Get_DefaultEqualsFalse()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var gbx = Mock.Of<IGbxService>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var logger = Mock.Of<ILogger>();

        var service = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, gbx, logger);
        
        // Act & Assert
        Assert.False(service.HasAutosavesScanned);
    }

    [Fact]
    public void HasAutosavesScanned_Set_EnablesRaisingEvents()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var gbx = Mock.Of<IGbxService>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var logger = Mock.Of<ILogger>();

        var service = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, gbx, logger);

        // Act
        service.HasAutosavesScanned = true;

        // Assert
        Assert.True(service.HasAutosavesScanned);
        Assert.True(watcher.EnableRaisingEvents);
    }

    [Fact]
    public void UserDataDirectoryPathUpdated_UserDataDirectoryPathSet_SetsWatcherPath()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var gbx = Mock.Of<IGbxService>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var logger = Mock.Of<ILogger>();

        var expected = $"C:{slash}UserData{slash}Tracks{slash}Replays{slash}Autosaves";
        
        var service = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, gbx, logger);

        // Act
        service.UserDataDirectoryPathUpdated();

        // Assert
        Assert.Equal(expected, actual: watcher.Path);
    }

    [Fact]
    public void UserDataDirectoryPathUpdated_UserDataDirectoryPathNull_SetsWatcherPath()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var gbx = Mock.Of<IGbxService>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var logger = Mock.Of<ILogger>();

        var expected = "";

        var service = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, gbx, logger);

        // Act
        service.UserDataDirectoryPathUpdated();

        // Assert
        Assert.Equal(expected, actual: watcher.Path);
    }

    [Fact]
    public void ResetAutosaves_ResetsAutosaveData()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var gbx = Mock.Of<IGbxService>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, gbx, logger);
        scanner.AutosaveHeaders.TryAdd("uid", new AutosaveHeader("C:/Replay.Gbx", new CGameCtnReplayRecord()));
        scanner.AutosaveHeaders.TryAdd("uid2", new AutosaveHeader("C:/Replay2.Gbx", new CGameCtnReplayRecord()));
        scanner.AutosaveDetails.TryAdd("uid", new AutosaveDetails(TimeInt32.Zero, null, null, null, null, null, TimeInt32.Zero, TimeInt32.Zero, TimeInt32.Zero, TimeInt32.Zero, 0, null));
        scanner.AutosaveDetails.TryAdd("uid2", new AutosaveDetails(TimeInt32.Zero, null, null, null, null, null, TimeInt32.Zero, TimeInt32.Zero, TimeInt32.Zero, TimeInt32.Zero, 0, null));
        scanner.HasAutosavesScanned = true;

        // Act
        scanner.ResetAutosaves();

        // Assert
        Assert.False(scanner.HasAutosavesScanned);
        Assert.Empty(scanner.AutosaveHeaders);
        Assert.Empty(scanner.AutosaveDetails);
    }

    [Fact]
    public void ScanAutosaves_NullAutosavesDirectoryPath_Throws()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var gbx = Mock.Of<IGbxService>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var logger = Mock.Of<ILogger>();

        var service = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, gbx, logger);

        // Act & Assert
        Assert.Throws<ImportantPropertyNullException>(() => service.ScanAutosaves());
    }

    [Fact]
    public void ProcessAutosaveHeader_GbxIsNotReplay_ReturnsFalse()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var mockGbx = new Mock<IGbxService>();
        mockGbx.Setup(x => x.ParseHeader(It.IsAny<Stream>())).Returns(new CGameCtnChallenge());
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "Challenge.Gbx", new MockFileData(Array.Empty<byte>()) }
        });
        var filePathManager = new FilePathManager(config, fileSystem);
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, mockGbx.Object, logger);

        // Act
        var result = scanner.ProcessAutosaveHeader("Challenge.Gbx");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ProcessAutosaveHeader_ReplayHasNullMapInfo_ReturnsFalse()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var mockGbx = new Mock<IGbxService>();
        mockGbx.Setup(x => x.ParseHeader(It.IsAny<Stream>())).Returns(new CGameCtnReplayRecord());
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "Replay.Gbx", new MockFileData(Array.Empty<byte>()) }
        });
        var filePathManager = new FilePathManager(config, fileSystem);
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, mockGbx.Object, logger);

        // Act
        var result = scanner.ProcessAutosaveHeader("Replay.Gbx");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ProcessAutosaveHeader_ReplayMapUidAlreadyStored_ReturnsFalse()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();

        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.MapInfo))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new Ident("uid", "Stadium", "bigbang1112")]);

        var mockGbx = new Mock<IGbxService>();
        mockGbx.Setup(x => x.ParseHeader(It.IsAny<Stream>())).Returns(replay);
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "Replay.Gbx", new MockFileData(Array.Empty<byte>()) }
        });
        var filePathManager = new FilePathManager(config, fileSystem);
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, mockGbx.Object, logger);
        scanner.AutosaveHeaders.TryAdd("uid", new AutosaveHeader("Replay.Gbx", new CGameCtnReplayRecord()));

        // Act
        var result = scanner.ProcessAutosaveHeader("Replay.Gbx");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ProcessAutosaveHeader_ReplayMapUidFresh_ReturnsTrue()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();

        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.MapInfo))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new Ident("uid", "Stadium", "bigbang1112")]);

        var mockGbx = new Mock<IGbxService>();
        mockGbx.Setup(x => x.ParseHeader(It.IsAny<Stream>())).Returns(replay);
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "Replay.Gbx", new MockFileData(Array.Empty<byte>()) }
        });
        var filePathManager = new FilePathManager(config, fileSystem);
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, mockGbx.Object, logger);
        
        // Act
        var result = scanner.ProcessAutosaveHeader("Replay.Gbx");

        // Assert
        Assert.True(result);
        Assert.True(scanner.AutosaveHeaders.ContainsKey("uid"));
    }

    [Fact]
    public void UpdateAutosaveDetail_AutosavesDirectoryPathIsNull_Throws()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var gbx = Mock.Of<IGbxService>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, gbx, logger);

        // Act & Assert
        Assert.Throws<ImportantPropertyNullException>(() => scanner.UpdateAutosaveDetail("uid"));
    }

    [Fact]
    public void UpdateAutosaveDetail_AutosaveHeadersDoesNotContainFileName_Throws()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var gbx = Mock.Of<IGbxService>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, gbx, logger);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => scanner.UpdateAutosaveDetail("uid"));
    }

    [Fact]
    public void UpdateAutosaveDetail_GbxIsNotReplay_AutosaveDetailNotAdded()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var mockGbx = new Mock<IGbxService>();
        mockGbx.Setup(x => x.Parse(It.IsAny<Stream>())).Returns(new CGameCtnChallenge());
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"C:{slash}UserData{slash}Tracks{slash}Replays{slash}Autosaves{slash}Replay.Gbx", new MockFileData(Array.Empty<byte>()) }
        });
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, mockGbx.Object, logger);
        scanner.AutosaveHeaders.TryAdd("uid", new AutosaveHeader("Replay.Gbx", new CGameCtnReplayRecord()));

        // Act
        scanner.UpdateAutosaveDetail("uid");

        // Assert
        Assert.Empty(scanner.AutosaveDetails);
    }

    [Fact]
    public void UpdateAutosaveDetail_ReplayHasNullTime_AutosaveDetailNotAdded()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var mockGbx = new Mock<IGbxService>();
        var replay = new CGameCtnReplayRecord();
        mockGbx.Setup(x => x.Parse(It.IsAny<Stream>())).Returns(replay);
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"C:{slash}UserData{slash}Tracks{slash}Replays{slash}Autosaves{slash}Replay.Gbx", new MockFileData(Array.Empty<byte>()) }
        });
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, mockGbx.Object, logger);
        scanner.AutosaveHeaders.TryAdd("uid", new AutosaveHeader("Replay.Gbx", new CGameCtnReplayRecord()));

        // Act
        scanner.UpdateAutosaveDetail("uid");

        // Assert
        Assert.Null(replay.Time); // making sure test tests what it should even tho its a mock
        Assert.Empty(scanner.AutosaveDetails);
    }

    [Fact]
    public void UpdateAutosaveDetail_ReplayHasNullChallenge_AutosaveDetailNotAdded()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();

        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.Time))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new TimeInt32(690)]);
        var mockGbx = new Mock<IGbxService>();
        mockGbx.Setup(x => x.Parse(It.IsAny<Stream>())).Returns(replay);
        
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"C:{slash}UserData{slash}Tracks{slash}Replays{slash}Autosaves{slash}Replay.Gbx", new MockFileData(Array.Empty<byte>()) }
        });
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, mockGbx.Object, logger);
        scanner.AutosaveHeaders.TryAdd("uid", new AutosaveHeader("Replay.Gbx", new CGameCtnReplayRecord()));

        // Act
        scanner.UpdateAutosaveDetail("uid");

        // Assert
        Assert.Null(replay.Challenge); // making sure test tests what it should even tho its a mock
        Assert.Empty(scanner.AutosaveDetails);
    }

    private AutosaveScanner Arrange_UpdateAutosaveDetail(CGameCtnChallenge map)
    {
        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.Time))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new TimeInt32(690)]);
        typeof(CGameCtnReplayRecord).GetField("challengeData", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(replay, Array.Empty<byte>());
        typeof(CGameCtnReplayRecord).GetField("challenge", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(replay, map);
        var mockGbx = new Mock<IGbxService>();
        mockGbx.Setup(x => x.Parse(It.IsAny<Stream>())).Returns(replay);

        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"C:{slash}UserData{slash}Tracks{slash}Replays{slash}Autosaves{slash}Replay.Gbx", new MockFileData(Array.Empty<byte>()) }
        });
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, mockGbx.Object, logger);
        scanner.AutosaveHeaders.TryAdd("uid", new AutosaveHeader("Replay.Gbx", new CGameCtnReplayRecord()));

        return scanner;
    }

    [Fact]
    public void UpdateAutosaveDetail_ReplayMapBronzeTimeNull_Throws()
    {
        // Arrange
        var map = new CGameCtnChallenge
        {
            AuthorTime = new TimeInt32(300),
            GoldTime = new TimeInt32(400),
            SilverTime = new TimeInt32(500),
            AuthorScore = 69
        };

        var scanner = Arrange_UpdateAutosaveDetail(map);
        
        // Act & Assert
        Assert.Throws<ImportantPropertyNullException>(() => scanner.UpdateAutosaveDetail("uid"));
        Assert.Null(map.BronzeTime); // making sure test tests what it should even tho its a mock
    }

    [Fact]
    public void UpdateAutosaveDetail_ReplayMapSilverTimeNull_Throws()
    {
        // Arrange
        var map = new CGameCtnChallenge
        {
            AuthorTime = new TimeInt32(300),
            GoldTime = new TimeInt32(400),
            BronzeTime = new TimeInt32(500),
            AuthorScore = 69
        };

        var scanner = Arrange_UpdateAutosaveDetail(map);

        // Act & Assert
        Assert.Throws<ImportantPropertyNullException>(() => scanner.UpdateAutosaveDetail("uid"));
        Assert.Null(map.SilverTime); // making sure test tests what it should even tho its a mock
    }

    [Fact]
    public void UpdateAutosaveDetail_ReplayMapGoldTimeNull_Throws()
    {
        // Arrange
        var map = new CGameCtnChallenge
        {
            AuthorTime = new TimeInt32(300),
            SilverTime = new TimeInt32(400),
            BronzeTime = new TimeInt32(500),
            AuthorScore = 69
        };

        var scanner = Arrange_UpdateAutosaveDetail(map);

        // Act & Assert
        Assert.Throws<ImportantPropertyNullException>(() => scanner.UpdateAutosaveDetail("uid"));
        Assert.Null(map.GoldTime); // making sure test tests what it should even tho its a mock
    }

    [Fact]
    public void UpdateAutosaveDetail_ReplayMapAuthorTimeNull_Throws()
    {
        // Arrange
        var map = new CGameCtnChallenge
        {
            GoldTime = new TimeInt32(300),
            SilverTime = new TimeInt32(400),
            BronzeTime = new TimeInt32(500),
            AuthorScore = 69
        };

        var scanner = Arrange_UpdateAutosaveDetail(map);

        // Act & Assert
        Assert.Throws<ImportantPropertyNullException>(() => scanner.UpdateAutosaveDetail("uid"));
        Assert.Null(map.AuthorTime); // making sure test tests what it should even tho its a mock
    }

    [Theory]
    [InlineData("AlpineCar", "SnowCar")]
    [InlineData("SnowCar", "SnowCar")]
    [InlineData("American", "DesertCar")]
    [InlineData("SpeedCar", "DesertCar")]
    [InlineData("DesertCar", "DesertCar")]
    [InlineData("Rally", "RallyCar")]
    [InlineData("RallyCar", "RallyCar")]
    [InlineData("SportCar", "IslandCar")]
    [InlineData("IslandCar", "IslandCar")]
    [InlineData("BayCar", "BayCar")]
    [InlineData("CoastCar", "CoastCar")]
    [InlineData("StadiumCar", "StadiumCar")]
    public void UpdateAutosaveDetail_ExpectedMapCarName_AddsAutosaveDetailWithExpectedMapCarName(string givenCar, string expectedCar)
    {
        // Arrange
        var map = new CGameCtnChallenge
        {
            AuthorTime = new TimeInt32(200),
            GoldTime = new TimeInt32(300),
            SilverTime = new TimeInt32(400),
            BronzeTime = new TimeInt32(500),
            AuthorScore = 69,
            PlayerModel = new Ident(givenCar, "Vehicles", "Nadeo")
        };

        var scanner = Arrange_UpdateAutosaveDetail(map);

        // Act
        scanner.UpdateAutosaveDetail("uid");

        // Assert
        Assert.NotEmpty(scanner.AutosaveDetails);
        Assert.Equal(expectedCar, actual: scanner.AutosaveDetails["uid"].MapCar);
    }

    [Fact]
    public void UpdateAutosaveDetail_NoPlayerModel_AddsAutosaveDetailWithExpectedMapCarName()
    {
        // Arrange
        var map = new CGameCtnChallenge
        {
            AuthorTime = new TimeInt32(200),
            GoldTime = new TimeInt32(300),
            SilverTime = new TimeInt32(400),
            BronzeTime = new TimeInt32(500),
            AuthorScore = 69
        };
        map.MapInfo = map.MapInfo with { Collection = new("Stadium") };

        var scanner = Arrange_UpdateAutosaveDetail(map);

        // Act
        scanner.UpdateAutosaveDetail("uid");

        // Assert
        Assert.NotEmpty(scanner.AutosaveDetails);
        Assert.Equal(expected: "StadiumCar", actual: scanner.AutosaveDetails["uid"].MapCar);
    }

    [Theory]
    [InlineData(null, "StadiumCar")]
    [InlineData("", "StadiumCar")]
    public void UpdateAutosaveDetail_NoPlayerModelId_AddsAutosaveDetailWithExpectedMapCarName(string givenCar, string expectedCar)
    {
        // Arrange
        var map = new CGameCtnChallenge
        {
            AuthorTime = new TimeInt32(200),
            GoldTime = new TimeInt32(300),
            SilverTime = new TimeInt32(400),
            BronzeTime = new TimeInt32(500),
            AuthorScore = 69,
            PlayerModel = new Ident(givenCar, "Vehicles", "Nadeo")
        };
        map.MapInfo = map.MapInfo with { Collection = new("Stadium") };

        var scanner = Arrange_UpdateAutosaveDetail(map);

        // Act
        scanner.UpdateAutosaveDetail("uid");

        // Assert
        Assert.NotEmpty(scanner.AutosaveDetails);
        Assert.Equal(expectedCar, actual: scanner.AutosaveDetails["uid"].MapCar);
    }

    [Fact]
    public void UpdateAutosaveDetail_HasGhost_AddsAutosaveDetailWithStuntsScoreAndRespawns()
    {
        // Arrange
        var map = new CGameCtnChallenge
        {
            AuthorTime = new TimeInt32(200),
            GoldTime = new TimeInt32(300),
            SilverTime = new TimeInt32(400),
            BronzeTime = new TimeInt32(500),
            AuthorScore = 69,
            PlayerModel = new Ident("StadiumCar", "Vehicles", "Nadeo")
        };
        map.MapInfo = map.MapInfo with { Collection = new("Stadium") };

        var ghost = new CGameCtnGhost();
        ghost.StuntScore = 69;
        ghost.Respawns = 69;

        var events = Mock.Of<IRandomizerEvents>();
        var watcher = Mock.Of<IFileSystemWatcher>();
        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.Time))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new TimeInt32(690)]);
        typeof(CGameCtnReplayRecord).GetField("challengeData", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(replay, Array.Empty<byte>());
        typeof(CGameCtnReplayRecord).GetField("challenge", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(replay, map);
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.Ghosts))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [ImmutableList.Create(ghost)]);
        var mockGbx = new Mock<IGbxService>();
        mockGbx.Setup(x => x.Parse(It.IsAny<Stream>())).Returns(replay);

        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"C:{slash}UserData{slash}Tracks{slash}Replays{slash}Autosaves{slash}Replay.Gbx", new MockFileData(Array.Empty<byte>()) }
        });
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var logger = Mock.Of<ILogger>();

        var scanner = new AutosaveScanner(events, watcher, filePathManager, config, fileSystem, mockGbx.Object, logger);
        scanner.AutosaveHeaders.TryAdd("uid", new AutosaveHeader("Replay.Gbx", new CGameCtnReplayRecord()));

        // Act
        scanner.UpdateAutosaveDetail("uid");

        // Assert
        Assert.NotEmpty(scanner.AutosaveDetails);
        Assert.Equal(expected: ghost.StuntScore, actual: scanner.AutosaveDetails["uid"].Score);
        Assert.Equal(expected: ghost.Respawns, actual: scanner.AutosaveDetails["uid"].Respawns);
    }
}
