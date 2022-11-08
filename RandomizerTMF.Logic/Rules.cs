namespace RandomizerTMF.Logic;

public class Rules
{
    public TimeSpan TimeLimit { get; init; }
    public RandomizationRules Randomization { get; init; }
    public Func<bool> TrackCompletionCondition { get; init; }
    public Func<bool> ChallengeCompletionCondition { get; init; }
}
