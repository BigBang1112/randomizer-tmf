using GBX.NET.Engines.Game;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic.Services;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Security.Cryptography;
using TmEssentials;
using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public class SessionData
{
    private readonly IRandomizerConfig config;
    private readonly ILogger? logger;
    private readonly IFileSystem? fileSystem;

    public string? Version { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public RandomizerRules Rules { get; set; }

    [YamlIgnore]
    public string StartedAtText => StartedAt.ToString("yyyy-MM-dd HH_mm_ss");
    
    [YamlIgnore]
    public string DirectoryPath { get; private set; }

    public List<SessionDataMap> Maps { get; set; } = [];

    public SessionData() : this(version: null, // will be overwriten by deserialization
                                DateTimeOffset.Now, // will be overwriten by deserialization
                                new RandomizerConfig(), // unused in read-only state
                                logger: null, // unused in read-only state
                                fileSystem: null) // unused in read-only state
    {
        // This is legit only for read-only use cases and for YAML deserialization!
    }

    private SessionData(string? version,
                        DateTimeOffset startedAt,
                        IRandomizerConfig config,
                        ILogger? logger,
                        IFileSystem? fileSystem)
    {
        Version = version;
        StartedAt = startedAt;
        
        this.config = config;
        this.logger = logger;
        this.fileSystem = fileSystem;
        
        Rules = config.Rules;

        DirectoryPath = Path.Combine(FilePathManager.SessionsDirectoryPath, StartedAtText);
    }

    internal static SessionData Initialize(DateTimeOffset startedAt, IRandomizerConfig config, ILogger logger, IFileSystem fileSystem)
    {
        var data = new SessionData(RandomizerEngine.Version, startedAt, config, logger, fileSystem);

        fileSystem.Directory.CreateDirectory(data.DirectoryPath);

        data.Save();

        return data;
    }

    public static SessionData Initialize(IRandomizerConfig config, ILogger logger, IFileSystem fileSystem)
    {
        return Initialize(DateTimeOffset.Now, config, logger, fileSystem);
    }

    public void SetMapResult(SessionMap map, string result)
    {
        var dataMap = Maps.First(x => x.Uid == map.MapUid);

        dataMap.Result = result;
        dataMap.LastTimestamp = map.LastTimestamp;

        Save();
    }

    public void Save()
    {
        logger?.LogInformation("Saving the session data into file...");

        if (fileSystem is not null)
        {
            //fileSystem.File.WriteAllText(Path.Combine(DirectoryPath, Constants.SessionYml), Yaml.Serializer.Serialize(this));

            using var fs = fileSystem.File.Create(Path.Combine(DirectoryPath, Constants.SessionBin));
            using var writer = new BinaryWriter(fs);
            Serialize(writer);
        }

        logger?.LogInformation("Session data saved.");
    }

    internal void InternalSetReadOnlySessionBin()
    {
        var sessionBinFile = Path.Combine(DirectoryPath, Constants.SessionBin);
        fileSystem?.File.SetAttributes(sessionBinFile, fileSystem.File.GetAttributes(sessionBinFile) | FileAttributes.ReadOnly);
    }

    public void SetReadOnlySessionBin()
    {
        try
        {
            InternalSetReadOnlySessionBin();
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
            CGameCtnChallenge.PlayMode.Stunts => replay.GetGhosts(alsoInClips: false).First().StuntScore + "_",
            CGameCtnChallenge.PlayMode.Platform => replay.GetGhosts(alsoInClips: false).First().Respawns + "_",
            _ => ""
        } + replay.Time.ToTmString(useHundredths: true, useApostrophe: true);

        var mapName = CompiledRegex.SpecialCharRegex().Replace(TextFormatter.Deformat(map.Map.MapName).Trim(), "_");

        var replayFileFormat = string.IsNullOrWhiteSpace(config.ReplayFileFormat)
            ? Constants.DefaultReplayFileFormat
            : config.ReplayFileFormat;

        var replayFileName = FilePathManager.ClearFileName(string.Format(replayFileFormat, mapName, score, replay.PlayerLogin));

        var replaysDir = Path.Combine(DirectoryPath, Constants.Replays);
        var replayFilePath = Path.Combine(replaysDir, replayFileName);

        if (fileSystem is not null)
        {
            fileSystem.Directory.CreateDirectory(replaysDir);
            fileSystem.File.Copy(fullPath, replayFilePath, overwrite: true);
        }
        
        Maps.First(x => x.Uid == map.MapUid)?
            .Replays
            .Add(new()
            {
                FileName = replayFileName,
                Timestamp = elapsed
            });

        Save();
    }

    public double GetScore()
    {
        var hasFinishedMaps = Maps.Count > 0 && Maps.Any(x => x.LastTimestamp.HasValue);

        if (!hasFinishedMaps)
        {
            return 0;
        }

        var score = 0;
        
        foreach (var map in Maps)
        {
            if (map.Result == Constants.AuthorMedal)
            {
                score += 5;
                continue;
            }

            if (map.Result == Constants.GoldMedal)
            {
                score += 1;
                continue;
            }

            if (map.Result == Constants.Skipped)
            {
                score -= 3;
                continue;
            }
        }

        var lengthInMinutes = Maps.Last(x => x.LastTimestamp.HasValue)
            .LastTimestamp
            .GetValueOrDefault()
            .TotalMinutes;

        return score / lengthInMinutes * ((int)lengthInMinutes);
    }

    public void Serialize(BinaryWriter writer)
    {
        const int version = 2;

        writer.Write("RandTMF");
        writer.Write((byte)version); // version
        
        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();

        writer.Write(aes.Key);
        writer.Write(aes.IV);

        using var enc = aes.CreateEncryptor();

        using var crypto = new CryptoStream(writer.BaseStream, enc, CryptoStreamMode.Write);
        using var deflate = new DeflateStream(crypto, CompressionLevel.Fastest);
        using var w = new BinaryWriter(deflate);

        w.Write(Version ?? string.Empty);
        w.Write(StartedAt.Ticks);
        w.Write((short)StartedAt.TotalOffsetMinutes);

        Rules.Serialize(w, version);

        w.Write(Maps.Count);
        foreach (var map in Maps)
        {
            map.Serialize(w);
        }
    }

    public void Deserialize(BinaryReader reader)
    {
        var magic = reader.ReadBytes(8);
        if (!magic.SequenceEqual(new byte[] { 7, (byte)'R', (byte)'a', (byte)'n', (byte)'d', (byte)'T', (byte)'M', (byte)'F' }))
        {
            throw new InvalidDataException("Invalid magic number.");
        }

        var version = reader.ReadByte();
        if (version > 1)
        {
            throw new InvalidDataException("Invalid version.");
        }

        using var aes = Aes.Create();
        aes.Key = reader.ReadBytes(aes.Key.Length);
        aes.IV = reader.ReadBytes(aes.IV.Length);

        using var dec = aes.CreateDecryptor();

        using var crypto = new CryptoStream(reader.BaseStream, dec, CryptoStreamMode.Read);
        using var inflate = new DeflateStream(crypto, CompressionMode.Decompress);
        using var r = new BinaryReader(inflate);

        var versionStr = r.ReadString();
        Version = string.IsNullOrEmpty(versionStr) ? null : versionStr;
        
        var startedAtTicks = r.ReadInt64();
        var startedAtOffset = r.ReadInt16();
        StartedAt = new DateTimeOffset(startedAtTicks, TimeSpan.FromMinutes(startedAtOffset));
        DirectoryPath = Path.Combine(FilePathManager.SessionsDirectoryPath, StartedAtText);

        Rules = new();
        Rules.Deserialize(r, version);

        Maps.Clear();
        var mapsCount = r.ReadInt32();
        for (var i = 0; i < mapsCount; i++)
        {
            Maps.Add(SessionDataMap.Deserialize(r));
        }
    }
}
