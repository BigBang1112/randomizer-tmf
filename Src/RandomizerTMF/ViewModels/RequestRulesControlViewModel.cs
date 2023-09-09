using RandomizerTMF.Logic;
using RandomizerTMF.Logic.Services;
using ReactiveUI;
using TmEssentials;

namespace RandomizerTMF.ViewModels;

internal class RequestRulesControlViewModel : WindowViewModelBase
{
    private readonly IRandomizerConfig config;

    // These properties have to be checked when saving presets
    // Any prop that enables something and is "connected" to another one (or multiple ones) by value
    // has to be put into these hashsets, coz of their nature when saving and loading presets
    
    public HashSet<string> PropsForEnable = new HashSet<string> { "MinATEnabled",
                                                                  "MaxATEnabled",
                                                                  "SurvivalEnabled",
                                                                  "FreeSkipLimitEnabled",
                                                                  "GoldSkipLimitEnabled"};

    public HashSet<string> PropsToReset = new HashSet<string> { "MinATMinute", "MinATSecond", "MinATMillisecond",
                                                                "MaxATMinute", "MaxATSecond", "MaxATMillisecond",
                                                                "SurvivalTimeGainMinute", "SurvivalTimeGainSecond",
                                                                "SurvivalTimeGainMillisecond", "FreeSkipLimit",
                                                                "GoldSkipLimit"};

    public RequestRulesControlViewModel(IRandomizerConfig config)
    {
        this.config = config;
    }

    // Because of the way the preset loading works, it is important to always have the props in the correct order
    // It means, that a prop related to enabling something always goes first, compared to the prop holding the value
    // For example
    // 1. FreeSkipLimitEnabled (determines wether you can or cannot set a value for FreeSkipLimit)
    // 2. FreeSkipLimit (actually holds the value for the amount of free skips)
    // If they were the other way around, the loader function would try to set the value first, (which it might fail to do,
    // because it might be disabled) and only then enable it
    //
    // Also the props are saved in the same order as they are declared
    // So the existing props' ORDER SHOULD NEVER BE CHANGED, otherwise presets saves from previous versions will not work properly
    // If the order is changed regardless, then DashboardWindowViewModel.PresetsDoubleClick should be modified accordingly,
    // so that the order does not effect the process
    // At the same time new props can be put anywhere, it won't break existing preset saves,
    // but existing preset saves will have no effect on that property (of course)

    public bool IsSiteTMNFChecked
    {
        get => config.Rules.RequestRules.Site.HasFlag(ESite.TMNF);
        set
        {
            if (value) config.Rules.RequestRules.Site |= ESite.TMNF;
            else config.Rules.RequestRules.Site &= ~ESite.TMNF;

            this.RaisePropertyChanged(nameof(IsSiteTMNFChecked));

            config.Save();
        }
    }

    public bool IsSiteTMUFChecked
    {
        get => config.Rules.RequestRules.Site.HasFlag(ESite.TMUF);
        set
        {
            if (value) config.Rules.RequestRules.Site |= ESite.TMUF;
            else config.Rules.RequestRules.Site &= ~ESite.TMUF;

            this.RaisePropertyChanged(nameof(IsSiteTMUFChecked));

            config.Save();
        }
    }

    public bool IsSiteNationsChecked
    {
        get => config.Rules.RequestRules.Site.HasFlag(ESite.Nations);
        set
        {
            if (value) config.Rules.RequestRules.Site |= ESite.Nations;
            else config.Rules.RequestRules.Site &= ~ESite.Nations;

            this.RaisePropertyChanged(nameof(IsSiteNationsChecked));

            config.Save();
        }
    }

    public bool IsSiteSunriseChecked
    {
        get => config.Rules.RequestRules.Site.HasFlag(ESite.Sunrise);
        set
        {
            if (value) config.Rules.RequestRules.Site |= ESite.Sunrise;
            else config.Rules.RequestRules.Site &= ~ESite.Sunrise;

            this.RaisePropertyChanged(nameof(IsSiteSunriseChecked));

            config.Save();
        }
    }

