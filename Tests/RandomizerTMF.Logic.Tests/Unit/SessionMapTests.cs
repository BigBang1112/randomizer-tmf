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
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorTime = new TimeInt32(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new TimeInt32(80)
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsAuthorMedal_RaceOrPuzzle_ReplayTimeEqualToAuthorTime_ReturnsTrue(CGameCtnChallenge.PlayMode mode)
        {
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorTime = new TimeInt32(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new TimeInt32(100)
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsAuthorMedal_RaceOrPuzzle_ReplayTimeGreaterThanAuthorTime_ReturnsFalse(CGameCtnChallenge.PlayMode mode)
        {
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorTime = new TimeInt32(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(120)
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsLessThanAuthorScoreAndReplayTimeLessThanAuthorTime_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorScore = 3,
                    AuthorTime = new TimeInt32(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(80),
                Respawns = 2
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsEqualToAuthorScoreAndReplayTimeEqualToAuthorTime_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorScore = 3,
                    AuthorTime = new TimeInt32(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(100),
                Respawns = 3
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsGreaterThanAuthorScoreAndReplayTimeLessThanAuthorTime_ReturnsFalse()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorScore = 3,
                    AuthorTime = new TimeInt32(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 5,
                RaceTime = new(80)
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsGreaterThanAuthorScoreAndReplayTimeEqualToAuthorTime_ReturnsFalse()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorScore = 3,
                    AuthorTime = new TimeInt32(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 5,
                RaceTime = new(100)
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsIsZeroAndReplayTimeLessThanAuthorTime_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorScore = 3,
                    AuthorTime = new TimeInt32(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 0,
                RaceTime = new(80)
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_Platform_RespawnsIsZeroAndReplayTimeEqualToAuthorTime_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorScore = 0,
                    AuthorTime = new TimeInt32(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 0,
                RaceTime = new(100)
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_PlatformMode_RespawnsIsZeroAndReplayTimeGreaterThanAuthorTime_ReturnsFalse()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorScore = 0,
                    AuthorTime = new TimeInt32(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 0,
                RaceTime = new(120)
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsAuthorMedal_Stunts_ScoreGreaterThanAuthorScore_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorScore = 200
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 250
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_Stunts_ScoreEqualToAuthorScore_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorScore = 200
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 200
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorMedal_Stunts_ScoreIsLessThanAuthorScore_ReturnsFalse()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    AuthorScore = 200
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 150
            };

            var result = sessionMap.IsAuthorMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsAuthorMedal_UnsupportedGamemode_ShouldThrow()
        {
            var map = new CGameCtnChallenge
            {
                Mode = (CGameCtnChallenge.PlayMode)999,
                ChallengeParameters = new CGameCtnChallengeParameters()
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost();

            Assert.Throws<NotSupportedException>(() => sessionMap.IsAuthorMedal(ghost));
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsGoldMedal_RaceOrPuzzle_RaceTimeLessThanGoldTime_ReturnsTrue(CGameCtnChallenge.PlayMode mode)
        {
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    GoldTime = new(60)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(50)
            };

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsGoldMedal_RaceOrPuzzle_RaceTimeEqualToGoldTime_ReturnsTrue(CGameCtnChallenge.PlayMode mode)
        {
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    GoldTime = new(60)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(60)
            };

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsGoldMedal_RaceOrPuzzle_RaceTimeGreaterThanGoldTime_ReturnsFalse(CGameCtnChallenge.PlayMode mode)
        {
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    GoldTime = new(60)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(70)
            };

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsGoldMedal_Platform_RespawnsLessThanGold_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    GoldTime = new(120)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 100
            };

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsGoldMedal_Platform_RespawnsEqualToGold_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    GoldTime = new(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 100
            };

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsGoldMedal_Platform_RespawnsIsGreaterThanGoldTime_ReturnsFalse()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    GoldTime = new(120)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 150
            };

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsGoldMedal_Stunts_ScoreGreaterThanGoldTime_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    GoldTime = new(200)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 250
            };

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsGoldMedal_Stunts_ScoreGreaterEqualToGoldTime_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    GoldTime = new(250)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 250
            };

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsGoldMedal_Stunts_ScoreLessThanGoldTime_ReturnsFalse()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    GoldTime = new(200)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 150
            };

            var result = sessionMap.IsGoldMedal(ghost);

            Assert.False(result);
        }
        
        [Fact]
        public void IsGoldMedal_UnsupportedGamemode_ShouldThrow()
        {
            var map = new CGameCtnChallenge
            {
                Mode = (CGameCtnChallenge.PlayMode)999,
                ChallengeParameters = new CGameCtnChallengeParameters()
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost();

            Assert.Throws<NotSupportedException>(() => sessionMap.IsGoldMedal(ghost));
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsSilverMedal_RaceOrPuzzle_RaceTimeLessThanSilverTime_ReturnsTrue(CGameCtnChallenge.PlayMode mode)
        {
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    SilverTime = new(60)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(50)
            };

            var result = sessionMap.IsSilverMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsSilverMedal_RaceOrPuzzle_RaceTimeEqualToSilverTime_ReturnsTrue(CGameCtnChallenge.PlayMode mode)
        {
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    SilverTime = new(60)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(60)
            };

            var result = sessionMap.IsSilverMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsSilverMedal_RaceOrPuzzle_RaceTimeGreaterThanSilverTime_ReturnsFalse(CGameCtnChallenge.PlayMode mode)
        {
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    SilverTime = new(60)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(70)
            };

            var result = sessionMap.IsSilverMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsSilverMedal_Platform_RespawnsLessThanSilver_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    SilverTime = new(120)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 100
            };

            var result = sessionMap.IsSilverMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsSilverMedal_Platform_RespawnsEqualToSilver_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    SilverTime = new(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 100
            };

            var result = sessionMap.IsSilverMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsSilverMedal_Platform_RespawnsIsGreaterThanSilverTime_ReturnsFalse()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    SilverTime = new(120)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 150
            };

            var result = sessionMap.IsSilverMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsSilverMedal_Stunts_ScoreGreaterThanSilverTime_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    SilverTime = new(200)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 250
            };

            var result = sessionMap.IsSilverMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsSilverMedal_Stunts_ScoreGreaterEqualToSilverTime_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    SilverTime = new(250)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 250
            };

            var result = sessionMap.IsSilverMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsSilverMedal_Stunts_ScoreLessThanSilverTime_ReturnsFalse()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    SilverTime = new(200)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 150
            };

            var result = sessionMap.IsSilverMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsSilverMedal_UnsupportedGamemode_ShouldThrow()
        {
            var map = new CGameCtnChallenge
            {
                Mode = (CGameCtnChallenge.PlayMode)999,
                ChallengeParameters = new CGameCtnChallengeParameters()
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost();

            Assert.Throws<NotSupportedException>(() => sessionMap.IsSilverMedal(ghost));
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsBronzeMedal_RaceOrPuzzle_RaceTimeLessThanBronzeTime_ReturnsTrue(CGameCtnChallenge.PlayMode mode)
        {
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    BronzeTime = new(60)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(50)
            };

            var result = sessionMap.IsBronzeMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsBronzeMedal_RaceOrPuzzle_RaceTimeEqualToBronzeTime_ReturnsTrue(CGameCtnChallenge.PlayMode mode)
        {
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    BronzeTime = new(60)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(60)
            };

            var result = sessionMap.IsBronzeMedal(ghost);

            Assert.True(result);
        }

        [Theory]
        [InlineData(CGameCtnChallenge.PlayMode.Race)]
        [InlineData(CGameCtnChallenge.PlayMode.Puzzle)]
        public void IsBronzeMedal_RaceOrPuzzle_RaceTimeGreaterThanBronzeTime_ReturnsFalse(CGameCtnChallenge.PlayMode mode)
        {
            var map = new CGameCtnChallenge
            {
                Mode = mode,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    BronzeTime = new(60)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                RaceTime = new(70)
            };

            var result = sessionMap.IsBronzeMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsBronzeMedal_Platform_RespawnsLessThanBronze_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    BronzeTime = new(120)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 100
            };

            var result = sessionMap.IsBronzeMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsBronzeMedal_Platform_RespawnsEqualToBronze_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    BronzeTime = new(100)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 100
            };

            var result = sessionMap.IsBronzeMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsBronzeMedal_Platform_RespawnsIsGreaterThanBronzeTime_ReturnsFalse()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Platform,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    BronzeTime = new(120)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                Respawns = 150
            };

            var result = sessionMap.IsBronzeMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsBronzeMedal_Stunts_ScoreGreaterThanBronzeTime_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    BronzeTime = new(200)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 250
            };

            var result = sessionMap.IsBronzeMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsBronzeMedal_Stunts_ScoreGreaterEqualToBronzeTime_ReturnsTrue()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    BronzeTime = new(250)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 250
            };

            var result = sessionMap.IsBronzeMedal(ghost);

            Assert.True(result);
        }

        [Fact]
        public void IsBronzeMedal_Stunts_ScoreLessThanBronzeTime_ReturnsFalse()
        {
            var map = new CGameCtnChallenge
            {
                Mode = CGameCtnChallenge.PlayMode.Stunts,
                ChallengeParameters = new CGameCtnChallengeParameters
                {
                    BronzeTime = new(200)
                }
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost
            {
                StuntScore = 150
            };

            var result = sessionMap.IsBronzeMedal(ghost);

            Assert.False(result);
        }

        [Fact]
        public void IsBronzeMedal_UnsupportedGamemode_ShouldThrow()
        {
            var map = new CGameCtnChallenge
            {
                Mode = (CGameCtnChallenge.PlayMode)999,
                ChallengeParameters = new CGameCtnChallengeParameters()
            };

            var sessionMap = new SessionMap(map, DateTimeOffset.UtcNow, "");

            var ghost = new CGameCtnGhost();

            Assert.Throws<NotSupportedException>(() => sessionMap.IsBronzeMedal(ghost));
        }
    }
}
