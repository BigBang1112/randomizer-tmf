using RandomizerTMF.Logic.Services;

namespace RandomizerTMF.Logic.Tests.Unit.Services;

public class DiscordRichPresenceTests
{
    [Fact]
    public void BuildState_Singular()
    {
        var actual = DiscordRichPresence.BuildState(1, 1, 1);

        Assert.Equal(expected: $"1 AT, 1 gold, 1 skip", actual);
    }

    [Theory]
    [InlineData(2, 3, 4)]
    [InlineData(42, 69, 420)]
    [InlineData(5, -5, -8)]
    public void BuildState_Plural(int atCount, int goldCount, int skipCount)
    {
        var actual = DiscordRichPresence.BuildState(atCount, goldCount, skipCount);

        Assert.Equal(expected: $"{atCount} ATs, {goldCount} golds, {skipCount} skips", actual);
    }
}
