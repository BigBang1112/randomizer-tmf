using GBX.NET.Engines.Game;

namespace RandomizerTMF.Logic;

public class RandomizerRules
{
    public TimeSpan TimeLimit { get; init; } = TimeSpan.FromHours(1);
    public RequestRules RequestRules { get; init; }
    public Func<CGameCtnChallenge, bool> TrackCompletionCondition { get; init; }
    public Func<RandomizerRules, bool> ChallengeCompletionCondition { get; init; }
}
