using GBX.NET.Engines.Game;
using TmEssentials;

namespace RandomizerTMF.Logic.Tests.Unit
{
    public class SessionMapTests
    {
        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsAuthorMedal_RaceOrPuzzle_ReplayTimeLessThanAuthorTime_ReturnsTrue(CGameCtnChallenge.PlayMode mode)
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = mode;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorTime = new TimeInt32(100);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.RaceTime = new TimeInt32(80);

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsAuthorMedal_RaceOrPuzzle_ReplayTimeEqualToAuthorTime_ReturnsTrue(CGameCtnChallenge.PlayMode mode)
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = mode;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorTime = new TimeInt32(100);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.RaceTime = new TimeInt32(100);

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsAuthorMedal_RaceOrPuzzle_ReplayTimeGreaterThanAuthorTime_ReturnsFalse(CGameCtnChallenge.PlayMode mode)
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = mode;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorTime = new TimeInt32(100);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.RaceTime = new(120);

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsLessThanAuthorScoreAndReplayTimeLessThanAuthorTime_ReturnsTrue()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Platform;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorScore = 3;
            map.ChallengeParameters.AuthorTime = new TimeInt32(100);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.RaceTime = new(80);
            ghost.Respawns = 2;

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsEqualToAuthorScoreAndReplayTimeEqualToAuthorTime_ReturnsTrue()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Platform;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorScore = 3;
            map.ChallengeParameters.AuthorTime = new TimeInt32(100);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.RaceTime = new(100);
            ghost.Respawns = 3;

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsGreaterThanAuthorScoreAndReplayTimeLessThanAuthorTime_ReturnsFalse()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Platform;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorScore = 3;
            map.ChallengeParameters.AuthorTime = new TimeInt32(100);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.Respawns = 5;
            ghost.RaceTime = new(80);

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsGreaterThanAuthorScoreAndReplayTimeEqualToAuthorTime_ReturnsFalse()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Platform;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorScore = 3;
            map.ChallengeParameters.AuthorTime = new TimeInt32(100);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.Respawns = 5;
            ghost.RaceTime = new(100);

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsIsZeroAndReplayTimeLessThanAuthorTime_ReturnsTrue()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Platform;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorScore = 3;
            map.ChallengeParameters.AuthorTime = new TimeInt32(100);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.Respawns = 0;
            ghost.RaceTime = new(80);

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsIsZeroAndReplayTimeEqualToAuthorTime_ReturnsTrue()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Platform;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorScore = 0;
            map.ChallengeParameters.AuthorTime = new TimeInt32(100);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.Respawns = 0;
            ghost.RaceTime = new(100);

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_PlatformMode_RespawnsIsZeroAndReplayTimeGreaterThanAuthorTime_ReturnsFalse()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Platform;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorScore = 0;
            map.ChallengeParameters.AuthorTime = new TimeInt32(100);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.Respawns = 0;
            ghost.RaceTime = new(120);

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsAuthorMedal_Stunts_ScoreGreaterThanAuthorScore_ReturnsTrue()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Stunts;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorScore = 200;

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.StuntScore = 250;

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_Stunts_ScoreEqualToAuthorScore_ReturnsTrue()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Stunts;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorScore = 200;

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.StuntScore = 200;

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_Stunts_ScoreIsLessThanAuthorScore_ReturnsFalse()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Stunts;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.AuthorScore = 200;

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.StuntScore = 150;

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsAuthorMedal_UnsupportedGamemode_ShouldThrow()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = (CGameCtnChallenge.PlayMode)999;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();

            Assert.Throws<NotSupportedException>(() => sessionMap.IsAuthorMedal(ghost));
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsGoldMedal_RaceOrPuzzle_RaceTimeLessThanGoldTime_ReturnsTrue(CGameCtnChallenge.PlayMode mode)
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = mode;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.GoldTime = new(60);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.RaceTime = new(50);

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsGoldMedal_RaceOrPuzzle_RaceTimeEqualToGoldTime_ReturnsTrue(CGameCtnChallenge.PlayMode mode)
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = mode;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.GoldTime = new(60);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.RaceTime = new(60);

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsGoldMedal_RaceOrPuzzle_RaceTimeGreaterThanGoldTime_ReturnsFalse(CGameCtnChallenge.PlayMode mode)
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = mode;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.GoldTime = new(60);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.RaceTime = new(70);

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsGoldMedal_Platform_RespawnsLessThanGold_ReturnsTrue()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Platform;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.GoldTime = new(120);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.Respawns = 100;

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsGoldMedal_Platform_RespawnsEqualToGold_ReturnsTrue()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Platform;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.GoldTime = new(100);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.Respawns = 100;

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsGoldMedal_Platform_RespawnsIsGreaterThanGoldTime_ReturnsFalse()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Platform;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.GoldTime = new(120);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.Respawns = 150;

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsGoldMedal_Stunts_ScoreGreaterThanGoldTime_ReturnsTrue()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Stunts;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.GoldTime = new(200);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.StuntScore = 250;

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsGoldMedal_Stunts_ScoreGreaterEqualToGoldTime_ReturnsTrue()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Stunts;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.GoldTime = new(250);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.StuntScore = 250;

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsGoldMedal_Stunts_ScoreLessThanGoldTime_ReturnsFalse()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = CGameCtnChallenge.PlayMode.Stunts;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();
            map.ChallengeParameters.GoldTime = new(200);

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();
            ghost.StuntScore = 150;

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.False(result);
        }
        
        [Fact]
        public void IsGoldMedal_UnsupportedGamemode_ShouldThrow()
        {
            var map = NodeInstance.Create<CGameCtnChallenge>();
            map.Mode = (CGameCtnChallenge.PlayMode)999;
            map.ChallengeParameters = NodeInstance.Create<CGameCtnChallengeParameters>();

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = NodeInstance.Create<CGameCtnGhost>();

            Assert.Throws<NotSupportedException>(() => sessionMap.IsGoldMedal(ghost));
        }
    }
}