    public bool IsSiteOriginalChecked
    {
        get => config.Rules.RequestRules.Site.HasFlag(ESite.Original);
        set
        {
            if (value) config.Rules.RequestRules.Site |= ESite.Original;
            else config.Rules.RequestRules.Site &= ~ESite.Original;

            this.RaisePropertyChanged(nameof(IsSiteOriginalChecked));

            config.Save();
        }
    }

    public bool IsPrimaryTypeRaceChecked
    {
        get => config.Rules.RequestRules.PrimaryType is EPrimaryType.Race;
        set
        {
            if (value) config.Rules.RequestRules.PrimaryType = EPrimaryType.Race;
            else config.Rules.RequestRules.PrimaryType = null;

            this.RaisePropertyChanged(nameof(IsPrimaryTypeRaceChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePlatformChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypeStuntsChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePuzzleChecked));

            config.Save();
        }
    }

    public bool IsPrimaryTypePlatformChecked
    {
        get => config.Rules.RequestRules.PrimaryType is EPrimaryType.Platform;
        set
        {
            if (value) config.Rules.RequestRules.PrimaryType = EPrimaryType.Platform;
            else config.Rules.RequestRules.PrimaryType = null;

            this.RaisePropertyChanged(nameof(IsPrimaryTypeRaceChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePlatformChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypeStuntsChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePuzzleChecked));

            config.Save();
        }
    }

    public bool IsPrimaryTypeStuntsChecked
    {
        get => config.Rules.RequestRules.PrimaryType is EPrimaryType.Stunts;
        set
        {
            if (value) config.Rules.RequestRules.PrimaryType = EPrimaryType.Stunts;
            else config.Rules.RequestRules.PrimaryType = null;

            this.RaisePropertyChanged(nameof(IsPrimaryTypeRaceChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePlatformChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypeStuntsChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePuzzleChecked));

            config.Save();
        }
    }

    public bool IsPrimaryTypePuzzleChecked
    {
        get => config.Rules.RequestRules.PrimaryType is EPrimaryType.Puzzle;
        set
        {
            if (value) config.Rules.RequestRules.PrimaryType = EPrimaryType.Puzzle;
            else config.Rules.RequestRules.PrimaryType = null;

            this.RaisePropertyChanged(nameof(IsPrimaryTypeRaceChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePlatformChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypeStuntsChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePuzzleChecked));

            config.Save();
        }
    }

