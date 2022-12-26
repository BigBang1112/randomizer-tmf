using GBX.NET.Engines.Game;
using RandomizerTMF.Logic.Exceptions;

namespace RandomizerTMF.Logic.Services;

public interface IValidator
{
    bool ValidateMap(CGameCtnChallenge map, out string? invalidBlock);
    void ValidateRequestRules(RequestRules requestRules);
    void ValidateRules(RandomizerRules rules);
}

public class Validator : IValidator
{
    private readonly IAutosaveScanner autosaveScanner;
    private readonly IAdditionalData additionalData;
    private readonly IRandomizerConfig config;

    public Validator(IAutosaveScanner autosaveScanner, IAdditionalData additionalData, IRandomizerConfig config)
    {
        this.autosaveScanner = autosaveScanner;
        this.additionalData = additionalData;
        this.config = config;
    }

    /// <summary>
    /// Validates the session rules. This should be called right before the session start and after loading the modules.
    /// </summary>
    /// <exception cref="RuleValidationException"></exception>
    public void ValidateRules(RandomizerRules rules)
    {
        if (rules.TimeLimit == TimeSpan.Zero)
        {
            throw new RuleValidationException("Time limit cannot be 0:00:00.");
        }

        if (rules.TimeLimit > new TimeSpan(9, 59, 59))
        {
            throw new RuleValidationException("Time limit cannot be above 9:59:59.");
        }

        ValidateRequestRules(rules.RequestRules);
    }

    public void ValidateRequestRules(RequestRules requestRules)
    {
        foreach (var primaryType in Enum.GetValues<EPrimaryType>())
        {
            if (primaryType is EPrimaryType.Race)
            {
                continue;
            }

            if (requestRules.PrimaryType == primaryType
            && (requestRules.Site is ESite.Any
             || requestRules.Site.HasFlag(ESite.TMNF) || requestRules.Site.HasFlag(ESite.Nations)))
            {
                throw new RuleValidationException($"{primaryType} cannot be specifically selected with TMNF or Nations Exchange.");
            }
        }

        if (requestRules.Environment?.Count > 0)
        {
            if (requestRules.Site.HasFlag(ESite.Sunrise)
            && !requestRules.Environment.Contains(EEnvironment.Island)
            && !requestRules.Environment.Contains(EEnvironment.Coast)
            && !requestRules.Environment.Contains(EEnvironment.Bay))
            {
                throw new RuleValidationException("Island, Coast, or Bay has to be selected when environments are specified and Sunrise Exchange is selected.");
            }

            if (requestRules.Site.HasFlag(ESite.Original)
            && !requestRules.Environment.Contains(EEnvironment.Snow)
            && !requestRules.Environment.Contains(EEnvironment.Desert)
            && !requestRules.Environment.Contains(EEnvironment.Rally))
            {
                throw new RuleValidationException("Snow, Desert, or Rally has to be selected when environments are specified and Original Exchange is selected.");
            }

            if (!requestRules.Environment.Contains(EEnvironment.Stadium)
             && (requestRules.Site.HasFlag(ESite.TMNF) || requestRules.Site.HasFlag(ESite.Nations)))
            {
                throw new RuleValidationException("Stadium has to be selected when environments are specified and TMNF or Nations Exchange is selected.");
            }

            if (requestRules.Site.HasFlag(ESite.Sunrise) || requestRules.Site.HasFlag(ESite.Original))
            {
                foreach (var env in requestRules.Environment)
                {
                    if (requestRules.Vehicle?.Contains(env) == false)
                    {
                        throw new RuleValidationException("Envimix randomization is not allowed when Sunrise or Original Exchange is selected.");
                    }
                }
            }
        }

        if (requestRules.Vehicle?.Count > 0)
        {
            if (!requestRules.Vehicle.Contains(EEnvironment.Island)
             && !requestRules.Vehicle.Contains(EEnvironment.Coast)
             && !requestRules.Vehicle.Contains(EEnvironment.Bay)
              && requestRules.Site.HasFlag(ESite.Sunrise))
            {
                throw new RuleValidationException("IslandCar, CoastCar, or BayCar has to be selected when cars are specified and Sunrise Exchange is selected.");
            }

            if (!requestRules.Vehicle.Contains(EEnvironment.Snow)
             && !requestRules.Vehicle.Contains(EEnvironment.Desert)
             && !requestRules.Vehicle.Contains(EEnvironment.Rally)
              && requestRules.Site.HasFlag(ESite.Original))
            {
                throw new RuleValidationException("SnowCar, DesertCar, or RallyCar has to be selected when cars are specified and Original Exchange is selected.");
            }

            if (!requestRules.Vehicle.Contains(EEnvironment.Stadium)
             && (requestRules.Site.HasFlag(ESite.TMNF) || requestRules.Site.HasFlag(ESite.Nations)))
            {
                throw new RuleValidationException("StadiumCar has to be selected when cars are specified and TMNF or Nations Exchange is selected.");
            }
        }

        if (requestRules.EqualEnvironmentDistribution
         && requestRules.EqualVehicleDistribution
         && requestRules.Site is not ESite.TMUF)
        {
            throw new RuleValidationException("Equal environment and car distribution combined is only valid with TMUF Exchange.");
        }

        if (requestRules.EqualEnvironmentDistribution
        && (requestRules.Site is ESite.Any
         || requestRules.Site.HasFlag(ESite.TMNF) || requestRules.Site.HasFlag(ESite.Nations)))
        {
            throw new RuleValidationException("Equal environment distribution is not valid with TMNF or Nations Exchange.");
        }

        if (requestRules.EqualVehicleDistribution
        && (requestRules.Site is ESite.Any
         || requestRules.Site.HasFlag(ESite.TMNF) || requestRules.Site.HasFlag(ESite.Nations)))
        {
            throw new RuleValidationException("Equal vehicle distribution is not valid with TMNF or Nations Exchange.");
        }
    }

    /// <summary>
    /// Checks if the map hasn't been already played or if it follows current session rules.
    /// </summary>
    /// <param name="autosaveScanner">Autosave information.</param>
    /// <param name="map"></param>
    /// <returns>True if valid, false if not valid.</returns>
    public bool ValidateMap(CGameCtnChallenge map, out string? invalidBlock)
    {
        invalidBlock = null;

        if (autosaveScanner.AutosaveHeaders.ContainsKey(map.MapUid))
        {
            return false;
        }

        if (config.Rules.NoUnlimiter)
        {
            if (map.Chunks.TryGet(0x3F001000, out _))
            {
                return false;
            }

            if (additionalData.MapSizes.TryGetValue(map.Collection, out var sizes))
            {
                if (map.Size is null || !sizes.Contains(map.Size.Value))
                {
                    return false;
                }
            }

            if (additionalData.OfficialBlocks.TryGetValue(map.Collection, out var officialBlocks))
            {
                foreach (var block in map.GetBlocks())
                {
                    var blockName = block.Name.Trim();

                    if (!officialBlocks.Contains(blockName))
                    {
                        invalidBlock = blockName;
                        return false;
                    }
                }
            }
        }

        return true;
    }
}
