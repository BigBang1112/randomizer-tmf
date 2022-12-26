using GBX.NET.Engines.Game;
using Microsoft.Extensions.Logging;

namespace RandomizerTMF.Logic;

public interface IRandomizerEvents
{
    event Action? MapEnded;
    event Action? MapSkip;
    event Action? MapStarted;
    event Action? MedalUpdate;
    event Action<string>? Status;
    event Action<string, CGameCtnReplayRecord> SessionAutosaveCreatedOrChanged;

    void OnMapEnded();
    void OnMapSkip();
    void OnMapStarted();
    void OnMedalUpdate();
    void OnStatus(string status);
    void OnSessionAutosaveCreatedOrChanged(string fileName, CGameCtnReplayRecord replay);
}

public class RandomizerEvents : IRandomizerEvents
{
    private readonly ILogger logger;

    public event Action<string>? Status;
    public event Action? MapStarted;
    public event Action? MapEnded;
    public event Action? MapSkip;
    public event Action? MedalUpdate;
    public event Action<string, CGameCtnReplayRecord>? SessionAutosaveCreatedOrChanged;

    public RandomizerEvents(ILogger logger)
    {
        this.logger = logger;
    }

    public void OnStatus(string status)
    {
        // Status kind of logging
        // Unlike regular logs, these are shown to the user in a module, while also written to the log file in its own way
        logger.LogInformation("STATUS: {status}", status);

        Status?.Invoke(status);
    }

    public void OnMapStarted() => MapStarted?.Invoke();
    public void OnMapEnded() => MapEnded?.Invoke();
    public void OnMapSkip() => MapSkip?.Invoke();
    public void OnMedalUpdate() => MedalUpdate?.Invoke();
    
    public void OnSessionAutosaveCreatedOrChanged(string fileName, CGameCtnReplayRecord replay)
    {
        SessionAutosaveCreatedOrChanged?.Invoke(fileName, replay);
    }
}
