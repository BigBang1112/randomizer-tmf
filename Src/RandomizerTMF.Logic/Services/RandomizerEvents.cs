﻿using GBX.NET.Engines.Game;
using Microsoft.Extensions.Logging;
using TmEssentials;

namespace RandomizerTMF.Logic.Services;

public interface IRandomizerEvents
{
    event Action? MapEnded;
    event Action? MapSkip;
    event Action<SessionMap>? MapStarted;
    event Action? MedalUpdate;
    event Action<string>? Status;
    event Action<string, CGameCtnReplayRecord> AutosaveCreatedOrChanged;
    event Action? FirstMapStarted;

    void OnMapEnded();
    void OnMapSkip();
    void OnMapStarted(SessionMap map);
    void OnMedalUpdate();
    void OnStatus(string status);
    void OnAutosaveCreatedOrChanged(string fileName, CGameCtnReplayRecord replay);
    void OnFirstMapStarted();
    void OnTimeResume(TimeSpan pausedTime);
}

public class RandomizerEvents : IRandomizerEvents
{
    private readonly IRandomizerConfig config;
    private readonly ILogger logger;
    private readonly IDiscordRichPresence discord;

    public event Action<string>? Status;
    public event Action<SessionMap>? MapStarted;
    public event Action? MapEnded;
    public event Action? MapSkip;
    public event Action? MedalUpdate;
    public event Action<string, CGameCtnReplayRecord>? AutosaveCreatedOrChanged;
    public event Action? FirstMapStarted;

    public RandomizerEvents(IRandomizerConfig config, ILogger logger, IDiscordRichPresence discord)
    {
        this.config = config;
        this.logger = logger;
        this.discord = discord;
    }

    public void OnStatus(string status)
    {
        // Status kind of logging
        // Unlike regular logs, these are shown to the user in a module, while also written to the log file in its own way
        logger.LogInformation("STATUS: {status}", status);

        Status?.Invoke(status);
    }

    public void OnFirstMapStarted()
    {
        FirstMapStarted?.Invoke();

        var now = DateTime.UtcNow;
        
        discord.SessionStart(now);
        discord.SessionPredictEnd(now + config.Rules.TimeLimit);
    }

    public void OnMapStarted(SessionMap map)
    {
        MapStarted?.Invoke(map);

        discord.SessionDetails("Playing " + TextFormatter.Deformat(map.Map.MapName));
    }

    public void OnMapEnded()
    {
        MapEnded?.Invoke();

        discord.SessionDefaultAsset();
        discord.SessionDetails("Map ended");
    }

    public void OnMapSkip() => MapSkip?.Invoke();
    public void OnMedalUpdate() => MedalUpdate?.Invoke();

    public void OnAutosaveCreatedOrChanged(string fileName, CGameCtnReplayRecord replay)
    {
        AutosaveCreatedOrChanged?.Invoke(fileName, replay);
    }

    public void OnTimeResume(TimeSpan pausedTime)
    {
        discord.AddToSessionPredictEnd(pausedTime);
    }
}
