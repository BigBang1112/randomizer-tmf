using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using TmEssentials;

namespace RandomizerTMF.Logic;

public class RequestRules
{
    public required ESite Site { get; set; }

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

    public string ToUrl(RandomizerRules additionalRules) // Not very efficient but does the job done fast enough
    {
        var b = new StringBuilder("https://");

        var matchingSites = Enum.GetValues<ESite>()
            .Where(x => x != ESite.Any && (Site & x) == x)
            .ToArray();

        var siteUrl = GetSiteUrl(matchingSites.Length == 0
            ? Enum.GetValues<ESite>().Where(x => x is not ESite.Any).ToArray()
            : matchingSites);

        b.Append(siteUrl);
        
        b.Append("/trackrandom");

        var first = true;

        foreach (var prop in GetType().GetProperties().Where(x => x.Name != nameof(Site)))
        {
            var val = prop.GetValue(this);
            if (NeedSkip(prop, additionalRules) && (val is not object || val is null || (val is IEnumerable enumerable && !enumerable.Cast<object>().Any())))
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

            if (prop.Name == nameof(Environment)){
                if (additionalRules.EvenEnvironmentDistribution)
                {
                    var a = GetRandomEEnvironment(Environment);
                    b.Append(a);
                    continue;
                }
            }

            if (prop.Name == nameof(Vehicle)){
                if (additionalRules.EvenVehicleDistribution)
                {
                    var c = GetRandomEEnvironment(Vehicle);
                    b.Append(c);
                    continue;
                }
            }

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

    private static bool NeedSkip(MemberInfo prop, RandomizerRules rules)
    {
        return !(prop.Name == nameof(Environment) && rules.EvenEnvironmentDistribution) &&
               !(prop.Name == nameof(Vehicle) && rules.EvenVehicleDistribution);
    }

    private static int GetRandomEEnvironment(IEnumerable<EEnvironment>? container)
    {
        if (container is null)
            return Random.Shared.Next(0, 7);
        var list = container.ToArray();
        return (int) list.ElementAt(Random.Shared.Next(0, list.Length));
    }
    
    private static string GetSiteUrl(ESite[] matchingSites)
    {
        var randomSite = matchingSites[Random.Shared.Next(matchingSites.Length)];

        return randomSite switch
        {
            ESite.Any => throw new UnreachableException("Any is not a valid site"),
            ESite.TMNF => "tmnf.exchange",
            ESite.TMUF => "tmuf.exchange",
            _ => $"{randomSite.ToString().ToLower()}.tm-exchange.com",
        };
    }

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