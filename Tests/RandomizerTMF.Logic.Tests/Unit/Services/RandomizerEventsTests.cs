using GBX.NET.Engines.Game;
using Microsoft.Extensions.Logging;
using Moq;
using RandomizerTMF.Logic.Services;

namespace RandomizerTMF.Logic.Tests.Unit.Services;

public class RandomizerEventsTests
{
    [Fact]
    public void OnStatus_InvokesEvent()
    {
        // Arrange
        var config = new RandomizerConfig();
        var mockLogger = new Mock<ILogger>();
        var mockDiscord = new Mock<IDiscordRichPresence>();
        var events = new RandomizerEvents(config, mockLogger.Object, mockDiscord.Object);

        var eventRaisedWithStatus = default(string);
        events.Status += (status) => eventRaisedWithStatus = status;

        // Act
        events.OnStatus("Some status");

        // Assert
        Assert.Equal(expected: "Some status", actual: eventRaisedWithStatus);
    }
    
    [Fact]
    public void OnFirstMapStarted_InvokesEvent()
    {
        // Arrange
        var config = new RandomizerConfig();
        var mockLogger = new Mock<ILogger>();
        var mockDiscord = new Mock<IDiscordRichPresence>();
        var events = new RandomizerEvents(config, mockLogger.Object, mockDiscord.Object);

        var eventRaised = false;
        events.FirstMapStarted += () => eventRaised = true;

        // Act
        events.OnFirstMapStarted();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void OnMapStarted_InvokesEvent()
    {
        // Arrange
        var config = new RandomizerConfig();
        var mockLogger = new Mock<ILogger>();
        var mockDiscord = new Mock<IDiscordRichPresence>();
        var events = new RandomizerEvents(config, mockLogger.Object, mockDiscord.Object);
        var sessionMap = new SessionMap(NodeInstance.Create<CGameCtnChallenge>(), DateTimeOffset.Now, "https://tmuf.exchange/trackshow/69");

        var eventRaised = false;
        events.MapStarted += (map) => eventRaised = true;

        // Act
        events.OnMapStarted(sessionMap);

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void OnMapEnded_InvokesEvent()
    {
        // Arrange
        var config = new RandomizerConfig();
        var mockLogger = new Mock<ILogger>();
        var mockDiscord = new Mock<IDiscordRichPresence>();
        var events = new RandomizerEvents(config, mockLogger.Object, mockDiscord.Object);

        var eventRaised = false;
        events.MapEnded += () => eventRaised = true;

        // Act
        events.OnMapEnded();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void OnMapSkip_InvokesEvent()
    {
        // Arrange
        var config = new RandomizerConfig();
        var mockLogger = new Mock<ILogger>();
        var mockDiscord = new Mock<IDiscordRichPresence>();
        var events = new RandomizerEvents(config, mockLogger.Object, mockDiscord.Object);

        var eventRaised = false;
        events.MapSkip += () => eventRaised = true;

        // Act
        events.OnMapSkip();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void OnMedalUpdate_InvokesEvent()
    {
        // Arrange
        var config = new RandomizerConfig();
        var mockLogger = new Mock<ILogger>();
        var mockDiscord = new Mock<IDiscordRichPresence>();
        var events = new RandomizerEvents(config, mockLogger.Object, mockDiscord.Object);

        var eventRaised = false;
        events.MedalUpdate += () => eventRaised = true;

        // Act
        events.OnMedalUpdate();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void OnAutosaveCreatedOrChanged_InvokesEvent()
    {
        // Arrange
        var config = new RandomizerConfig();
        var mockLogger = new Mock<ILogger>();
        var mockDiscord = new Mock<IDiscordRichPresence>();
        var events = new RandomizerEvents(config, mockLogger.Object, mockDiscord.Object);
        var replay = NodeInstance.Create<CGameCtnReplayRecord>();
        
        var eventFileName = default(string);
        var eventReplay = default(CGameCtnReplayRecord);

        events.AutosaveCreatedOrChanged += (fileName, replay) =>
        {
            eventFileName = fileName;
            eventReplay = replay;
        };

        // Act
        events.OnAutosaveCreatedOrChanged("File", replay);

        // Assert
        Assert.Equal(expected: "File", actual: eventFileName);
        Assert.Same(expected: replay, actual: eventReplay);
    }
}
