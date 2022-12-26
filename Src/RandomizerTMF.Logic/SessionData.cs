using GBX.NET.Engines.Game;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic.Services;
using TmEssentials;
using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public class SessionData
{
    private readonly IRandomizerConfig config;
    private readonly ILogger? logger;

    public string? Version { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public RandomizerRules Rules { get; set; }

    [YamlIgnore]
    public string StartedAtText => StartedAt.ToString("yyyy-MM-dd HH_mm_ss");
    
    [YamlIgnore]
    public string DirectoryPath { get; }

    public List<SessionDataMap> Maps { get; set; } = new();

    public SessionData() : this(null, DateTimeOffset.Now, new RandomizerConfig(), null)
    {
        
    }

    private SessionData(string? version, DateTimeOffset startedAt, IRandomizerConfig config, ILogger? logger)
    {
        Version = version;
        StartedAt = startedAt;
        
        this.config = config;
        this.logger = logger;

        Rules = config.Rules;

        DirectoryPath = Path.Combine(FilePathManager.SessionsDirectoryPath, StartedAtText);
    }

    public static SessionData Initialize(IRandomizerConfig config, ILogger logger)
    {
        var startedAt = DateTimeOffset.Now;

        var data = new SessionData(RandomizerEngine.Version, startedAt, config, logger);

        Directory.CreateDirectory(data.DirectoryPath);

        data.Save();

        return data;
    }

    public void SetMapResult(SessionMap map, string result)
    {
        var dataMap = Maps.FirstOrDefault(x => x.Uid == map.MapUid);

        if (dataMap is not null)
        {
            dataMap.Result = result;
            dataMap.LastTimestamp = map.LastTimestamp;
        }

        Save();
    }

    public void Save()
    {
        logger?.LogInformation("Saving the session data into file...");

        File.WriteAllText(Path.Combine(DirectoryPath, Constants.SessionYml), Yaml.Serializer.Serialize(this));

        logger?.LogInformation("Session data saved.");
    }

    public void SetReadOnlySessionYml()
    {
        try
        {
            var sessionYmlFile = Path.Combine(DirectoryPath, Constants.SessionYml);
            File.SetAttributes(sessionYmlFile, File.GetAttributes(sessionYmlFile) | FileAttributes.ReadOnly);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to set Session.yml as read-only.");
        }
    }

    public void UpdateFromAutosave(string fullPath, SessionMap map, CGameCtnReplayRecord replay, TimeSpan elapsed)
    {
        var score = map.Map.Mode switch
        {
            CGameCtnChallenge.PlayMode.Stunts => replay.GetGhosts().First().StuntScore + "_",
            CGameCtnChallenge.PlayMode.Platform => replay.GetGhosts().First().Respawns + "_",
            _ => ""
        } + replay.Time.ToTmString(useHundredths: true, useApostrophe: true);

        var mapName = CompiledRegex.SpecialCharRegex().Replace(TextFormatter.Deformat(map.Map.MapName).Trim(), "_");

        var replayFileFormat = string.IsNullOrWhiteSpace(config.ReplayFileFormat)
            ? Constants.DefaultReplayFileFormat
            : config.ReplayFileFormat;

        var replayFileName = FilePathManager.ClearFileName(string.Format(replayFileFormat, mapName, score, replay.PlayerLogin));

        var replaysDir = Path.Combine(DirectoryPath, Constants.Replays);
        var replayFilePath = Path.Combine(replaysDir, replayFileName);

        Directory.CreateDirectory(replaysDir);
        File.Copy(fullPath, replayFilePath, overwrite: true);

        Maps.FirstOrDefault(x => x.Uid == map.MapUid)?
            .Replays
            .Add(new()
            {
                FileName = replayFileName,
                Timestamp = elapsed
            });

        Save();
    }
}
