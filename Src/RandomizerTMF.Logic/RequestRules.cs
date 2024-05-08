using RandomizerTMF.Logic.Services;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using TmEssentials;

namespace RandomizerTMF.Logic;

public class RequestRules
{
    private static readonly ESite[] siteValues = Enum.GetValues<ESite>();
    private static readonly EEnvironment[] envValues = Enum.GetValues<EEnvironment>();
    private static readonly EEnvironment[] sunriseEnvValues = new [] { EEnvironment.Island, EEnvironment.Bay, EEnvironment.Coast };
    private static readonly EEnvironment[] originalEnvValues = new [] { EEnvironment.Desert, EEnvironment.Snow, EEnvironment.Rally };

    // Custom rules that are not part of the official API

    public required ESite Site { get; set; }
    public bool EqualEnvironmentDistribution { get; set; }
    public bool EqualVehicleDistribution { get; set; }

    public string? Author { get; set; }
    public HashSet<EEnvironment>? Environment { get; set; }
    public string? Name { get; set; }
    public HashSet<EEnvironment>? Vehicle { get; set; }
    public EPrimaryType? PrimaryType { get; set; }
    public ETag? Tag { get; set; }
    public HashSet<EMood>? Mood { get; set; }
    public HashSet<EDifficulty>? Difficulty { get; set; }
    public HashSet<ERoutes>? Routes { get; set; }
    public ELbType? LbType { get; set; }
    public bool? InBeta { get; set; }
    public bool? InPlayLater { get; set; }
    public bool? InFeatured { get; set; }
    public bool? InSupporter { get; set; }
    public bool? InFavorite { get; set; }
    public bool? InDownloads { get; set; }
    public bool? InReplays { get; set; }
    public bool? InEnvmix { get; set; }
    public bool? InHasRecord { get; set; }
    public bool? InLatestAuthor { get; set; }
    public bool? InLatestAwardedAuthor { get; set; }
    public bool? InScreenshot { get; set; }
    public DateOnly? UploadedBefore { get; set; }
    public DateOnly? UploadedAfter { get; set; }
    public TimeInt32? AuthorTimeMin { get; set; }
    public TimeInt32? AuthorTimeMax { get; set; }

    public string ToUrl(IRandomGenerator random) // Not very efficient but does the job done fast enough
    {
        var b = new StringBuilder("https://");

        var matchingSites = siteValues
            .Where(x => x != ESite.Any && (Site & x) == x)
            .ToArray();

        // If Site is Any, then it picks from sites that are valid within environments and cars
        var site = GetRandomSite(random, matchingSites.Length == 0
            ? siteValues.Where(x => x is not ESite.Any
            && IsSiteValidWithEnvironments(x)
            && IsSiteValidWithVehicles(x)
            && IsSiteValidWithEnvimix(x)).ToArray()
            : matchingSites);

        var siteUrl = GetSiteUrl(site);

        b.Append(siteUrl);
        
        b.Append("/trackrandom");

        var first = true;

        foreach (var prop in GetType().GetProperties().Where(IsQueryProperty))
        {
            var val = prop.GetValue(this);

            if (EqualEnvironmentDistribution && prop.Name == nameof(Environment))
            {
                val = GetRandomEnvironmentThroughSet(random, Environment, site);
            }

            if (EqualVehicleDistribution && prop.Name == nameof(Vehicle))
            {
                val = GetRandomEnvironmentThroughSet(random, Vehicle, site);
            }

            if (val is null || (val is IEnumerable enumerable && !enumerable.Cast<object>().Any()))
            {
                continue;
            }

            // Adjust url on weird combinations
            if (site is ESite.TMNF or ESite.Nations && !IsValidInNations(prop, val))
            {
                continue;
            }

            if (first)
            {
                b.Append('?');
                first = false;
            }
            else
            {
                b.Append('&');
            }

            b.Append(prop.Name.ToLower());
            b.Append('=');

            var genericType = prop.PropertyType.IsGenericType ? prop.PropertyType.GetGenericTypeDefinition() : null;

            if (genericType == typeof(Nullable<>))
            {
                AppendValue(b, prop.PropertyType.GetGenericArguments()[0], val, genericType);
            }
            else
            {
                AppendValue(b, prop.PropertyType, val, genericType);
            }
        }

        return b.ToString();
    }

