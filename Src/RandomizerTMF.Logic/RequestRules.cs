using RandomizerTMF.Logic.Exceptions;
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

    public string ToUrl() // Not very efficient but does the job done fast enough
    {
        var b = new StringBuilder("https://");

        var matchingSites = siteValues
            .Where(x => x != ESite.Any && (Site & x) == x)
            .ToArray();

        // If Site is Any, then it picks from sites that are valid within environments and cars
        var site = GetRandomSite(matchingSites.Length == 0
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
                val = GetRandomEnvironmentThroughSet(Environment);
            }

            if (EqualVehicleDistribution && prop.Name == nameof(Vehicle))
            {
                val = GetRandomEnvironmentThroughSet(Vehicle);
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

    private static EEnvironment GetRandomEnvironment(HashSet<EEnvironment>? container)
    {
        if (container is null || container.Count == 0)
        {
            return (EEnvironment)Random.Shared.Next(0, envValues.Length); // Safe in case of EEnvironment
        }
        
        return container.ElementAt(Random.Shared.Next(0, container.Count));
    }

    private static HashSet<EEnvironment> GetRandomEnvironmentThroughSet(HashSet<EEnvironment>? container)
    {
        return new HashSet<EEnvironment>() { GetRandomEnvironment(container) };
    }

    private static ESite GetRandomSite(ESite[] matchingSites)
    {
        return matchingSites[Random.Shared.Next(matchingSites.Length)];
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
}