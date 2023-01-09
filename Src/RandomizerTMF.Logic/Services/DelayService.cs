namespace RandomizerTMF.Logic.Services;

public interface IDelayService
{
    Task Delay(int ms, CancellationToken cancellationToken);
}

public class DelayService : IDelayService
{
    public async Task Delay(int ms, CancellationToken cancellationToken)
    {
        await Task.Delay(ms, cancellationToken);
    }
}
