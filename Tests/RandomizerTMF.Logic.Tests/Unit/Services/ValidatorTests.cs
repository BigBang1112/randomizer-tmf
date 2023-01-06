using GBX.NET;
using GBX.NET.Engines.Game;
using Moq;
using RandomizerTMF.Logic.Exceptions;
using RandomizerTMF.Logic.Services;
using System.Collections.Concurrent;

namespace RandomizerTMF.Logic.Tests.Unit.Services;

public class ValidatorTests
{
    [Fact]
    public void ValidateRules_TimeLimitEqualsZero_Throws()
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig
        {
            Rules = new() { TimeLimit = TimeSpan.Zero }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);
        
        Assert.Throws<RuleValidationException>(validator.ValidateRules);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    public void ValidateRules_TimeLimit10HoursOrMore_Throws(int hours)
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig
        {
            Rules = new() { TimeLimit = TimeSpan.FromHours(hours) }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);

        Assert.Throws<RuleValidationException>(validator.ValidateRules);
    }

    [Fact]
    public void ValidateRules_AllRulesValid_DoesNotThrow()
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig();

        var validator = new Validator(autosaveScanner, additionalData, config);

        var exception = Record.Exception(validator.ValidateRules);

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateRequestRules_AllRulesValid_DoesNotThrow()
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig();

        var validator = new Validator(autosaveScanner, additionalData, config);

        var exception = Record.Exception(validator.ValidateRequestRules);

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(ESite.Nations, EPrimaryType.Platform)]
    [InlineData(ESite.Nations, EPrimaryType.Stunts)]
    [InlineData(ESite.Nations, EPrimaryType.Puzzle)]
    [InlineData(ESite.TMNF, EPrimaryType.Platform)]
    [InlineData(ESite.TMNF, EPrimaryType.Stunts)]
    [InlineData(ESite.TMNF, EPrimaryType.Puzzle)]
    public void ValidateRequestRules_UnitedGamemodeWithTMNFOrNations_Throws(ESite site, EPrimaryType type)
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig
        {
            Rules = new()
            {
                RequestRules = new()
                {
                    Site = site,
                    PrimaryType = type
                }
            }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);

        Assert.Throws<RuleValidationException>(validator.ValidateRequestRules);
    }

    [Theory]
    [InlineData(ESite.Original, EEnvironment.Island)]
    [InlineData(ESite.Original, EEnvironment.Bay)]
    [InlineData(ESite.Original, EEnvironment.Coast)]
    [InlineData(ESite.Sunrise, EEnvironment.Snow)]
    [InlineData(ESite.Sunrise, EEnvironment.Desert)]
    [InlineData(ESite.Sunrise, EEnvironment.Rally)]
    [InlineData(ESite.Nations, EEnvironment.Island)]
    [InlineData(ESite.Nations, EEnvironment.Bay)]
    [InlineData(ESite.Nations, EEnvironment.Coast)]
    [InlineData(ESite.Nations, EEnvironment.Snow)]
    [InlineData(ESite.Nations, EEnvironment.Desert)]
    [InlineData(ESite.Nations, EEnvironment.Rally)]
    [InlineData(ESite.TMNF, EEnvironment.Island)]
    [InlineData(ESite.TMNF, EEnvironment.Bay)]
    [InlineData(ESite.TMNF, EEnvironment.Coast)]
    [InlineData(ESite.TMNF, EEnvironment.Snow)]
    [InlineData(ESite.TMNF, EEnvironment.Desert)]
    [InlineData(ESite.TMNF, EEnvironment.Rally)]
    public void ValidateRequestRules_SpecificEnvsWithOtherExchange_Throws(ESite site, EEnvironment env)
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig
        {
            Rules = new()
            {
                RequestRules = new()
                {
                    Site = site,
                    Environment = new() { env }
                }
            }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);

        Assert.Throws<RuleValidationException>(validator.ValidateRequestRules);
    }

    [Theory]
    [InlineData(ESite.Sunrise, EEnvironment.Island, EEnvironment.Bay)]
    [InlineData(ESite.Sunrise, EEnvironment.Island, EEnvironment.Coast)]
    [InlineData(ESite.Sunrise, EEnvironment.Bay, EEnvironment.Island)]
    [InlineData(ESite.Sunrise, EEnvironment.Bay, EEnvironment.Coast)]
    [InlineData(ESite.Sunrise, EEnvironment.Coast, EEnvironment.Bay)]
    [InlineData(ESite.Sunrise, EEnvironment.Coast, EEnvironment.Island)]
    [InlineData(ESite.Original, EEnvironment.Desert, EEnvironment.Snow)]
    [InlineData(ESite.Original, EEnvironment.Desert, EEnvironment.Rally)]
    [InlineData(ESite.Original, EEnvironment.Snow, EEnvironment.Desert)]
    [InlineData(ESite.Original, EEnvironment.Snow, EEnvironment.Rally)]
    [InlineData(ESite.Original, EEnvironment.Rally, EEnvironment.Snow)]
    [InlineData(ESite.Original, EEnvironment.Rally, EEnvironment.Desert)]
    public void ValidateRequestRules_EnvimixOnOldExchange_Throws(ESite site, EEnvironment env, EEnvironment vehicle)
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig
        {
            Rules = new()
            {
                RequestRules = new()
                {
                    Site = site,
                    Environment = new() { env },
                    Vehicle = new() { vehicle }
                }
            }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);

        Assert.Throws<RuleValidationException>(validator.ValidateRequestRules);
    }

    [Theory]
    [InlineData(ESite.Original, EEnvironment.Island)]
    [InlineData(ESite.Original, EEnvironment.Bay)]
    [InlineData(ESite.Original, EEnvironment.Coast)]
    [InlineData(ESite.Sunrise, EEnvironment.Snow)]
    [InlineData(ESite.Sunrise, EEnvironment.Desert)]
    [InlineData(ESite.Sunrise, EEnvironment.Rally)]
    [InlineData(ESite.Nations, EEnvironment.Island)]
    [InlineData(ESite.Nations, EEnvironment.Bay)]
    [InlineData(ESite.Nations, EEnvironment.Coast)]
    [InlineData(ESite.Nations, EEnvironment.Snow)]
    [InlineData(ESite.Nations, EEnvironment.Desert)]
    [InlineData(ESite.Nations, EEnvironment.Rally)]
    [InlineData(ESite.TMNF, EEnvironment.Island)]
    [InlineData(ESite.TMNF, EEnvironment.Bay)]
    [InlineData(ESite.TMNF, EEnvironment.Coast)]
    [InlineData(ESite.TMNF, EEnvironment.Snow)]
    [InlineData(ESite.TMNF, EEnvironment.Desert)]
    [InlineData(ESite.TMNF, EEnvironment.Rally)]
    public void ValidateRequestRules_SpecificVehiclesWithOtherExchange_Throws(ESite site, EEnvironment env)
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig
        {
            Rules = new()
            {
                RequestRules = new()
                {
                    Site = site,
                    Vehicle = new() { env }
                }
            }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);

        Assert.Throws<RuleValidationException>(validator.ValidateRequestRules);
    }

    [Theory]
    [InlineData(ESite.Any)]
    [InlineData(ESite.Original)]
    [InlineData(ESite.Nations)]
    [InlineData(ESite.TMNF)]
    public void ValidateRequestRules_EqualEnvAndVehicleDistributionWithNonTMUFExchange_Throws(ESite site)
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig
        {
            Rules = new()
            {
                RequestRules = new()
                {
                    Site = site,
                    EqualEnvironmentDistribution = true,
                    EqualVehicleDistribution = true
                }
            }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);

        Assert.Throws<RuleValidationException>(validator.ValidateRequestRules);
    }

    [Theory]
    [InlineData(ESite.Nations)]
    [InlineData(ESite.TMNF)]
    public void ValidateRequestRules_EqualEnvDistributionWithStadiumExchange_Throws(ESite site)
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig
        {
            Rules = new()
            {
                RequestRules = new()
                {
                    Site = site,
                    EqualEnvironmentDistribution = true
                }
            }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);

        Assert.Throws<RuleValidationException>(validator.ValidateRequestRules);
    }

    [Theory]
    [InlineData(ESite.Nations)]
    [InlineData(ESite.TMNF)]
    public void ValidateRequestRules_EqualVehicleDistributionWithStadiumExchange_Throws(ESite site)
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig
        {
            Rules = new()
            {
                RequestRules = new()
                {
                    Site = site,
                    EqualVehicleDistribution = true
                }
            }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);

        Assert.Throws<RuleValidationException>(validator.ValidateRequestRules);
    }

    [Theory]
    [InlineData(ESite.Original, EEnvironment.Snow)]
    [InlineData(ESite.Original, EEnvironment.Desert)]
    [InlineData(ESite.Original, EEnvironment.Rally)]
    [InlineData(ESite.Sunrise, EEnvironment.Island)]
    [InlineData(ESite.Sunrise, EEnvironment.Bay)]
    [InlineData(ESite.Sunrise, EEnvironment.Coast)]
    [InlineData(ESite.Nations, EEnvironment.Stadium)]
    [InlineData(ESite.TMNF, EEnvironment.Stadium)]
    [InlineData(ESite.TMUF, EEnvironment.Snow)]
    [InlineData(ESite.TMUF, EEnvironment.Desert)]
    [InlineData(ESite.TMUF, EEnvironment.Rally)]
    [InlineData(ESite.TMUF, EEnvironment.Island)]
    [InlineData(ESite.TMUF, EEnvironment.Bay)]
    [InlineData(ESite.TMUF, EEnvironment.Coast)]
    [InlineData(ESite.TMUF, EEnvironment.Stadium)]
    public void ValidateRequestRules_ValidEnvironmentWithExchange_DoesNotThrow(ESite site, EEnvironment env)
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig
        {
            Rules = new()
            {
                RequestRules = new()
                {
                    Site = site,
                    Environment = new() { env }
                }
            }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);
        
        var exception = Record.Exception(validator.ValidateRequestRules);

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(ESite.Original, EEnvironment.Snow)]
    [InlineData(ESite.Original, EEnvironment.Desert)]
    [InlineData(ESite.Original, EEnvironment.Rally)]
    [InlineData(ESite.Sunrise, EEnvironment.Island)]
    [InlineData(ESite.Sunrise, EEnvironment.Bay)]
    [InlineData(ESite.Sunrise, EEnvironment.Coast)]
    [InlineData(ESite.Nations, EEnvironment.Stadium)]
    [InlineData(ESite.TMNF, EEnvironment.Stadium)]
    [InlineData(ESite.TMUF, EEnvironment.Snow)]
    [InlineData(ESite.TMUF, EEnvironment.Desert)]
    [InlineData(ESite.TMUF, EEnvironment.Rally)]
    [InlineData(ESite.TMUF, EEnvironment.Island)]
    [InlineData(ESite.TMUF, EEnvironment.Bay)]
    [InlineData(ESite.TMUF, EEnvironment.Coast)]
    [InlineData(ESite.TMUF, EEnvironment.Stadium)]
    public void ValidateRequestRules_ValidVehicleWithExchange_DoesNotThrow(ESite site, EEnvironment env)
    {
        var autosaveScanner = new Mock<IAutosaveScanner>().Object;
        var additionalData = new Mock<IAdditionalData>().Object;

        var config = new RandomizerConfig
        {
            Rules = new()
            {
                RequestRules = new()
                {
                    Site = site,
                    Vehicle = new() { env }
                }
            }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);

        var exception = Record.Exception(validator.ValidateRequestRules);

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateMap_AutosavesContainUid_ReturnsFalse()
    {
        var autosaveHeaders = new ConcurrentDictionary<string, AutosaveHeader>();
        autosaveHeaders["mapuid"] = new AutosaveHeader("path/to/file.Replay.Gbx", (CGameCtnReplayRecord)Activator.CreateInstance(typeof(CGameCtnReplayRecord), nonPublic: true)!);

        var mockAutosaveScanner = new Mock<IAutosaveScanner>();
        mockAutosaveScanner.SetupGet(x => x.AutosaveHeaders).Returns(autosaveHeaders);
        var autosaveScanner = mockAutosaveScanner.Object;

        var additionalData = new Mock<IAdditionalData>().Object;
        var config = new RandomizerConfig();

        var validator = new Validator(autosaveScanner, additionalData, config);

        var map = (CGameCtnChallenge)Activator.CreateInstance(typeof(CGameCtnChallenge), nonPublic: true)!;
        map.MapUid = "mapuid";

        var result = validator.ValidateMap(map, out string? invalidBlock);

        Assert.Null(invalidBlock);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMap_MapNotPlayedAndNoUnlimiter_ReturnsTrue()
    {
        var mockAutosaveScanner = new Mock<IAutosaveScanner>();
        mockAutosaveScanner.SetupGet(x => x.AutosaveHeaders).Returns(new ConcurrentDictionary<string, AutosaveHeader>());
        var autosaveScanner = mockAutosaveScanner.Object;

        var additionalData = new Mock<IAdditionalData>().Object;
        var config = new RandomizerConfig
        {
            Rules = new()
            {
                NoUnlimiter = false
            }
        };

        var validator = new Validator(autosaveScanner, additionalData, config);

        var map = (CGameCtnChallenge)Activator.CreateInstance(typeof(CGameCtnChallenge), nonPublic: true)!;
        map.MapUid = "mapuid";

        var result = validator.ValidateMap(map, out string? invalidBlock);

        Assert.Null(invalidBlock);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMap_NoUnlimiterHasUnlimiterChunk_ReturnsFalse()
    {
        var mockAutosaveScanner = new Mock<IAutosaveScanner>();
        mockAutosaveScanner.SetupGet(x => x.AutosaveHeaders).Returns(new ConcurrentDictionary<string, AutosaveHeader>());
        var autosaveScanner = mockAutosaveScanner.Object;

        var additionalData = new Mock<IAdditionalData>().Object;
        var config = new RandomizerConfig();

        var validator = new Validator(autosaveScanner, additionalData, config);

        var map = (CGameCtnChallenge)Activator.CreateInstance(typeof(CGameCtnChallenge), nonPublic: true)!;
        map.MapUid = "mapuid";
        map.Chunks.Add(new SkippableChunk<CGameCtnChallenge>(map, Array.Empty<byte>(), 0x3F001000));

        var result = validator.ValidateMap(map, out string? invalidBlock);

        Assert.Null(invalidBlock);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMap_NoUnlimiterNullMapSize_ReturnsFalse()
    {
        var mockAutosaveScanner = new Mock<IAutosaveScanner>();
        mockAutosaveScanner.SetupGet(x => x.AutosaveHeaders).Returns(new ConcurrentDictionary<string, AutosaveHeader>());
        var autosaveScanner = mockAutosaveScanner.Object;

        var additionalData = new Mock<IAdditionalData>().Object;
        var config = new RandomizerConfig();

        var validator = new Validator(autosaveScanner, additionalData, config);

        var map = (CGameCtnChallenge)Activator.CreateInstance(typeof(CGameCtnChallenge), nonPublic: true)!;
        map.MapUid = "mapuid";
        map.Size = null;

        var result = validator.ValidateMap(map, out string? invalidBlock);

        Assert.Null(invalidBlock);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMap_NoUnlimiterMapSizesDoesNotContainCollection_ReturnsFalse()
    {
        var mockAutosaveScanner = new Mock<IAutosaveScanner>();
        mockAutosaveScanner.SetupGet(x => x.AutosaveHeaders).Returns(new ConcurrentDictionary<string, AutosaveHeader>());
        var autosaveScanner = mockAutosaveScanner.Object;

        var mockAdditionalData = new Mock<IAdditionalData>();
        mockAdditionalData.SetupGet(x => x.MapSizes).Returns(new Dictionary<string, HashSet<Int3>>());
        var additionalData = mockAdditionalData.Object;
        
        var config = new RandomizerConfig();

        var validator = new Validator(autosaveScanner, additionalData, config);

        var map = (CGameCtnChallenge)Activator.CreateInstance(typeof(CGameCtnChallenge), nonPublic: true)!;
        map.MapUid = "mapuid";
        map.Collection = "Rally";
        map.Size = (45, 32, 45);

        var result = validator.ValidateMap(map, out string? invalidBlock);

        Assert.Null(invalidBlock);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMap_NoUnlimiterInvalidMapSize_ReturnsFalse()
    {
        var mockAutosaveScanner = new Mock<IAutosaveScanner>();
        mockAutosaveScanner.SetupGet(x => x.AutosaveHeaders).Returns(new ConcurrentDictionary<string, AutosaveHeader>());
        var autosaveScanner = mockAutosaveScanner.Object;

        var mockAdditionalData = new Mock<IAdditionalData>();
        mockAdditionalData.SetupGet(x => x.MapSizes).Returns(new Dictionary<string, HashSet<Int3>>
        {
            { "Rally", new HashSet<Int3> { (45, 32, 45) } }
        });
        var additionalData = mockAdditionalData.Object;

        var config = new RandomizerConfig();

        var validator = new Validator(autosaveScanner, additionalData, config);

        var map = (CGameCtnChallenge)Activator.CreateInstance(typeof(CGameCtnChallenge), nonPublic: true)!;
        map.MapUid = "mapuid";
        map.Collection = "Rally";
        map.Size = (255, 32, 255);

        var result = validator.ValidateMap(map, out string? invalidBlock);

        Assert.Null(invalidBlock);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMap_NoUnlimiterOfficialBlocksDoesNotContainCollection_ReturnsFalse()
    {
        var mockAutosaveScanner = new Mock<IAutosaveScanner>();
        mockAutosaveScanner.SetupGet(x => x.AutosaveHeaders).Returns(new ConcurrentDictionary<string, AutosaveHeader>());
        var autosaveScanner = mockAutosaveScanner.Object;

        var mockAdditionalData = new Mock<IAdditionalData>();
        mockAdditionalData.SetupGet(x => x.MapSizes).Returns(new Dictionary<string, HashSet<Int3>>
        {
            { "Rally", new HashSet<Int3> { (45, 32, 45) } }
        });
        mockAdditionalData.SetupGet(x => x.OfficialBlocks).Returns(new Dictionary<string, HashSet<string>>());
        var additionalData = mockAdditionalData.Object;

        var config = new RandomizerConfig();

        var validator = new Validator(autosaveScanner, additionalData, config);

        var map = (CGameCtnChallenge)Activator.CreateInstance(typeof(CGameCtnChallenge), nonPublic: true)!;
        map.MapUid = "mapuid";
        map.Collection = "Rally";
        map.Size = (45, 32, 45);

        var result = validator.ValidateMap(map, out string? invalidBlock);

        Assert.Null(invalidBlock);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMap_NoUnlimiterHasUnofficialBlock_ReturnsFalse()
    {
        var mockAutosaveScanner = new Mock<IAutosaveScanner>();
        mockAutosaveScanner.SetupGet(x => x.AutosaveHeaders).Returns(new ConcurrentDictionary<string, AutosaveHeader>());
        var autosaveScanner = mockAutosaveScanner.Object;

        var mockAdditionalData = new Mock<IAdditionalData>();
        mockAdditionalData.SetupGet(x => x.MapSizes).Returns(new Dictionary<string, HashSet<Int3>>
        {
            { "Rally", new HashSet<Int3> { (45, 32, 45) } }
        });
        mockAdditionalData.SetupGet(x => x.OfficialBlocks).Returns(new Dictionary<string, HashSet<string>>
        {
            { "Rally", new HashSet<string> { "OfficialBlock1" } }
        });
        var additionalData = mockAdditionalData.Object;

        var config = new RandomizerConfig();

        var validator = new Validator(autosaveScanner, additionalData, config);

        var map = (CGameCtnChallenge)Activator.CreateInstance(typeof(CGameCtnChallenge), nonPublic: true)!;
        map.MapUid = "mapuid";
        map.Collection = "Rally";
        map.Size = (45, 32, 45);
        map.Blocks = new List<CGameCtnBlock> { CGameCtnBlock.Unassigned1 };

        var result = validator.ValidateMap(map, out string? invalidBlock);

        Assert.Equal(expected: "Unassigned1", actual: invalidBlock);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMap_NoUnlimiterHasValidMapSizeAndBlocks_ReturnsTrue()
    {
        var mockAutosaveScanner = new Mock<IAutosaveScanner>();
        mockAutosaveScanner.SetupGet(x => x.AutosaveHeaders).Returns(new ConcurrentDictionary<string, AutosaveHeader>());
        var autosaveScanner = mockAutosaveScanner.Object;

        var mockAdditionalData = new Mock<IAdditionalData>();
        mockAdditionalData.SetupGet(x => x.MapSizes).Returns(new Dictionary<string, HashSet<Int3>>
        {
            { "Rally", new HashSet<Int3> { (45, 32, 45) } }
        });
        mockAdditionalData.SetupGet(x => x.OfficialBlocks).Returns(new Dictionary<string, HashSet<string>>
        {
            { "Rally", new HashSet<string> { "Unassigned1" } }
        });
        var additionalData = mockAdditionalData.Object;

        var config = new RandomizerConfig();

        var validator = new Validator(autosaveScanner, additionalData, config);

        var map = (CGameCtnChallenge)Activator.CreateInstance(typeof(CGameCtnChallenge), nonPublic: true)!;
        map.MapUid = "mapuid";
        map.Collection = "Rally";
        map.Size = (45, 32, 45);
        map.Blocks = new List<CGameCtnBlock> { CGameCtnBlock.Unassigned1 };

        var result = validator.ValidateMap(map, out string? invalidBlock);

        Assert.Null(invalidBlock);
        Assert.True(result);
    }
}
