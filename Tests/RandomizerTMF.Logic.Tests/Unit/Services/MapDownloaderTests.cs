using Moq;
using RandomizerTMF.Logic.Services;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using GBX.NET.Engines.Game;
using RandomizerTMF.Logic.Exceptions;
using System.Net;
using System.Diagnostics;

namespace RandomizerTMF.Logic.Tests.Unit.Services;

public class MapDownloaderTests
{
    private readonly char slash = Path.DirectorySeparatorChar;
    
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
        var gbxService = Mock.Of<IGbxService>();

        var map = new CGameCtnChallenge();
        var cts = new CancellationTokenSource();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, mockValidator.Object, http, random, delay, fileSystem, gbxService, logger);

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
        var gbxService = Mock.Of<IGbxService>();

        var map = new CGameCtnChallenge();
        var cts = new CancellationTokenSource();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, mockValidator.Object, http, random, delay, fileSystem, gbxService, logger);

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
        var gbxService = Mock.Of<IGbxService>();

        var map = new CGameCtnChallenge();
        var cts = new CancellationTokenSource();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, mockValidator.Object, http, random, delay, fileSystem, gbxService, logger);

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
        var gbxService = Mock.Of<IGbxService>();

        var map = new CGameCtnChallenge();
        var cts = new CancellationTokenSource();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, mockValidator.Object, http, random, delay, fileSystem, gbxService, logger);
        
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
        var gbxService = Mock.Of<IGbxService>();

        var expectedUrl = "https://tmuf.exchange/trackgbx/69";

        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When(expectedUrl).Respond(HttpStatusCode.OK);
        var http = new HttpClient(mockHandler);
        
        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

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
        var gbxService = Mock.Of<IGbxService>();

        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://tmuf.exchange/trackgbx/69").Respond(HttpStatusCode.NotFound);
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

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
        var gbxService = Mock.Of<IGbxService>();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

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
        var gbxService = Mock.Of<IGbxService>();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

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
        var gbxService = Mock.Of<IGbxService>();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

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
        var gbxService = Mock.Of<IGbxService>();

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

        var response = new HttpResponseMessage();

        // Act
        var result = mapDownloader.GetRequestUriFromResponse(response);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchRandomTrackAsync_MapNotFound_Throws()
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
        var gbxService = Mock.Of<IGbxService>();

        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://tmnf.exchange/trackrandom?primarytype=0&authortimemax=180000").Respond(HttpStatusCode.NotFound);
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

        var cts = new CancellationTokenSource();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidSessionException>(async () => await mapDownloader.FetchRandomTrackAsync(cts.Token));
    }

    [Fact]
    public async Task FetchRandomTrackAsync_Success_ReturnsResponse()
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
        var gbxService = Mock.Of<IGbxService>();

        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://tmnf.exchange/trackrandom?primarytype=0&authortimemax=180000").Respond(HttpStatusCode.OK);
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

        var cts = new CancellationTokenSource();

        // Act
        var response = await mapDownloader.FetchRandomTrackAsync(cts.Token);

        // Assert
        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);
    }

    [Fact]
    public async Task SaveMapAsync_DownloadedDirectoryPathIsNull_Unreachable()
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
        var gbxService = Mock.Of<IGbxService>();

        var mockHandler = new MockHttpMessageHandler();
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

        var cts = new CancellationTokenSource();

        var response = new HttpResponseMessage
        {
            RequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri("https://tmuf.exchange/trackshow/69")
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnreachableException>(async () => await mapDownloader.SaveMapAsync(response, "xdd", cts.Token));
    }
    
    [Fact]
    public async Task SaveMapAsync_FileNameAvailable_SavesMapAndReturnsPath()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var gbxService = Mock.Of<IGbxService>();

        var mockHandler = new MockHttpMessageHandler();
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

        var cts = new CancellationTokenSource();

        var expectedData = "    "u8.ToArray();
        
        var content = new ByteArrayContent(expectedData);
        content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
        {
            FileName = "SomeMap.Challenge.Gbx"
        };

        var response = new HttpResponseMessage
        {
            RequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri("https://tmuf.exchange/trackshow/69")
            },
            Content = content
        };

        var expectedSaveFilePath = Path.Combine("C:", "UserData", "Tracks", "Challenges", "Downloaded", "_RandomizerTMF", "SomeMap.Challenge.Gbx");

        // Act
        var path = await mapDownloader.SaveMapAsync(response, "xdd", cts.Token);

        // Assert
        Assert.True(fileSystem.File.Exists(expectedSaveFilePath));
        Assert.Equal(expectedData, actual: fileSystem.File.ReadAllBytes(expectedSaveFilePath));
        Assert.Equal(expectedSaveFilePath, actual: path);
    }

    [Fact]
    public async Task SaveMapAsync_FileNameNull_SavesMapAndReturnsPath()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var gbxService = Mock.Of<IGbxService>();

        var mockHandler = new MockHttpMessageHandler();
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

        var cts = new CancellationTokenSource();

        var expectedData = "    "u8.ToArray();
        
        var content = new ByteArrayContent(expectedData);
        content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
        {
            FileName = null
        };

        var response = new HttpResponseMessage
        {
            RequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri("https://tmuf.exchange/trackshow/69")
            },
            Content = content
        };

        var expectedSaveFilePath = Path.Combine("C:", "UserData", "Tracks", "Challenges", "Downloaded", "_RandomizerTMF", "xdd.Challenge.Gbx");

        // Act
        var path = await mapDownloader.SaveMapAsync(response, "xdd", cts.Token);

        // Assert
        Assert.True(fileSystem.File.Exists(expectedSaveFilePath));
        Assert.Equal(expectedData, actual: fileSystem.File.ReadAllBytes(expectedSaveFilePath));
        Assert.Equal(expectedSaveFilePath, actual: path);
    }

    [Fact]
    public async Task SaveMapAsync_ContentDispositionNull_SavesMapAndReturnsPath()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var gbxService = Mock.Of<IGbxService>();

        var mockHandler = new MockHttpMessageHandler();
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

        var cts = new CancellationTokenSource();

        var expectedData = "    "u8.ToArray();

        var response = new HttpResponseMessage
        {
            RequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri("https://tmuf.exchange/trackshow/69")
            },
            Content = new ByteArrayContent(expectedData)
        };

        var expectedSaveFilePath = Path.Combine("C:", "UserData", "Tracks", "Challenges", "Downloaded", "_RandomizerTMF", "xdd.Challenge.Gbx");

        // Act
        var path = await mapDownloader.SaveMapAsync(response, "xdd", cts.Token);

        // Assert
        Assert.True(fileSystem.File.Exists(expectedSaveFilePath));
        Assert.Equal(expectedData, actual: fileSystem.File.ReadAllBytes(expectedSaveFilePath));
        Assert.Equal(expectedSaveFilePath, actual: path);
    }

    [Fact]
    public async Task PrepareNewMapAsync_RequestUriIsNull_ReturnsFalse()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var game = Mock.Of<ITMForever>();
        var gbxService = Mock.Of<IGbxService>();

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };

        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://tmnf.exchange/trackrandom?primarytype=0&authortimemax=180000").Respond(req =>
        {
            req.RequestUri = null;
            return response;
        });
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

        var cts = new CancellationTokenSource();

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem);

        // Act
        var result = await mapDownloader.PrepareNewMapAsync(session, cts.Token);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task PrepareNewMapAsync_MapIsInvalid_Throws()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var game = Mock.Of<ITMForever>();
        var gbxService = Mock.Of<IGbxService>();

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };

        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://tmnf.exchange/trackrandom?primarytype=0&authortimemax=180000").Respond(req =>
        {
            req.RequestUri = new("https://tmnf.exchange/trackshow/69");
            return response;
        });
        mockHandler.When("https://tmnf.exchange/trackgbx/69").Respond(new ByteArrayContent("E*"u8.ToArray()));
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, gbxService, logger);

        var cts = new CancellationTokenSource();

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem);

        // Act
        var result = await mapDownloader.PrepareNewMapAsync(session, cts.Token);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task PrepareNewMapAsync_MapIsNull_ReturnsFalse()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var discord = Mock.Of<IDiscordRichPresence>();
        var validator = Mock.Of<IValidator>();
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var game = Mock.Of<ITMForever>();
        
        var map = new CGameCtnChallenge();
        var mockGbxService = new Mock<IGbxService>();
        mockGbxService.Setup(x => x.Parse(It.IsAny<Stream>())).Returns(map);

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };

        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://tmnf.exchange/trackrandom?primarytype=0&authortimemax=180000").Respond(req =>
        {
            req.RequestUri = new("https://tmnf.exchange/trackshow/69");
            return response;
        });
        mockHandler.When("https://tmnf.exchange/trackgbx/69").Respond(new ByteArrayContent("E*"u8.ToArray()));
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, mockGbxService.Object, logger);

        var cts = new CancellationTokenSource();

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem);

        // Act & Assert
        await Assert.ThrowsAsync<MapValidationException>(async () => await mapDownloader.PrepareNewMapAsync(session, cts.Token));
    }

    [Fact]
    public async Task PrepareNewMapAsync_Valid_SavesMapAndReturnsTrue()
    {
        // Arrange
        var events = Mock.Of<IRandomizerEvents>();
        var config = new RandomizerConfig();
        var fileSystem = new MockFileSystem();
        var filePathManager = new FilePathManager(config, fileSystem)
        {
            UserDataDirectoryPath = $"C:{slash}UserData"
        };
        var discord = Mock.Of<IDiscordRichPresence>();
        
        var mockValidator = new Mock<IValidator>();
        var invalidBlock = default(string);
        mockValidator.Setup(x => x.ValidateMap(It.IsAny<CGameCtnChallenge>(), out invalidBlock)).Returns(true);
        var validator = mockValidator.Object;
        
        var random = Mock.Of<IRandomGenerator>();
        var delay = Mock.Of<IDelayService>();
        var logger = Mock.Of<ILogger>();
        var game = Mock.Of<ITMForever>();

        var map = new CGameCtnChallenge();
        var mockGbxService = new Mock<IGbxService>();
        mockGbxService.Setup(x => x.Parse(It.IsAny<Stream>())).Returns(map);

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK
        };

        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://tmnf.exchange/trackrandom?primarytype=0&authortimemax=180000").Respond(req =>
        {
            req.RequestUri = new("https://tmnf.exchange/trackshow/69");
            return response;
        });
        mockHandler.When("https://tmnf.exchange/trackgbx/69").Respond(new ByteArrayContent("E*"u8.ToArray()));
        var http = new HttpClient(mockHandler);

        var mapDownloader = new MapDownloader(events, config, filePathManager, discord, validator, http, random, delay, fileSystem, mockGbxService.Object, logger);

        var cts = new CancellationTokenSource();

        var session = new Session(events, mapDownloader, validator, config, game, logger, fileSystem);

        // Act
        var result = await mapDownloader.PrepareNewMapAsync(session, cts.Token);

        // Assert
        Assert.True(result);

        // TODO: Check for saved stuff
    }
}
