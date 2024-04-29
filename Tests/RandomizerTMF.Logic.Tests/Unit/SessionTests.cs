using GBX.NET;
using GBX.NET.Engines.Game;
using Microsoft.Extensions.Logging;
using Moq;
using RandomizerTMF.Logic.Services;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using TmEssentials;
using static GBX.NET.Engines.Hms.CHmsLightMapCache;
using static GBX.NET.Engines.Plug.CPlugMaterialUserInst;

namespace RandomizerTMF.Logic.Tests.Unit;

public class SessionTests
{
    [Fact]
    public void ReloadMap_MapIsNull_ReturnsFalse()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem);

        // Act
        var actual = session.ReloadMap();

        // Assert
        Assert.False(actual);
    }
    
    [Fact]
    public void ReloadMap_MapFilePathIsNull_ReturnsFalse()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();
        var map = new CGameCtnChallenge();

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69")
        };

        // Act
        var actual = session.ReloadMap();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void ReloadMap_MapFilePathIsNotNull_ReturnsTrue()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();
        var map = new CGameCtnChallenge();

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69") { FilePath = "SomePath" }
        };

        // Act
        var actual = session.ReloadMap();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task SkipMapAsync_CancelsSkipTokenSource()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();
        var map = new CGameCtnChallenge();

        var cts = new CancellationTokenSource();

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69") { FilePath = "SomePath" }
        };

        var task = session.PlayMapAsync(cts.Token);

        // Act
        await session.SkipMapAsync();

        // Assert
        Assert.True(session.SkipTokenSource!.IsCancellationRequested);
    }

    [Fact]
    public void StopTrackingMap_SetsMapPropetiesToNull()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();
        var map = new CGameCtnChallenge();

        var cts = new CancellationTokenSource();

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69") { FilePath = "SomePath" }
        };

        // This weirdness is being done because SkipTokenSource has private setter

        var task = session.PlayMapAsync(cts.Token);

        // Act
        session.StopTrackingMap();

        // Assert
        Assert.Null(session.Map);
        Assert.Null(session.SkipTokenSource);
    }

    [Fact]
    public void SkipManually_HasGold_NotClassifiedAsSkipMap()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();

        var map = new CGameCtnChallenge
        {
            MapUid = "420uid"
        };

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69") { FilePath = "SomePath" }
        };

        session.GoldMaps.Add(map.MapUid, session.Map);

        // Act
        session.SkipManually(session.Map);

        // Assert
        Assert.DoesNotContain(map.MapUid, (IDictionary<string, SessionMap>)session.SkippedMaps);
    }

    [Fact]
    public void SkipManually_DoesNotHaveGold_ClassifiedAsSkipMap()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();

        var map = new CGameCtnChallenge
        {
            MapUid = "420uid"
        };

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69") { FilePath = "SomePath" }
        };

        // Act
        session.SkipManually(session.Map);

        // Assert
        Assert.Contains(map.MapUid, (IDictionary<string, SessionMap>)session.SkippedMaps);
    }

    [Fact]
    public void EvaluateAutosave_NullAuthorTime_AutoSkipsMap()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();

        var map = new CGameCtnChallenge
        {
            MapUid = "420uid",
            ChallengeParameters = new CGameCtnChallengeParameters()
            {
                AuthorTime = null
            }
        };

        var cts = new CancellationTokenSource();

        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.MapInfo))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new Ident("mapuid", new("Stadium"), "bigbang1112")]);

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69") { FilePath = "SomePath" }
        };

        var task = session.PlayMapAsync(cts.Token);

        // Act
        session.EvaluateAutosave("SomeOtherPath", replay);

        // Assert
        Assert.True(session.SkipTokenSource!.IsCancellationRequested);
    }

    [Fact]
    public void EvaluateAutosave_NoGhostInReplay_Throws()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();

        var map = new CGameCtnChallenge
        {
            Mode = CGameCtnChallenge.PlayMode.Race,
            MapUid = "420uid",
            ChallengeParameters = new CGameCtnChallengeParameters
            {
                AuthorTime = new TimeInt32(69),
                GoldTime = new TimeInt32(420)
            }
        };

        var cts = new CancellationTokenSource();

        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.MapInfo))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new Ident("mapuid", new("Stadium"), "bigbang1112")]);

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69") { FilePath = "SomePath" }
        };

        // This weirdness is being done because SkipTokenSource has private setter

        var task = session.PlayMapAsync(cts.Token);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => session.EvaluateAutosave("SomeOtherPath", replay));
    }

    [Fact]
    public void EvaluateAutosave_IsAuthorMedal_UpdatesAndAutoSkipsMap()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();

        var map = new CGameCtnChallenge
        {
            Mode = CGameCtnChallenge.PlayMode.Race,
            MapUid = "420uid",
            ChallengeParameters = new CGameCtnChallengeParameters
            {
                AuthorTime = new TimeInt32(69)
            }
        };

        var cts = new CancellationTokenSource();

        var ghost = new CGameCtnGhost
        {
            RaceTime = new(42)
        };

        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.MapInfo))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new Ident("mapuid", new("Stadium"), "bigbang1112")]);

        var ghosts = ImmutableList.Create(ghost);
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.Ghosts))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [ghosts]);

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69") { FilePath = "SomePath" }
        };

        // This weirdness is being done because SkipTokenSource has private setter

        var task = session.PlayMapAsync(cts.Token);

        // Act
        session.EvaluateAutosave("SomeOtherPath", replay);

        // Assert
        Assert.True(session.SkipTokenSource!.IsCancellationRequested);
        Assert.DoesNotContain(map.MapUid, (IDictionary<string, SessionMap>)session.GoldMaps);
        Assert.Contains(map.MapUid, (IDictionary<string, SessionMap>)session.AuthorMaps);
    }

    [Fact]
    public void EvaluateAutosave_IsGoldMedal_UpdatesMap()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();

        var map = new CGameCtnChallenge
        {
            Mode = CGameCtnChallenge.PlayMode.Race,
            MapUid = "420uid",
            ChallengeParameters = new CGameCtnChallengeParameters
            {
                AuthorTime = new TimeInt32(69),
                GoldTime = new TimeInt32(420)
            }
        };

        var cts = new CancellationTokenSource();

        var ghost = new CGameCtnGhost
        {
            RaceTime = new(128)
        };

        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.MapInfo))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new Ident("mapuid", new("Stadium"), "bigbang1112")]);

        var ghosts = ImmutableList.Create(ghost);
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.Ghosts))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [ghosts]);

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69") { FilePath = "SomePath" }
        };

        // This weirdness is being done because SkipTokenSource has private setter

        var task = session.PlayMapAsync(cts.Token);

        // Act
        session.EvaluateAutosave("SomeOtherPath", replay);

        // Assert
        Assert.False(session.SkipTokenSource!.IsCancellationRequested);
        Assert.Contains(map.MapUid, (IDictionary<string, SessionMap>)session.GoldMaps);
        Assert.DoesNotContain(map.MapUid, (IDictionary<string, SessionMap>)session.AuthorMaps);
    }

    [Fact]
    public void AutosaveCreatedOrChanged_ReplayMapInfoIsNull_ReturnsFalse()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();

        var replay = new CGameCtnReplayRecord();

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem);

        // Act
        var result = session.AutosaveCreatedOrChanged("SomePath", replay);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AutosaveCreatedOrChanged_SessionMapIsNull_ReturnsFalse()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();

        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.MapInfo))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new Ident("mapuid", new("Stadium"), "bigbang1112")]);

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem);

        // Act
        var result = session.AutosaveCreatedOrChanged("SomePath", replay);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AutosaveCreatedOrChanged_SessionMapInfoNotMatchingReplayMapInfo_ReturnsFalse()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();

        var map = new CGameCtnChallenge
        {
            Mode = CGameCtnChallenge.PlayMode.Race,
            MapInfo = ("420uid", new("Stadium"), "bigbang1112"),
            ChallengeParameters = new CGameCtnChallengeParameters
            {
                AuthorTime = new TimeInt32(69),
                GoldTime = new TimeInt32(420)
            }
        };

        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.MapInfo))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new Ident("mapuid", new("Stadium"), "bigbang1112")]);

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69") { FilePath = "SomePath" }
        };

        // Act
        var result = session.AutosaveCreatedOrChanged("SomePath", replay);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AutosaveCreatedOrChanged_SessionMapInfoMatchesReplayMapInfo_ReturnsTrue()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var mapDownloader = Mock.Of<IMapDownloader>();
        var validator = Mock.Of<IValidator>();
        var config = new RandomizerConfig();
        var game = Mock.Of<ITMForever>();
        var logger = Mock.Of<ILogger>();
        var fileSystem = new MockFileSystem();

        var map = new CGameCtnChallenge
        {
            Mode = CGameCtnChallenge.PlayMode.Race,
            MapInfo = ("mapuid", new("Stadium"), "bigbang1112"),
            ChallengeParameters = new CGameCtnChallengeParameters
            {
                AuthorTime = new TimeInt32(69),
                GoldTime = new TimeInt32(420)
            }
        };

        var ghost = new CGameCtnGhost
        {
            RaceTime = new(128)
        };

        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.MapInfo))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new Ident("mapuid", new("Stadium"), "bigbang1112")]);

        var ghosts = ImmutableList.Create(ghost);
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.Ghosts))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [ghosts]);

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem)
        {
            Map = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackgbx/69") { FilePath = "SomePath" }
        };

        // Act
        var result = session.AutosaveCreatedOrChanged("SomePath", replay);

        // Assert
        Assert.True(result);
    }
}