    public bool IsEnvironmentSnowChecked
    {
        get => config.Rules.RequestRules.Environment?.Contains(EEnvironment.Snow) == true;
        set
        {
            config.Rules.RequestRules.Environment ??= new();

            if (value) config.Rules.RequestRules.Environment.Add(EEnvironment.Snow);
            else config.Rules.RequestRules.Environment.Remove(EEnvironment.Snow);

            if (config.Rules.RequestRules.Environment.Count == 0)
            {
                config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentSnowChecked));

            config.Save();
        }
    }

    public bool IsEnvironmentDesertChecked
    {
        get => config.Rules.RequestRules.Environment?.Contains(EEnvironment.Desert) == true;
        set
        {
            config.Rules.RequestRules.Environment ??= new();

            if (value) config.Rules.RequestRules.Environment.Add(EEnvironment.Desert);
            else config.Rules.RequestRules.Environment.Remove(EEnvironment.Desert);

            if (config.Rules.RequestRules.Environment.Count == 0)
            {
                config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentDesertChecked));

            config.Save();
        }
    }

    public bool IsEnvironmentRallyChecked
    {
        get => config.Rules.RequestRules.Environment?.Contains(EEnvironment.Rally) == true;
        set
        {
            config.Rules.RequestRules.Environment ??= new();

            if (value) config.Rules.RequestRules.Environment.Add(EEnvironment.Rally);
            else config.Rules.RequestRules.Environment.Remove(EEnvironment.Rally);

            if (config.Rules.RequestRules.Environment.Count == 0)
            {
                config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentRallyChecked));

            config.Save();
        }
    }

    public bool IsEnvironmentIslandChecked
    {
        get => config.Rules.RequestRules.Environment?.Contains(EEnvironment.Island) == true;
        set
        {
            config.Rules.RequestRules.Environment ??= new();

            if (value) config.Rules.RequestRules.Environment.Add(EEnvironment.Island);
            else config.Rules.RequestRules.Environment.Remove(EEnvironment.Island);

            if (config.Rules.RequestRules.Environment.Count == 0)
            {
                config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentIslandChecked));

            config.Save();
        }
    }

    public bool IsEnvironmentCoastChecked
    {
        get => config.Rules.RequestRules.Environment?.Contains(EEnvironment.Coast) == true;
        set
        {
            config.Rules.RequestRules.Environment ??= new();

            if (value) config.Rules.RequestRules.Environment.Add(EEnvironment.Coast);
            else config.Rules.RequestRules.Environment.Remove(EEnvironment.Coast);

            if (config.Rules.RequestRules.Environment.Count == 0)
            {
                config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentCoastChecked));

            config.Save();
        }
    }

    public bool IsEnvironmentBayChecked
    {
        get => config.Rules.RequestRules.Environment?.Contains(EEnvironment.Bay) == true;
        set
        {
            config.Rules.RequestRules.Environment ??= new();

            if (value) config.Rules.RequestRules.Environment.Add(EEnvironment.Bay);
            else config.Rules.RequestRules.Environment.Remove(EEnvironment.Bay);

            if (config.Rules.RequestRules.Environment.Count == 0)
            {
                config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentBayChecked));

            config.Save();
        }
    }

    public bool IsEnvironmentStadiumChecked
    {
        get => config.Rules.RequestRules.Environment?.Contains(EEnvironment.Stadium) == true;
        set
        {
            config.Rules.RequestRules.Environment ??= new();

            if (value) config.Rules.RequestRules.Environment.Add(EEnvironment.Stadium);
            else config.Rules.RequestRules.Environment.Remove(EEnvironment.Stadium);

            if (config.Rules.RequestRules.Environment.Count == 0)
            {
                config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentStadiumChecked));

            config.Save();
        }
    }

    public bool IsVehicleSnowChecked
    {
        get => config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Snow) == true;
        set
        {
            config.Rules.RequestRules.Vehicle ??= new();

            if (value) config.Rules.RequestRules.Vehicle.Add(EEnvironment.Snow);
            else config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Snow);

            if (config.Rules.RequestRules.Vehicle.Count == 0)
            {
                config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleSnowChecked));

            config.Save();
        }
    }

    public bool IsVehicleDesertChecked
    {
        get => config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Desert) == true;
        set
        {
            config.Rules.RequestRules.Vehicle ??= new();

            if (value) config.Rules.RequestRules.Vehicle.Add(EEnvironment.Desert);
            else config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Desert);

            if (config.Rules.RequestRules.Vehicle.Count == 0)
            {
                config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleDesertChecked));

            config.Save();
        }
    }

    public bool IsVehicleRallyChecked
    {
        get => config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Rally) == true;
        set
        {
            config.Rules.RequestRules.Vehicle ??= new();

            if (value) config.Rules.RequestRules.Vehicle.Add(EEnvironment.Rally);
            else config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Rally);

            if (config.Rules.RequestRules.Vehicle.Count == 0)
            {
                config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleRallyChecked));

            config.Save();
        }
    }

    public bool IsVehicleIslandChecked
    {
        get => config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Island) == true;
        set
        {
            config.Rules.RequestRules.Vehicle ??= new();

            if (value) config.Rules.RequestRules.Vehicle.Add(EEnvironment.Island);
            else config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Island);

            if (config.Rules.RequestRules.Vehicle.Count == 0)
            {
                config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleIslandChecked));

            config.Save();
        }
    }

    public bool IsVehicleCoastChecked
    {
        get => config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Coast) == true;
        set
        {
            config.Rules.RequestRules.Vehicle ??= new();

            if (value) config.Rules.RequestRules.Vehicle.Add(EEnvironment.Coast);
            else config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Coast);

            if (config.Rules.RequestRules.Vehicle.Count == 0)
            {
                config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleCoastChecked));

            config.Save();
        }
    }

    public bool IsVehicleBayChecked
    {
        get => config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Bay) == true;
        set
        {
            config.Rules.RequestRules.Vehicle ??= new();

            if (value) config.Rules.RequestRules.Vehicle.Add(EEnvironment.Bay);
            else config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Bay);

            if (config.Rules.RequestRules.Vehicle.Count == 0)
            {
                config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleBayChecked));

            config.Save();
        }
    }

    public bool IsVehicleStadiumChecked
    {
        get => config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Stadium) == true;
        set
        {
            config.Rules.RequestRules.Vehicle ??= new();

            if (value) config.Rules.RequestRules.Vehicle.Add(EEnvironment.Stadium);
            else config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Stadium);

            if (config.Rules.RequestRules.Vehicle.Count == 0)
            {
                config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleStadiumChecked));

            config.Save();
        }
    }

    public bool IsDifficultyBeginnerChecked
    {
        get => config.Rules.RequestRules.Difficulty?.Contains(EDifficulty.Beginner) == true;
        set
        {
            config.Rules.RequestRules.Difficulty ??= new();

            if (value) config.Rules.RequestRules.Difficulty.Add(EDifficulty.Beginner);
            else config.Rules.RequestRules.Difficulty.Remove(EDifficulty.Beginner);

            if (config.Rules.RequestRules.Difficulty.Count == 0)
            {
                config.Rules.RequestRules.Difficulty = null;
            }

            this.RaisePropertyChanged(nameof(IsDifficultyBeginnerChecked));

            config.Save();
        }
    }

    public bool IsDifficultyIntermediateChecked
    {
        get => config.Rules.RequestRules.Difficulty?.Contains(EDifficulty.Intermediate) == true;
        set
        {
            config.Rules.RequestRules.Difficulty ??= new();

            if (value) config.Rules.RequestRules.Difficulty.Add(EDifficulty.Intermediate);
            else config.Rules.RequestRules.Difficulty.Remove(EDifficulty.Intermediate);

            if (config.Rules.RequestRules.Difficulty.Count == 0)
            {
                config.Rules.RequestRules.Difficulty = null;
            }

            this.RaisePropertyChanged(nameof(IsDifficultyIntermediateChecked));

            config.Save();
        }
    }

    public bool IsDifficultyExpertChecked
    {
        get => config.Rules.RequestRules.Difficulty?.Contains(EDifficulty.Expert) == true;
        set
        {
            config.Rules.RequestRules.Difficulty ??= new();

            if (value) config.Rules.RequestRules.Difficulty.Add(EDifficulty.Expert);
            else config.Rules.RequestRules.Difficulty.Remove(EDifficulty.Expert);

            if (config.Rules.RequestRules.Difficulty.Count == 0)
            {
                config.Rules.RequestRules.Difficulty = null;
            }

            this.RaisePropertyChanged(nameof(IsDifficultyExpertChecked));

            config.Save();
        }
    }

    public bool IsDifficultyLunaticChecked
    {
        get => config.Rules.RequestRules.Difficulty?.Contains(EDifficulty.Lunatic) == true;
        set
        {
            config.Rules.RequestRules.Difficulty ??= new();

            if (value) config.Rules.RequestRules.Difficulty.Add(EDifficulty.Lunatic);
            else config.Rules.RequestRules.Difficulty.Remove(EDifficulty.Lunatic);

            if (config.Rules.RequestRules.Difficulty.Count == 0)
            {
                config.Rules.RequestRules.Difficulty = null;
            }

            this.RaisePropertyChanged(nameof(IsDifficultyLunaticChecked));

            config.Save();
        }
    }

    public bool IsRouteSingleChecked
    {
        get => config.Rules.RequestRules.Routes?.Contains(ERoutes.Single) == true;
        set
        {
            config.Rules.RequestRules.Routes ??= new();

            if (value) config.Rules.RequestRules.Routes.Add(ERoutes.Single);
            else config.Rules.RequestRules.Routes.Remove(ERoutes.Single);

            if (config.Rules.RequestRules.Routes.Count == 0)
            {
                config.Rules.RequestRules.Routes = null;
            }

            this.RaisePropertyChanged(nameof(IsRouteSingleChecked));

            config.Save();
        }
    }

    public bool IsRouteMultiChecked
    {
        get => config.Rules.RequestRules.Routes?.Contains(ERoutes.Multi) == true;
        set
        {
            config.Rules.RequestRules.Routes ??= new();

            if (value) config.Rules.RequestRules.Routes.Add(ERoutes.Multi);
            else config.Rules.RequestRules.Routes.Remove(ERoutes.Multi);

            if (config.Rules.RequestRules.Routes.Count == 0)
            {
                config.Rules.RequestRules.Routes = null;
            }

            this.RaisePropertyChanged(nameof(IsRouteMultiChecked));

            config.Save();
        }
    }

    public bool IsRouteSymmetricChecked
    {
        get => config.Rules.RequestRules.Routes?.Contains(ERoutes.Symmetric) == true;
        set
        {
            config.Rules.RequestRules.Routes ??= new();

            if (value) config.Rules.RequestRules.Routes.Add(ERoutes.Symmetric);
            else config.Rules.RequestRules.Routes.Remove(ERoutes.Symmetric);

            if (config.Rules.RequestRules.Routes.Count == 0)
            {
                config.Rules.RequestRules.Routes = null;
            }

            this.RaisePropertyChanged(nameof(IsRouteSymmetricChecked));

            config.Save();
        }
    }

    public bool IsMoodSunriseChecked
    {
        get => config.Rules.RequestRules.Mood?.Contains(EMood.Sunrise) == true;
        set
        {
            config.Rules.RequestRules.Mood ??= new();

            if (value) config.Rules.RequestRules.Mood.Add(EMood.Sunrise);
            else config.Rules.RequestRules.Mood.Remove(EMood.Sunrise);

            if (config.Rules.RequestRules.Mood.Count == 0)
            {
                config.Rules.RequestRules.Mood = null;
            }

            this.RaisePropertyChanged(nameof(IsMoodSunriseChecked));

            config.Save();
        }
    }

    public bool IsMoodDayChecked
    {
        get => config.Rules.RequestRules.Mood?.Contains(EMood.Day) == true;
        set
        {
            config.Rules.RequestRules.Mood ??= new();

            if (value) config.Rules.RequestRules.Mood.Add(EMood.Day);
            else config.Rules.RequestRules.Mood.Remove(EMood.Day);

            if (config.Rules.RequestRules.Mood.Count == 0)
            {
                config.Rules.RequestRules.Mood = null;
            }

            this.RaisePropertyChanged(nameof(IsMoodDayChecked));

            config.Save();
        }
    }

    public bool IsMoodSunsetChecked
    {
        get => config.Rules.RequestRules.Mood?.Contains(EMood.Sunset) == true;
        set
        {
            config.Rules.RequestRules.Mood ??= new();

            if (value) config.Rules.RequestRules.Mood.Add(EMood.Sunset);
            else config.Rules.RequestRules.Mood.Remove(EMood.Sunset);

            if (config.Rules.RequestRules.Mood.Count == 0)
            {
                config.Rules.RequestRules.Mood = null;
            }

            this.RaisePropertyChanged(nameof(IsMoodSunsetChecked));

            config.Save();
        }
    }

    public bool IsMoodNightChecked
    {
        get => config.Rules.RequestRules.Mood?.Contains(EMood.Night) == true;
        set
        {
            config.Rules.RequestRules.Mood ??= new();

            if (value) config.Rules.RequestRules.Mood.Add(EMood.Night);
            else config.Rules.RequestRules.Mood.Remove(EMood.Night);

            if (config.Rules.RequestRules.Mood.Count == 0)
            {
                config.Rules.RequestRules.Mood = null;
            }

            this.RaisePropertyChanged(nameof(IsMoodNightChecked));

            config.Save();
        }
    }

    public string? MapName
    {
        get => config.Rules.RequestRules.Name;
        set
        {
            config.Rules.RequestRules.Name = string.IsNullOrWhiteSpace(value) ? null : value;

            this.RaisePropertyChanged(nameof(MapName));

            config.Save();
        }
    }

    public string? MapAuthor
    {
        get => config.Rules.RequestRules.Author;
        set
        {
            config.Rules.RequestRules.Author = string.IsNullOrWhiteSpace(value) ? null : value;

            this.RaisePropertyChanged(nameof(MapAuthor));

            config.Save();
        }
    }

    public static string[] TagValues { get; } = Enum.GetValues<ETag>().Select(x => x.ToString())
        .Prepend("(any)").ToArray();

    public int TagIndex
    {
        get => config.Rules.RequestRules.Tag.HasValue ? (int)config.Rules.RequestRules.Tag + 1 : 0;
        set
        {
            config.Rules.RequestRules.Tag = value <= 0 ? null : (ETag)(value - 1);

            this.RaisePropertyChanged(nameof(TagIndex));

            config.Save();
        }
    }

    public static string[] LbTypeValues { get; } = Enum.GetValues<ELbType>().Select(x => x.ToString())
        .Prepend("(any)").ToArray();

    public int LbTypeIndex
    {
        get => config.Rules.RequestRules.LbType.HasValue ? (int)config.Rules.RequestRules.LbType + 1 : 0;
        set
        {
            config.Rules.RequestRules.LbType = value <= 0 ? null : (ELbType)(value - 1);

            this.RaisePropertyChanged(nameof(LbTypeIndex));

            config.Save();
        }
    }

    public int TimeLimitHour
    {
        get => config.Rules.TimeLimit.Hours;
        set
        {
            config.Rules.TimeLimit = new TimeSpan(value,
                config.Rules.TimeLimit.Minutes,
                config.Rules.TimeLimit.Seconds);

            config.Rules.OriginalTimeLimit = config.Rules.TimeLimit;

            this.RaisePropertyChanged(nameof(TimeLimitHour));

            config.Save();
        }
    }

    public int TimeLimitMinute
    {
        get => config.Rules.TimeLimit.Minutes;
        set
        {
            config.Rules.TimeLimit = new TimeSpan(config.Rules.TimeLimit.Hours,
                value,
                config.Rules.TimeLimit.Seconds);

            config.Rules.OriginalTimeLimit = config.Rules.TimeLimit;

            this.RaisePropertyChanged(nameof(TimeLimitMinute));

            config.Save();
        }
    }

    public int TimeLimitSecond
    {
        get => config.Rules.TimeLimit.Seconds;
        set
        {
            config.Rules.TimeLimit = new TimeSpan(config.Rules.TimeLimit.Hours,
                config.Rules.TimeLimit.Minutes,
                value);

            config.Rules.OriginalTimeLimit = config.Rules.TimeLimit;

            this.RaisePropertyChanged(nameof(TimeLimitSecond));

            config.Save();
        }
    }

    public bool MinATEnabled
    {
        get => config.Rules.RequestRules.AuthorTimeMin is not null;
        set
        {
            config.Rules.RequestRules.AuthorTimeMin = value ? TimeInt32.Zero : null;

            this.RaisePropertyChanged(nameof(MinATEnabled));
            this.RaisePropertyChanged(nameof(MinATMinute));
            this.RaisePropertyChanged(nameof(MinATSecond));
            this.RaisePropertyChanged(nameof(MinATMillisecond));

            config.Save();
        }
    }

    public int MinATMinute
    {
        get => config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Minutes;
        set
        {
            config.Rules.RequestRules.AuthorTimeMin = new TimeInt32(0, 0, value,
                config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Seconds,
                config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Milliseconds);

            this.RaisePropertyChanged(nameof(MinATMinute));

            config.Save();
        }
    }

    public int MinATSecond
    {
        get => config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Seconds;
        set
        {
            config.Rules.RequestRules.AuthorTimeMin = new TimeInt32(0, 0,
                config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Minutes,
                value,
                config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Milliseconds);

            this.RaisePropertyChanged(nameof(MinATSecond));

            config.Save();
        }
    }

    public int MinATMillisecond
    {
        get => config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Milliseconds / 10;
        set
        {
            config.Rules.RequestRules.AuthorTimeMin = new TimeInt32(0, 0,
                config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Minutes,
                config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Seconds,
                value * 10);

            this.RaisePropertyChanged(nameof(MinATMillisecond));

            config.Save();
        }
    }

    public bool MaxATEnabled
    {
        get => config.Rules.RequestRules.AuthorTimeMax is not null;
        set
        {
            config.Rules.RequestRules.AuthorTimeMax = value ? TimeInt32.Zero : null;

            this.RaisePropertyChanged(nameof(MaxATEnabled));
            this.RaisePropertyChanged(nameof(MaxATMinute));
            this.RaisePropertyChanged(nameof(MaxATSecond));
            this.RaisePropertyChanged(nameof(MaxATMillisecond));

            config.Save();
        }
    }

    public int MaxATMinute
    {
        get => config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Minutes;
        set
        {
            config.Rules.RequestRules.AuthorTimeMax = new TimeInt32(0, 0, value,
                config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Seconds,
                config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Milliseconds);

            this.RaisePropertyChanged(nameof(MaxATMinute));

            config.Save();
        }
    }

    public int MaxATSecond
    {
        get => config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Seconds;
        set
        {
            config.Rules.RequestRules.AuthorTimeMax = new TimeInt32(0, 0,
                config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Minutes,
                value,
                config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Milliseconds);

            this.RaisePropertyChanged(nameof(MaxATSecond));

            config.Save();
        }
    }

    public int MaxATMillisecond
    {
        get => config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Milliseconds / 10;
        set
        {
            config.Rules.RequestRules.AuthorTimeMax = new TimeInt32(0, 0,
                config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Minutes,
                config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Seconds,
                value * 10);

            this.RaisePropertyChanged(nameof(MaxATMillisecond));

            config.Save();
        }
    }

    public bool SurvivalEnabled
    {
        get => config.Rules.RequestRules.SurvivalBonusTime is not null;
        set
        {
            config.Rules.RequestRules.SurvivalMode = value;
            config.Rules.RequestRules.SurvivalBonusTime = value ? TimeSpan.Zero : null;

            this.RaisePropertyChanged(nameof(SurvivalEnabled));
            this.RaisePropertyChanged(nameof(SurvivalTimeGainMinute));
            this.RaisePropertyChanged(nameof(SurvivalTimeGainSecond));
            this.RaisePropertyChanged(nameof(SurvivalTimeGainMillisecond));

            config.Save();
        }
    }

    public int SurvivalTimeGainMinute
    {
        get => config.Rules.RequestRules.SurvivalBonusTime.GetValueOrDefault().Minutes;
        set
        {
            config.Rules.RequestRules.SurvivalBonusTime = new TimeSpan(0, 0, value,
                config.Rules.RequestRules.SurvivalBonusTime.GetValueOrDefault().Seconds,
                config.Rules.RequestRules.SurvivalBonusTime.GetValueOrDefault().Milliseconds);

            this.RaisePropertyChanged(nameof(SurvivalTimeGainMinute));

            config.Save();
        }
    }

    public int SurvivalTimeGainSecond
    {
        get => config.Rules.RequestRules.SurvivalBonusTime.GetValueOrDefault().Seconds;
        set
        {
            config.Rules.RequestRules.SurvivalBonusTime = new TimeSpan(0, 0,
                config.Rules.RequestRules.SurvivalBonusTime.GetValueOrDefault().Minutes,
                value,
                config.Rules.RequestRules.SurvivalBonusTime.GetValueOrDefault().Milliseconds);

            this.RaisePropertyChanged(nameof(SurvivalTimeGainSecond));

            config.Save();
        }
    }

    public int SurvivalTimeGainMillisecond
    {
        get => config.Rules.RequestRules.SurvivalBonusTime.GetValueOrDefault().Milliseconds;
        set
        {
            config.Rules.RequestRules.SurvivalBonusTime = new TimeSpan(0, 0,
                config.Rules.RequestRules.SurvivalBonusTime.GetValueOrDefault().Minutes,
                config.Rules.RequestRules.SurvivalBonusTime.GetValueOrDefault().Seconds,
                value * 10);

            this.RaisePropertyChanged(nameof(SurvivalTimeGainMillisecond));

            config.Save();
        }
    }

    public bool FreeSkipLimitEnabled
    {
        get => config.Rules.RequestRules.FreeSkipLimit is not null;
        set
        {
            config.Rules.RequestRules.FreeSkipLimit = value ? 0 : null;

            this.RaisePropertyChanged(nameof(FreeSkipLimitEnabled));
            this.RaisePropertyChanged(nameof(FreeSkipLimit));

            config.Save();
        }
    }

    public int FreeSkipLimit
    {
        get => config.Rules.RequestRules.FreeSkipLimit.GetValueOrDefault();
        set
        {
            config.Rules.RequestRules.FreeSkipLimit = value;

            this.RaisePropertyChanged(nameof(FreeSkipLimit));

            config.Save();
        }
    }

    public bool GoldSkipLimitEnabled
    {
        get => config.Rules.RequestRules.GoldSkipLimit is not null;
        set
        {
            config.Rules.RequestRules.GoldSkipLimit = value ? 0 : null;

            this.RaisePropertyChanged(nameof(GoldSkipLimitEnabled));
            this.RaisePropertyChanged(nameof(GoldSkipLimit));

            config.Save();
        }
    }

    public int GoldSkipLimit
    {
        get => config.Rules.RequestRules.GoldSkipLimit.GetValueOrDefault();
        set
        {
            config.Rules.RequestRules.GoldSkipLimit = value;

            this.RaisePropertyChanged(nameof(GoldSkipLimit));

            config.Save();
        }
    }

    public DateTimeOffset? UploadedAfter
    {
        get => config.Rules.RequestRules.UploadedAfter?.ToDateTime(new());
        set
        {
            config.Rules.RequestRules.UploadedAfter = value.HasValue ? DateOnly.FromDateTime(value.Value.DateTime) : null;

            this.RaisePropertyChanged(nameof(UploadedAfter));

            config.Save();
        }
    }

    public void UploadedAfterReset() => UploadedAfter = null;

    public DateTimeOffset? UploadedBefore
    {
        get => config.Rules.RequestRules.UploadedBefore?.ToDateTime(new());
        set
        {
            config.Rules.RequestRules.UploadedBefore = value.HasValue ? DateOnly.FromDateTime(value.Value.DateTime) : null;

            this.RaisePropertyChanged(nameof(UploadedBefore));

            config.Save();
        }
    }

    public bool EqualEnvDistribution
    {
        get => config.Rules.RequestRules.EqualEnvironmentDistribution;
        set
        {
            config.Rules.RequestRules.EqualEnvironmentDistribution = value;

            this.RaisePropertyChanged(nameof(EqualEnvDistribution));

            config.Save();
        }
    }

    public bool EqualVehicleDistribution
    {
        get => config.Rules.RequestRules.EqualVehicleDistribution;
        set
        {
            config.Rules.RequestRules.EqualVehicleDistribution = value;

            this.RaisePropertyChanged(nameof(EqualVehicleDistribution));

            config.Save();
        }
    }

    public void UploadedBeforeReset() => UploadedBefore = null;
}
