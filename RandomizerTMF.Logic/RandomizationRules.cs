namespace RandomizerTMF.Logic;

public class RandomizationRules
{
    public required ESite Site { get; init; }

    public string? Author { get; init; }
    public EEnvironment[]? Environment { get; init; }
    public string? Name { get; init; }
    public EEnvironment[]? Vehicle { get; init; }
    public EPrimaryType? PrimaryType { get; init; }
    public ETag? Tag { get; init; }
    public EMood[]? Mood { get; init; }
    public EDifficulty[]? Difficulty { get; init; }
    public ERoutes[]? Routes { get; init; }
    public ELbType? LbType { get; init; }
    public bool InBeta { get; init; }
    public bool InPlayLater { get; init; }
    public bool InFeatured { get; init; }
    public bool InSupporter { get; init; }
    public bool InFavorite { get; init; }
    public bool InDownloads { get; init; }
    public bool InReplays { get; init; }
    public bool InEnvmix { get; init; }
    public bool InHasRecord { get; init; }
    public bool InLatestAuthor { get; init; }
    public bool InLatestAwardedAuthor { get; init; }
    public bool InScreenshot { get; init; }
    public DateTimeOffset UploadedBefore { get; init; }
    public DateTimeOffset UploadedAfter { get; init; }
}