    private bool IsQueryProperty(PropertyInfo prop)
    {
        return prop.Name is not nameof(Site)
                        and not nameof(EqualEnvironmentDistribution)
                        and not nameof(EqualVehicleDistribution);
    }

    private static bool IsSiteValidWithEnvironments(ESite site, HashSet<EEnvironment>? envs)
    {
        if (envs is null)
        {
            return true;
        }
        
        return site switch
        {
            ESite.Sunrise => envs.Contains(EEnvironment.Island) || envs.Contains(EEnvironment.Coast) || envs.Contains(EEnvironment.Bay),
            ESite.Original => envs.Contains(EEnvironment.Snow) || envs.Contains(EEnvironment.Desert) || envs.Contains(EEnvironment.Rally),
            ESite.TMNF or ESite.Nations => envs.Contains(EEnvironment.Stadium),
            _ => true,
        };
    }

    private bool IsSiteValidWithEnvironments(ESite site)
    {
        return IsSiteValidWithEnvironments(site, Environment);
    }

    private bool IsSiteValidWithVehicles(ESite site)
    {
        return IsSiteValidWithEnvironments(site, Vehicle);
    }

    private bool IsSiteValidWithEnvimix(ESite site)
    {
        if (site is not ESite.Sunrise and not ESite.Original || Environment is null || Environment.Count == 0)
        {
            return true;
        }

        foreach (var env in Environment)
        {
            if (Vehicle?.Contains(env) == false)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsValidInNations(PropertyInfo prop, object val)
    {
        if (prop.Name is nameof(Environment) or nameof(Vehicle) && !val.Equals(EEnvironment.Stadium))
        {
            return false;
        }

        if (prop.Name is nameof(PrimaryType) && !val.Equals(EPrimaryType.Race))
        {
            return false;
        }

        return true;
    }

    private static EEnvironment GetRandomEnvironment(IRandomGenerator random, HashSet<EEnvironment>? container, ESite site)
    {
        if (container is not null && container.Count != 0)
        {
            return container.ElementAt(random.Next(container.Count));
        }
        
        return site switch
        {
            ESite.Sunrise => sunriseEnvValues[random.Next(sunriseEnvValues.Length)],
            ESite.Original => originalEnvValues[random.Next(originalEnvValues.Length)],
            _ => (EEnvironment)random.Next(envValues.Length) // Safe in case of EEnvironment
        };
    }

    private static HashSet<EEnvironment> GetRandomEnvironmentThroughSet(IRandomGenerator random, HashSet<EEnvironment>? container, ESite site)
    {
        return new HashSet<EEnvironment>() { GetRandomEnvironment(random, container, site) };
    }

    private static ESite GetRandomSite(IRandomGenerator random, ESite[] matchingSites)
    {
        return matchingSites[random.Next(matchingSites.Length)];
    }

    private static string GetSiteUrl(ESite site) => site switch
    {
        ESite.Any => throw new UnreachableException("Any is not a valid site"),
        ESite.TMNF => "tmnf.exchange",
        ESite.TMUF => "tmuf.exchange",
        _ => $"{site.ToString().ToLower()}.tm-exchange.com",
    };

    private static void AppendValue(StringBuilder b, Type type, object val, Type? genericType = null)
    {
        if (val is TimeInt32 timeInt32)
        {
            b.Append(timeInt32.TotalMilliseconds);
        }
        else if (val is bool boolVal)
        {
            b.Append(boolVal ? '1' : '0');
        }
        else if (val is DateOnly date)
        {
            b.Append(date.ToString("yyyy-MM-dd"));
        }
        else if (genericType == typeof(HashSet<>))
        {
            var elementType = type.GetGenericArguments()[0] ?? throw new UnreachableException("Array has null element type");

            var first = true;

            foreach (var elem in (IEnumerable)val)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    b.Append("%2C");
                }

                AppendValue(b, elementType, elem);
            }
            
        }
        else if (type.IsEnum)
        {
            b.Append((int)val);
        }
        else
        {
            b.Append(val);
        }
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write((int)Site);
        writer.Write(EqualEnvironmentDistribution);
        writer.Write(EqualVehicleDistribution);
        writer.Write(Author ?? string.Empty);
        writer.Write(Environment?.Count ?? 0);
        if (Environment is not null)
        {
            foreach (var env in Environment)
            {
                writer.Write((int)env);
            }
        }
        writer.Write(Name ?? string.Empty);
        writer.Write(Vehicle?.Count ?? 0);
        if (Vehicle is not null)
        {
            foreach (var veh in Vehicle)
            {
                writer.Write((int)veh);
            }
        }
        writer.Write(PrimaryType.HasValue ? (int)PrimaryType.Value : 255);
        writer.Write(Tag.HasValue ? (int)Tag.Value : 255);
        writer.Write(Mood?.Count ?? 0);
        if (Mood is not null)
        {
            foreach (var mood in Mood)
            {
                writer.Write((int)mood);
            }
        }
        writer.Write(Difficulty?.Count ?? 0);
        if (Difficulty is not null)
        {
            foreach (var diff in Difficulty)
            {
                writer.Write((int)diff);
            }
        }
        writer.Write(Routes?.Count ?? 0);
        if (Routes is not null)
        {
            foreach (var route in Routes)
            {
                writer.Write((int)route);
            }
        }
        writer.Write(LbType.HasValue ? (int)LbType.Value : 255);
        writer.Write(InBeta.HasValue ? Convert.ToByte(InBeta.Value) : 255);
        writer.Write(InPlayLater.HasValue ? Convert.ToByte(InPlayLater.Value) : 255);
        writer.Write(InFeatured.HasValue ? Convert.ToByte(InFeatured.Value) : 255);
        writer.Write(InSupporter.HasValue ? Convert.ToByte(InSupporter.Value) : 255);
        writer.Write(InFavorite.HasValue ? Convert.ToByte(InFavorite.Value) : 255);
        writer.Write(InDownloads.HasValue ? Convert.ToByte(InDownloads.Value) : 255);
        writer.Write(InReplays.HasValue ? Convert.ToByte(InReplays.Value) : 255);
        writer.Write(InEnvmix.HasValue ? Convert.ToByte(InEnvmix.Value) : 255);
        writer.Write(InHasRecord.HasValue ? Convert.ToByte(InHasRecord.Value) : 255);
        writer.Write(InLatestAuthor.HasValue ? Convert.ToByte(InLatestAuthor.Value) : 255);
        writer.Write(InLatestAwardedAuthor.HasValue ? Convert.ToByte(InLatestAwardedAuthor.Value) : 255);
        writer.Write(InScreenshot.HasValue ? Convert.ToByte(InScreenshot.Value) : 255);
        writer.Write(UploadedBefore?.ToString("yyyy-MM-dd") ?? string.Empty);
        writer.Write(UploadedAfter?.ToString("yyyy-MM-dd") ?? string.Empty);
        writer.Write(AuthorTimeMin?.TotalMilliseconds ?? 0);
        writer.Write(AuthorTimeMax?.TotalMilliseconds ?? 0);
    }

