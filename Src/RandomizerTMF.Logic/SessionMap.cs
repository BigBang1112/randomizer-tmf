using GBX.NET;
using GBX.NET.Engines.Game;

namespace RandomizerTMF.Logic;

/// <summary>
/// Real-time session map object
/// </summary>
public class SessionMap : ISessionMap
{
    public CGameCtnChallenge Map { get; }
    public DateTimeOffset ReceivedAt { get; }
    public string TmxLink { get; }
    
    public TimeSpan? LastTimestamp { get; set; }
    
    public string? FilePath { get; set; }

    public CGameCtnChallengeParameters ChallengeParameters => Map.ChallengeParameters ?? throw new InvalidOperationException("Map does not have challenge parameters.");
    public CGameCtnChallenge.PlayMode? Mode => Map.Mode;
    public string MapUid => Map.MapUid;
    public Ident MapInfo => Map.MapInfo;

    public SessionMap(CGameCtnChallenge map, DateTimeOffset receivedAt, string tmxLink)
    {
        Map = map;
        ReceivedAt = receivedAt;
        TmxLink = tmxLink;
    }
    
    internal bool IsAuthorMedal(CGameCtnGhost ghost)
    {
        switch (Mode)
        {
            case CGameCtnChallenge.PlayMode.Race:
            case CGameCtnChallenge.PlayMode.Puzzle:
                return ghost.RaceTime <= ChallengeParameters.AuthorTime;
            case CGameCtnChallenge.PlayMode.Platform:

                if (ChallengeParameters.AuthorScore > 0 && ghost.Respawns <= ChallengeParameters.AuthorScore)
                {
                    return true;
                }

                if (ghost.Respawns == 0 && ghost.RaceTime <= ChallengeParameters.AuthorTime)
                {
                    return true;
                }

                return false;
            case CGameCtnChallenge.PlayMode.Stunts:
                return ghost.StuntScore >= ChallengeParameters.AuthorScore;
            default:
                throw new NotSupportedException($"Unsupported gamemode {Mode}.");
        }
    }

    internal bool IsGoldMedal(CGameCtnGhost ghost)
    {
        return Mode switch
        {
            CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle => ghost.RaceTime <= ChallengeParameters.GoldTime,
            CGameCtnChallenge.PlayMode.Platform => ghost.Respawns <= ChallengeParameters.GoldTime.GetValueOrDefault().TotalMilliseconds,
            CGameCtnChallenge.PlayMode.Stunts => ghost.StuntScore >= ChallengeParameters.GoldTime.GetValueOrDefault().TotalMilliseconds,
            _ => throw new NotSupportedException($"Unsupported gamemode {Mode}."),
        };
    }

    internal bool IsSilverMedal(CGameCtnGhost ghost)
    {
        return Mode switch
        {
            CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle => ghost.RaceTime <= ChallengeParameters.SilverTime,
            CGameCtnChallenge.PlayMode.Platform => ghost.Respawns <= ChallengeParameters.SilverTime.GetValueOrDefault().TotalMilliseconds,
            CGameCtnChallenge.PlayMode.Stunts => ghost.StuntScore >= ChallengeParameters.SilverTime.GetValueOrDefault().TotalMilliseconds,
            _ => throw new NotSupportedException($"Unsupported gamemode {Mode}."),
        };
    }

    internal bool IsBronzeMedal(CGameCtnGhost ghost)
    {
        return Mode switch
        {
            CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle => ghost.RaceTime <= ChallengeParameters.BronzeTime,
            CGameCtnChallenge.PlayMode.Platform => ghost.Respawns <= ChallengeParameters.BronzeTime.GetValueOrDefault().TotalMilliseconds,
            CGameCtnChallenge.PlayMode.Stunts => ghost.StuntScore >= ChallengeParameters.BronzeTime.GetValueOrDefault().TotalMilliseconds,
            _ => throw new NotSupportedException($"Unsupported gamemode {Mode}."),
        };
    }

    internal bool IsFinished(CGameCtnGhost ghost)
    {
        return Mode switch
        {
            CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle => ghost.RaceTime > ChallengeParameters.BronzeTime,
            CGameCtnChallenge.PlayMode.Platform => ghost.Respawns > ChallengeParameters.BronzeTime.GetValueOrDefault().TotalMilliseconds,
            CGameCtnChallenge.PlayMode.Stunts => ghost.StuntScore > ChallengeParameters.BronzeTime.GetValueOrDefault().TotalMilliseconds,
            _ => throw new NotSupportedException($"Unsupported gamemode {Mode}."),
        };
    }
}