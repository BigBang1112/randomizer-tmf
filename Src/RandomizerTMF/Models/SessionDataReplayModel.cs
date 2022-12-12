using GBX.NET;
using GBX.NET.Engines.Game;
using RandomizerTMF.Logic;
using TmEssentials;

namespace RandomizerTMF.Models;

public class SessionDataReplayModel
{
    public SessionDataReplay Replay { get; }
    public CGameCtnChallenge Map { get; }
    public TimeInt32? Time { get; }
    public string TimeText { get; }
    public int? Respawns { get; }
    public int? StuntScore { get; }

    public bool HasBronzeMedal => (Map.Mode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle && Time <= Map.ChallengeParameters?.BronzeTime)
        || (Map.Mode is CGameCtnChallenge.PlayMode.Platform && Respawns <= Map.ChallengeParameters?.BronzeTime?.TotalMilliseconds)
        || (Map.Mode is CGameCtnChallenge.PlayMode.Stunts && StuntScore >= Map.ChallengeParameters?.BronzeTime?.TotalMilliseconds);

    public bool HasSilverMedal => (Map.Mode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle && Time <= Map.ChallengeParameters?.SilverTime)
        || (Map.Mode is CGameCtnChallenge.PlayMode.Platform && Respawns <= Map.ChallengeParameters?.SilverTime?.TotalMilliseconds)
        || (Map.Mode is CGameCtnChallenge.PlayMode.Stunts && StuntScore >= Map.ChallengeParameters?.SilverTime?.TotalMilliseconds);

    public bool HasGoldMedal => (Map.Mode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle && Time <= Map.ChallengeParameters?.GoldTime)
        || (Map.Mode is CGameCtnChallenge.PlayMode.Platform && Respawns <= Map.ChallengeParameters?.GoldTime?.TotalMilliseconds)
        || (Map.Mode is CGameCtnChallenge.PlayMode.Stunts && StuntScore >= Map.ChallengeParameters?.GoldTime?.TotalMilliseconds);

    public bool HasAuthorMedal => (Map.Mode is CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle && Time <= Map.ChallengeParameters?.AuthorTime)
        || (Map.Mode is CGameCtnChallenge.PlayMode.Platform && ((Map.ChallengeParameters?.AuthorScore > 0 && Respawns <= Map.ChallengeParameters?.AuthorScore) || (Respawns == 0 && Time <= Map.ChallengeParameters?.AuthorTime)))
        || (Map.Mode is CGameCtnChallenge.PlayMode.Stunts && StuntScore >= Map.ChallengeParameters?.AuthorScore);

    public SessionDataReplayModel(SessionDataReplay replay, string sessionStr, bool first, CGameCtnChallenge? map)
    {
        Replay = replay;

        var path = Path.Combine(Constants.Sessions, sessionStr, Constants.Replays, replay.FileName);
        var node = GameBox.ParseNode<CGameCtnReplayRecord>(path);

        if (first)
        {
            map = node.Challenge;
        }

        Map = map ?? throw new Exception("Map is null");
        Time = node.Time;
        TimeText = Time.ToTmString(useHundredths: true);

        if (node.GetGhosts().FirstOrDefault() is CGameCtnGhost ghost)
        {
            Respawns = ghost.Respawns;
            StuntScore = ghost.StuntScore;
        }

        TimeText = map.Mode switch
        {
            CGameCtnChallenge.PlayMode.Race or CGameCtnChallenge.PlayMode.Puzzle => Time.ToTmString(useHundredths: true),
            CGameCtnChallenge.PlayMode.Platform => $"{Respawns} ({Time.ToTmString(useHundredths: true)})",
            CGameCtnChallenge.PlayMode.Stunts => StuntScore.ToString() ?? "",
            _ => "",
        };
    }
}
