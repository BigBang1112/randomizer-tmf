using Microsoft.Extensions.Logging;

namespace RandomizerTMF.Logic;

internal class DiscordRpcLogger : DiscordRPC.Logging.ILogger
{
    private readonly ILogger logger;

    public DiscordRpcLogger(ILogger logger)
	{
        this.logger = logger;
    }

    public DiscordRPC.Logging.LogLevel Level { get; set; } = DiscordRPC.Logging.LogLevel.Info;

    public void Error(string message, params object[] args)
    {
        using var _ = CreateScope();
        logger.LogError(message, args);
    }

    public void Info(string message, params object[] args)
    {
        using var _ = CreateScope();
        logger.LogInformation(message, args);
    }

    public void Trace(string message, params object[] args)
    {
        // using var _ = CreateScope();
        // logger.LogTrace(message, args);
    }

    public void Warning(string message, params object[] args)
    {
        using var _ = CreateScope();
        logger.LogWarning(message, args);
    }

    private IDisposable? CreateScope()
    {
        return logger.BeginScope("Discord RPC");
    }
}