    public void Deserialize(BinaryReader r)
    {
        Site = (ESite)r.ReadInt32();
        EqualEnvironmentDistribution = r.ReadBoolean();
        EqualVehicleDistribution = r.ReadBoolean();
        Author = r.ReadString();
        var envCount = r.ReadInt32();
        if (envCount > 0)
        {
            Environment = new HashSet<EEnvironment>();
            for (var i = 0; i < envCount; i++)
            {
                Environment.Add((EEnvironment)r.ReadInt32());
            }
        }
        Name = r.ReadString();
        var vehCount = r.ReadInt32();
        if (vehCount > 0)
        {
            Vehicle = new HashSet<EEnvironment>();
            for (var i = 0; i < vehCount; i++)
            {
                Vehicle.Add((EEnvironment)r.ReadInt32());
            }
        }
        var primaryType = r.ReadInt32();
        PrimaryType = primaryType == 255 ? null : (EPrimaryType)primaryType;
        var tag = r.ReadInt32();
        Tag = tag == 255 ? null : (ETag)tag;
        var moodCount = r.ReadInt32();
        if (moodCount > 0)
        {
            Mood = new HashSet<EMood>();
            for (var i = 0; i < moodCount; i++)
            {
                Mood.Add((EMood)r.ReadInt32());
            }
        }
        var diffCount = r.ReadInt32();
        if (diffCount > 0)
        {
            Difficulty = new HashSet<EDifficulty>();
            for (var i = 0; i < diffCount; i++)
            {
                Difficulty.Add((EDifficulty)r.ReadInt32());
            }
        }
        var routeCount = r.ReadInt32();
        if (routeCount > 0)
        {
            Routes = new HashSet<ERoutes>();
            for (var i = 0; i < routeCount; i++)
            {
                Routes.Add((ERoutes)r.ReadInt32());
            }
        }
        var lbType = r.ReadInt32();
        LbType = lbType == 255 ? null : (ELbType)lbType;
        var inBeta = r.ReadByte();
        InBeta = inBeta == 255 ? null : Convert.ToBoolean(inBeta);
        var inPlayLater = r.ReadByte();
        InPlayLater = inPlayLater == 255 ? null : Convert.ToBoolean(inPlayLater);
        var inFeatured = r.ReadByte();
        InFeatured = inFeatured == 255 ? null : Convert.ToBoolean(inFeatured);
        var inSupporter = r.ReadByte();
        InSupporter = inSupporter == 255 ? null : Convert.ToBoolean(inSupporter);
        var inFavorite = r.ReadByte();
        InFavorite = inFavorite == 255 ? null : Convert.ToBoolean(inFavorite);
        var inDownloads = r.ReadByte();
        InDownloads = inDownloads == 255 ? null : Convert.ToBoolean(inDownloads);
        var inReplays = r.ReadByte();
        InReplays = inReplays == 255 ? null : Convert.ToBoolean(inReplays);
        var inEnvmix = r.ReadByte();
        InEnvmix = inEnvmix == 255 ? null : Convert.ToBoolean(inEnvmix);
        var inHasRecord = r.ReadByte();
        InHasRecord = inHasRecord == 255 ? null : Convert.ToBoolean(inHasRecord);
        var inLatestAuthor = r.ReadByte();
        InLatestAuthor = inLatestAuthor == 255 ? null : Convert.ToBoolean(inLatestAuthor);
        var inLatestAwardedAuthor = r.ReadByte();
        InLatestAwardedAuthor = inLatestAwardedAuthor == 255 ? null : Convert.ToBoolean(inLatestAwardedAuthor);
        var inScreenshot = r.ReadByte();
        InScreenshot = inScreenshot == 255 ? null : Convert.ToBoolean(inScreenshot);
        var uploadedBefore = r.ReadString();
        UploadedBefore = string.IsNullOrEmpty(uploadedBefore) ? null : DateOnly.Parse(uploadedBefore);
        var uploadedAfter = r.ReadString();
        UploadedAfter = string.IsNullOrEmpty(uploadedAfter) ? null : DateOnly.Parse(uploadedAfter);
        var authorTimeMin = r.ReadDouble();
        AuthorTimeMin = authorTimeMin == 0 ? null : new TimeInt32((int)authorTimeMin);
        var authorTimeMax = r.ReadDouble();
        AuthorTimeMax = authorTimeMax == 0 ? null : new TimeInt32((int)authorTimeMax);
    }
}