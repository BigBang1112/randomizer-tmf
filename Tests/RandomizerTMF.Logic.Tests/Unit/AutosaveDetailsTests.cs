using TmEssentials;

using static GBX.NET.Engines.Game.CGameCtnChallenge;

namespace RandomizerTMF.Logic.Tests.Unit;

public class AutosaveDetailsTests
{
    [Theory]
    [InlineData(                                          false,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      60001,            110,            2)]
    [InlineData(                                           true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      60000,            110,            2)]
    [InlineData(                                           true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      50000,            130,            1)]
    [InlineData(                                           true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      30000,              0,            0)]
    [InlineData(                                          false,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      60001,            110,            2)]
    [InlineData(                                           true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      60000,            110,            2)]
    [InlineData(                                           true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      50000,            130,            1)]
    [InlineData(                                           true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      30000,              0,            0)]
    [InlineData(                                           true, PlayMode.Platform,          6,          4,        2,          32000,               1,      30000,             10,            3)]
    [InlineData(                                           true, PlayMode.Platform,          6,          4,        2,          32000,               1,      35000,              4,            6)]
    [InlineData(                                          false, PlayMode.Platform,          6,          4,        2,          32000,               1,      30000,             10,            7)]
    [InlineData(                                           true,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            250,            2)]
    [InlineData(                                           true,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            200,            2)]
    [InlineData(                                          false,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            199,            2)]
    [InlineData(                                          false,   PlayMode.Script,        200,        400,      600,          32000,             650,      30000,            200,            2)]
    public void HasBronzeMedal_ReturnsCorrectBool(bool expected,     PlayMode mode, int bronze, int silver, int gold, int authorTime, int authorScore, int timeMs, int stuntScore, int respawns)
    {
        // Arrange
        var details = new AutosaveDetails(
            Time: new TimeInt32(timeMs),
            Score: stuntScore,
            Respawns: respawns,
            MapName: null,
            MapEnvironment: null,
            MapCar: null,
            MapBronzeTime: new TimeInt32(bronze),
            MapSilverTime: new TimeInt32(silver),
            MapGoldTime: new TimeInt32(gold),
            MapAuthorTime: new TimeInt32(authorTime),
            MapAuthorScore: authorScore,
            MapMode: mode);

        Assert.Equal(expected, actual: details.HasBronzeMedal);
    }

    [Theory]
    [InlineData(                                          false,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      40001,            110,            2)]
    [InlineData(                                           true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      40000,            110,            2)]
    [InlineData(                                           true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      30000,            130,            1)]
    [InlineData(                                           true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      30000,              0,            0)]
    [InlineData(                                          false,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      40001,            110,            2)]
    [InlineData(                                           true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      40000,            110,            2)]
    [InlineData(                                           true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      30000,            130,            1)]
    [InlineData(                                           true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      30000,              0,            0)]
    [InlineData(                                           true, PlayMode.Platform,          6,          4,        2,          32000,               1,      30000,             10,            3)]
    [InlineData(                                           true, PlayMode.Platform,          6,          4,        2,          32000,               1,      35000,              4,            4)]
    [InlineData(                                          false, PlayMode.Platform,          6,          4,        2,          32000,               1,      30000,             10,            5)]
    [InlineData(                                           true,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            550,            2)]
    [InlineData(                                           true,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            400,            2)]
    [InlineData(                                          false,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            399,            2)]
    [InlineData(                                          false,   PlayMode.Script,        200,        400,      600,          32000,             650,      30000,            400,            2)]
    public void HasSilverMedal_ReturnsCorrectBool(bool expected,     PlayMode mode, int bronze, int silver, int gold, int authorTime, int authorScore, int timeMs, int stuntScore, int respawns)
    {
        // Arrange
        var details = new AutosaveDetails(
            Time: new TimeInt32(timeMs),
            Score: stuntScore,
            Respawns: respawns,
            MapName: null,
            MapEnvironment: null,
            MapCar: null,
            MapBronzeTime: new TimeInt32(bronze),
            MapSilverTime: new TimeInt32(silver),
            MapGoldTime: new TimeInt32(gold),
            MapAuthorTime: new TimeInt32(authorTime),
            MapAuthorScore: authorScore,
            MapMode: mode);

        Assert.Equal(expected, actual: details.HasSilverMedal);
    }

