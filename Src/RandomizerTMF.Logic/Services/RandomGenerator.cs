namespace RandomizerTMF.Logic.Services;

public interface IRandomGenerator
{
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
}

public class RandomGenerator : IRandomGenerator
{
    public int Next(int maxValue)
    {
        return Random.Shared.Next(maxValue); // Not testable
    }

    public int Next(int minValue, int maxValue)
    {
        return Random.Shared.Next(minValue, maxValue); // Not testable
    }
}
