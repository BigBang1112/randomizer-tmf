using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace RandomizerTMF.Logic.Services;

public interface IRandomizerEngine
{
    ISession? CurrentSession { get; }
    bool HasSessionRunning { get; }
    bool SessionEnding { get; }

    Task EndSessionAsync();
    void Exit();
    void StartSession();
}

public class RandomizerEngine : IRandomizerEngine
{
    private readonly IRandomizerConfig config;
    private readonly IRandomizerEvents events;
    private readonly IDiscordRichPresence discord;
    private readonly ILogger logger;
    private readonly Func<ISession> session;

    public static string? Version { get; } = typeof(RandomizerEngine).Assembly.GetName().Version?.ToString(3);

    public ISession? CurrentSession { get; private set; }
    public bool HasSessionRunning => CurrentSession is not null;
    public bool SessionEnding { get; private set; }

    public RandomizerEngine(IRandomizerConfig config,
                            IRandomizerEvents events,
                            IDiscordRichPresence discord,
                            IFileSystem fileSystem,
                            ILogger logger,
                            Func<ISession> session)
    {
        this.config = config;
        this.events = events;
        this.discord = discord;
        this.logger = logger;
        this.session = session;

        logger.LogInformation("Starting Randomizer Engine...");

        fileSystem.Directory.CreateDirectory(FilePathManager.SessionsDirectoryPath);

        logger.LogInformation("Predefining LZO algorithm...");

        GBX.NET.Lzo.SetLzo(typeof(GBX.NET.LZO.MiniLZO));

        logger.LogInformation("Randomizer TMF initialized.");

        events.MedalUpdate += ScoreChanged;
        events.MapSkip += ScoreChanged;
    }

    private void ScoreChanged()
    {
        if (CurrentSession is null)
        {
            discord.SessionState();
        }
        else
        {
            discord.SessionState(CurrentSession.AuthorMaps.Count, CurrentSession.GoldMaps.Count, CurrentSession.SkippedMaps.Count);
        }
    }

    /// <summary>
    /// Starts the randomizer session by creating a new <see cref="Session"/> that will handle randomization on different thread from the UI thread.
    /// </summary>
    public void StartSession()
    {
        if (config.GameDirectory is null)
        {
            return;
        }

        CurrentSession = session();
        CurrentSession.Start();
    }

    /// <summary>
    /// Does the cleanup of the session so that the new one can be instantiated without issues.
    /// </summary>
    private void ClearCurrentSession()
    {
        CurrentSession?.Stop();
        CurrentSession = null;

        if (logger is LoggerToFile loggerToFile)
        {
            loggerToFile.RemoveSessionWriter();
        }
    }

    /// <summary>
    /// MANUAL end of session.
    /// </summary>
    /// <returns></returns>
    public async Task EndSessionAsync()
    {
        if (SessionEnding || CurrentSession is null)
        {
            return;
        }

        SessionEnding = true;

        events.OnStatus("Ending the session...");

        CurrentSession.TokenSource.Cancel();

        try
        {
            if (CurrentSession.Task is not null)
            {
                await CurrentSession.Task; // Kindly waits until the session considers it was cancelled. ClearCurrentSession is called within it.
            }
        }
        catch (TaskCanceledException)
        {

        }

        ClearCurrentSession();

        SessionEnding = false;
    }

    public void Exit()
    {
        logger.LogInformation("Exiting...");
        Environment.Exit(0);
    }
}
