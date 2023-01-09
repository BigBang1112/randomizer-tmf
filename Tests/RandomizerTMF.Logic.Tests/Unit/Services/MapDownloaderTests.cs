using Moq;
using RandomizerTMF.Logic.Services;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using GBX.NET.Engines.Game;
using RandomizerTMF.Logic.Exceptions;
using System.Net;

namespace RandomizerTMF.Logic.Tests.Unit.Services;

public class MapDownloaderTests
{
    [Fact]
    public async Task ValidateMapAsync_InvalidMapUnderMaxAttempts_DoesNotThrow()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var discord = Mock.Of<IDiscordRichPresence>();
        var mockValidator = new Mock<IValidator>();
        var expectedInvalidBlock = default(string);
        mockValidator.Setup(x => x.ValidateMap(It.IsAny<CGameCtnChallenge>(), out expectedInvalidBlock))
            .Returns(true);
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var http = Mock.Of<HttpClient>();

        var map = NodeInstance.Create<CGameCtnChallenge>();
        var cts = new CancellationTokenSource();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, mockValidator.Object, http, random, delay, logger);

        // Act
        var exception = await Record.ExceptionAsync(async () => await mapDownloader.ValidateMapAsync(map, cts.Token));

        // Assert
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("LolBlock")]
    public async Task ValidateMapAsync_InvalidMapUnderMaxAttempts_ThrowsMapValidationException(string? expectedInvalidBlock)
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var discord = Mock.Of<IDiscordRichPresence>();
        var mockValidator = new Mock<IValidator>();
        mockValidator.Setup(x => x.ValidateMap(It.IsAny<CGameCtnChallenge>(), out expectedInvalidBlock))
            .Returns(false);
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var http = Mock.Of<HttpClient>();

        var map = NodeInstance.Create<CGameCtnChallenge>();
        var cts = new CancellationTokenSource();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, mockValidator.Object, http, random, delay, logger);

        // Act & Assert
        await Assert.ThrowsAsync<MapValidationException>(async () => await mapDownloader.ValidateMapAsync(map, cts.Token));
    }

    [Theory]
    [InlineData(null, 8)]
    [InlineData("LolBlock", 8)]
    [InlineData(null, 9)]
    [InlineData("LolBlock", 9)]
    public async Task ValidateMapAsync_InvalidMapJustBeforeMaxAttempts_ThrowsMapValidationException(string? expectedInvalidBlock, int attempts)
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var discord = Mock.Of<IDiscordRichPresence>();
        var mockValidator = new Mock<IValidator>();
        mockValidator.Setup(x => x.ValidateMap(It.IsAny<CGameCtnChallenge>(), out expectedInvalidBlock))
            .Returns(false);
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var http = Mock.Of<HttpClient>();

        var map = NodeInstance.Create<CGameCtnChallenge>();
        var cts = new CancellationTokenSource();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, mockValidator.Object, http, random, delay, logger);

        // Act & Assert
        for (var i = 0; i < attempts; i++)
        {
            await Assert.ThrowsAsync<MapValidationException>(async () => await mapDownloader.ValidateMapAsync(map, cts.Token));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("LolBlock")]
    public async Task ValidateMapAsync_InvalidMapAtMaxAttempts_ThrowsInvalidSessionException(string? expectedInvalidBlock)
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var discord = Mock.Of<IDiscordRichPresence>();
        var mockValidator = new Mock<IValidator>();
        mockValidator.Setup(x => x.ValidateMap(It.IsAny<CGameCtnChallenge>(), out expectedInvalidBlock))
            .Returns(false);
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var http = Mock.Of<HttpClient>();

        var map = NodeInstance.Create<CGameCtnChallenge>();
        var cts = new CancellationTokenSource();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, mockValidator.Object, http, random, delay, logger);
        
        // Act & Assert
        for (var i = 0; i < 9; i++)
        {
            try
            {
                await mapDownloader.ValidateMapAsync(map, cts.Token);
            }
            catch
            {

            }
        }

        await Assert.ThrowsAsync<InvalidSessionException>(async () => await mapDownloader.ValidateMapAsync(map, cts.Token));
    }

    [Fact]
    public async Task DownloadMapByTrackIdAsync_Success_UrlIsValid()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();

        var expectedUrl = "https://tmuf.exchange/trackgbx/69";

        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When(expectedUrl).Respond(HttpStatusCode.OK);
        var http = new HttpClient(mockHandler);
        
        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, logger);

        var cts = new CancellationTokenSource();

        // Act
        var response = await mapDownloader.DownloadMapByTrackIdAsync("tmuf.exchange", "69", cts.Token);

        // Assert
        Assert.Equal(expectedUrl, actual: response.RequestMessage?.RequestUri?.ToString());
    }

    [Fact]
    public async Task DownloadMapByTrackIdAsync_NotSuccess_Throws()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();

        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://tmuf.exchange/trackgbx/69").Respond(HttpStatusCode.NotFound);
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, logger);

        var cts = new CancellationTokenSource();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () => await mapDownloader.DownloadMapByTrackIdAsync("tmuf.exchange", "69", cts.Token));
    }

    [Fact]
    public void GetTrackIdFromUri_ReturnsCorrectSegment()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var http = Mock.Of<HttpClient>();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, logger);

        var uri = new Uri("https://tmuf.exchange/trackshow/69");

        // Act
        var result = mapDownloader.GetTrackIdFromUri(uri);

        // Assert
        Assert.Equal(expected: "69", actual: result);
    }

    [Fact]
    public void GetRequestUriFromResponse_AllCorrect_ReturnsUri()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var http = Mock.Of<HttpClient>();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, logger);

        var uri = new Uri("https://tmuf.exchange/trackshow/69");

        var response = new HttpResponseMessage
        {
            RequestMessage = new HttpRequestMessage
            {
                RequestUri = uri
            }
        };

        // Act
        var result = mapDownloader.GetRequestUriFromResponse(response);

        // Assert
        Assert.Equal(expected: uri, actual: result);
    }

    [Fact]
    public void GetRequestUriFromResponse_NullRequestUri_ReturnsNull()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var http = Mock.Of<HttpClient>();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, logger);

        var response = new HttpResponseMessage
        {
            RequestMessage = new HttpRequestMessage()
        };

        // Act
        var result = mapDownloader.GetRequestUriFromResponse(response);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetRequestUriFromResponse_NullRequestMessage_ReturnsNull()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem);
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var http = Mock.Of<HttpClient>();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, logger);

        var response = new HttpResponseMessage();

        // Act
        var result = mapDownloader.GetRequestUriFromResponse(response);

        // Assert
        Assert.Null(result);
    }
}
