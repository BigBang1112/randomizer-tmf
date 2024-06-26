﻿using Microsoft.Extensions.Logging;
using GBX.NET.Engines.Game;
using Moq;
using RandomizerTMF.Logic.Services;
using System.IO.Abstractions.TestingHelpers;
using TmEssentials;
using System.Reflection;
using GBX.NET;
using System.Collections.Immutable;

namespace RandomizerTMF.Logic.Tests.Unit;

public class SessionDataTests
{
    [Fact]
    public void Initialize()
    {
        var startedAt = DateTimeOffset.Now;
        var config = new RandomizerConfig();
        var logger = new Mock<ILogger>();
        var fileSystem = new MockFileSystem();
        var data = SessionData.Initialize(startedAt, config, logger.Object, fileSystem);

        Assert.Equal(RandomizerEngine.Version, data.Version);
        Assert.Equal(startedAt, data.StartedAt);
        Assert.Equal(config.Rules, data.Rules);
        Assert.Equal(Path.Combine(FilePathManager.SessionsDirectoryPath, data.StartedAtText), data.DirectoryPath);
        Assert.NotNull(data.Maps);
        Assert.True(fileSystem.Directory.Exists(data.DirectoryPath));
    }

    [Theory]
    [InlineData(CGameCtnChallenge.PlayMode.Race, null, "map_name__2'03''45_playerLogin.Replay.Gbx")]
    [InlineData(CGameCtnChallenge.PlayMode.Stunts, null, "map_name__67_2'03''45_playerLogin.Replay.Gbx")]
    [InlineData(CGameCtnChallenge.PlayMode.Platform, null, "map_name__3_2'03''45_playerLogin.Replay.Gbx")]
    [InlineData(CGameCtnChallenge.PlayMode.Race, "", "map_name__2'03''45_playerLogin.Replay.Gbx")]
    [InlineData(CGameCtnChallenge.PlayMode.Stunts, "", "map_name__67_2'03''45_playerLogin.Replay.Gbx")]
    [InlineData(CGameCtnChallenge.PlayMode.Platform, "", "map_name__3_2'03''45_playerLogin.Replay.Gbx")]
    [InlineData(CGameCtnChallenge.PlayMode.Race, " {1}-{0}-{2}", " 2'03''45-map_name_-playerLogin")]
    [InlineData(CGameCtnChallenge.PlayMode.Stunts, "{1}-{2}-{0}.Replay.Gbx", "67_2'03''45-playerLogin-map_name_.Replay.Gbx")]
    [InlineData(CGameCtnChallenge.PlayMode.Platform, "{0}-{2}-{1} ", "map_name_-playerLogin-3_2'03''45 ")]
    public void UpdateFromAutosave_SavesReplayToCurrentMap(CGameCtnChallenge.PlayMode mode, string? replayFileFormat, string expectedReplayFileName)
    {
        // Arrange
        var fullPath = @"C:\Test\Autosave.Replay.Gbx";

        var config = new RandomizerConfig
        {
            ReplayFileFormat = replayFileFormat
        };
        var mockLogger = new Mock<ILogger>();
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { fullPath, new MockFileData("some replay content") }
        });
        var sessionData = SessionData.Initialize(config, mockLogger.Object, fileSystem);

        var replaysDir = Path.Combine(sessionData.DirectoryPath, "Replays");
        var expectedReplayFilePath = Path.Combine(replaysDir, expectedReplayFileName);

        var map = new CGameCtnChallenge
        {
            MapName = "map name*",
            MapUid = "uid",
            Mode = mode
        };

        var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackshow/69");
        
        sessionData.Maps.Add(new SessionDataMap
        {
            Name = sessionMap.Map.MapName,
            Uid = sessionMap.Map.MapUid,
            TmxLink = sessionMap.TmxLink,
        });

        var ghost = new CGameCtnGhost
        {
            StuntScore = 67,
            Respawns = 3
        };

        var replay = new CGameCtnReplayRecord();
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.Time))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [new TimeInt32(123450)]);
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.Ghosts))!.GetSetMethod(nonPublic: true)!.Invoke(replay, [ImmutableList.Create(ghost)]);
        typeof(CGameCtnReplayRecord).GetProperty(nameof(CGameCtnReplayRecord.PlayerLogin))!.GetSetMethod(nonPublic: true)!.Invoke(replay, ["playerLogin"]);

        var expectedElapsed = TimeSpan.FromMinutes(1);

        // Act
        sessionData.UpdateFromAutosave(fullPath, sessionMap, replay, expectedElapsed);

        // Assert
        Assert.NotEmpty(sessionData.Maps);
        Assert.NotEmpty(sessionData.Maps[0].Replays);

        var sessionReplay = sessionData.Maps[0].Replays[0];

        Assert.Equal(expectedReplayFileName, actual: sessionReplay.FileName);
        Assert.Equal(expected: expectedElapsed, sessionReplay.Timestamp);

        Assert.True(fileSystem.FileExists(expectedReplayFilePath));
        Assert.True(fileSystem.FileExists(Path.Combine(sessionData.DirectoryPath, "Session.bin")));
        Assert.Equal(fileSystem.File.ReadAllBytes(expectedReplayFilePath), actual: fileSystem.File.ReadAllBytes(fullPath));
    }

    [Fact]
    public void Save_CreatesSessionBin()
    {
        // Arrange
        var config = new RandomizerConfig();
        var mockLogger = new Mock<ILogger>();
        var fileSystem = new MockFileSystem();
        var sessionData = SessionData.Initialize(config, mockLogger.Object, fileSystem);

        // Act
        sessionData.Save();

        // Assert
        Assert.True(fileSystem.FileExists(Path.Combine(sessionData.DirectoryPath, "Session.bin")));
    }

    [Fact]
    public void InternalSetReadOnlySessionBin_SetsReadOnlyAttribute()
    {
        // Arrange
        var config = new RandomizerConfig();
        var mockLogger = new Mock<ILogger>();
        var fileSystem = new MockFileSystem();
        var sessionData = SessionData.Initialize(config, mockLogger.Object, fileSystem);

        // Act
        sessionData.InternalSetReadOnlySessionBin();

        // Assert
        Assert.True(fileSystem.File.GetAttributes(Path.Combine(sessionData.DirectoryPath, "Session.bin")).HasFlag(FileAttributes.ReadOnly));
    }

    [Fact]
    public void InternalSetReadOnlySessionBin_SetsReadOnlyAttribute_FileNotFound()
    {
        // Arrange
        var config = new RandomizerConfig();
        var mockLogger = new Mock<ILogger>();
        var fileSystem = new MockFileSystem();
        var sessionData = SessionData.Initialize(config, mockLogger.Object, fileSystem);
        
        // To make things easier
        fileSystem.File.Delete(Path.Combine(sessionData.DirectoryPath, "Session.bin"));

        // Act & Assert
        Assert.Throws<FileNotFoundException>(sessionData.InternalSetReadOnlySessionBin);
    }

    [Fact]
    public void SetMapResult_UpdatesMapResultAndLastTimestamp()
    {
        // Arrange
        var config = new RandomizerConfig();
        var mockLogger = new Mock<ILogger>();
        var fileSystem = new MockFileSystem();
        var sessionData = SessionData.Initialize(config, mockLogger.Object, fileSystem);
        var fileTimestamp = fileSystem.GetFile(Path.Combine(sessionData.DirectoryPath, "Session.bin")).LastWriteTime;

        var map = new CGameCtnChallenge
        {
            MapName = "map name*",
            MapUid = "uid"
        };

        var lastTimestamp = TimeSpan.FromMinutes(1);

        var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "https://tmuf.exchange/trackshow/69")
        {
            LastTimestamp = lastTimestamp
        };
        
        sessionData.Maps.Add(new SessionDataMap
        {
            Name = sessionMap.Map.MapName,
            Uid = sessionMap.Map.MapUid,
            TmxLink = sessionMap.TmxLink,
        });

        // Act
        sessionData.SetMapResult(sessionMap, "SomeResult");

        // Assert
        Assert.Equal(expected: "SomeResult", actual: sessionData.Maps[0].Result);
        Assert.Equal(expected: lastTimestamp, actual: sessionData.Maps[0].LastTimestamp);
        Assert.NotEqual(expected: fileTimestamp, actual: fileSystem.GetFile(Path.Combine(sessionData.DirectoryPath, "Session.bin")).LastWriteTime);
    }
}
