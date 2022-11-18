using GBX.NET.Engines.Game;

namespace RandomizerTMF.Logic;

public class Rules
{
    public TimeSpan TimeLimit { get; init; } = TimeSpan.FromHours(1);
    public RandomizationRules Randomization { get; init; }
    public Func<CGameCtnChallenge, bool> TrackCompletionCondition { get; init; }
    public Func<Rules, bool> ChallengeCompletionCondition { get; init; }
}