    [Theory]
    [InlineData(                                        false,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      35001,            110,            2)]
    [InlineData(                                         true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      35000,            110,            2)]
    [InlineData(                                         true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      30000,            130,            1)]
    [InlineData(                                         true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      30000,              0,            0)]
    [InlineData(                                        false,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      35001,            110,            2)]
    [InlineData(                                         true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      35000,            110,            2)]
    [InlineData(                                         true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      30000,            130,            1)]
    [InlineData(                                         true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      30000,              0,            0)]
    [InlineData(                                         true, PlayMode.Platform,          6,          4,        2,          32000,               1,      30000,             10,            0)]
    [InlineData(                                         true, PlayMode.Platform,          6,          4,        2,          32000,               1,      35000,              4,            2)]
    [InlineData(                                        false, PlayMode.Platform,          6,          4,        2,          32000,               1,      30000,             10,            3)]
    [InlineData(                                         true,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            850,            2)]
    [InlineData(                                         true,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            600,            2)]
    [InlineData(                                        false,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            599,            2)]
    [InlineData(                                        false,   PlayMode.Script,        200,        400,      600,          32000,             650,      30000,            602,            2)]
    public void HasGoldMedal_ReturnsCorrectBool(bool expected,     PlayMode mode, int bronze, int silver, int gold, int authorTime, int authorScore, int timeMs, int stuntScore, int respawns)
    {
        // Arrange
        var details = new AutosaveDetails(
            Time: new TimeInt32(timeMs),
            Score: stuntScore,
            Respawns: respawns,
            MapName: null,
            MapEnvironment: null,
            MapCar: null,
            MapBronzeTime: new TimeInt32(bronze),
            MapSilverTime: new TimeInt32(silver),
            MapGoldTime: new TimeInt32(gold),
            MapAuthorTime: new TimeInt32(authorTime),
            MapAuthorScore: authorScore,
            MapMode: mode);

        Assert.Equal(expected, actual: details.HasGoldMedal);
    }

    [Theory]
    [InlineData(                                          false,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      32001,            110,            2)]
    [InlineData(                                           true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      32000,            110,            2)]
    [InlineData(                                           true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      30000,            130,            1)]
    [InlineData(                                           true,     PlayMode.Race,      60000,      40000,    35000,          32000,             300,      30000,              0,            0)]
    [InlineData(                                          false,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      32001,            110,            2)]
    [InlineData(                                           true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      32000,            110,            2)]
    [InlineData(                                           true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      30000,            130,            1)]
    [InlineData(                                           true,   PlayMode.Puzzle,      60000,      40000,    35000,          32000,             300,      30000,              0,            0)]
    [InlineData(                                           true, PlayMode.Platform,          6,          4,        2,          32000,               1,      30000,             10,            0)]
    [InlineData(                                           true, PlayMode.Platform,          6,          4,        2,          32000,               1,      35000,              4,            1)]
    [InlineData(                                          false, PlayMode.Platform,          6,          4,        2,          32000,               1,      30000,             10,            2)]
    [InlineData(                                          false, PlayMode.Platform,          6,          4,        2,          32000,               0,      34000,             10,            0)]
    [InlineData(                                          false, PlayMode.Platform,          6,          4,        2,          32000,               0,      32001,             10,            0)]
    [InlineData(                                           true, PlayMode.Platform,          6,          4,        2,          32000,               0,      32000,             10,            0)]
    [InlineData(                                           true,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            700,            2)]
    [InlineData(                                           true,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            650,            2)]
    [InlineData(                                          false,   PlayMode.Stunts,        200,        400,      600,          32000,             650,      30000,            649,            2)]
    [InlineData(                                          false,   PlayMode.Script,        200,        400,      600,          32000,             650,      30000,            602,            2)]
    public void HasAuthorMedal_ReturnsCorrectBool(bool expected,     PlayMode mode, int bronze, int silver, int gold, int authorTime, int authorScore, int timeMs, int stuntScore, int respawns)
    {
        // Arrange
        var details = new AutosaveDetails(
            Time: new TimeInt32(timeMs),
            Score: stuntScore,
            Respawns: respawns,
            MapName: null,
            MapEnvironment: null,
            MapCar: null,
            MapBronzeTime: new TimeInt32(bronze),
            MapSilverTime: new TimeInt32(silver),
            MapGoldTime: new TimeInt32(gold),
            MapAuthorTime: new TimeInt32(authorTime),
            MapAuthorScore: authorScore,
            MapMode: mode);

        Assert.Equal(expected, actual: details.HasAuthorMedal);
    }
}
