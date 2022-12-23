using RandomizerTMF.Logic;
using ReactiveUI;
using TmEssentials;

namespace RandomizerTMF.ViewModels;

public class RequestRulesControlViewModel : WindowViewModelBase
{
    public bool IsSiteTMNFChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Site.HasFlag(ESite.TMNF);
        set
        {
            if (value) RandomizerEngine.Config.Rules.RequestRules.Site |= ESite.TMNF;
            else RandomizerEngine.Config.Rules.RequestRules.Site &= ~ESite.TMNF;
            
            this.RaisePropertyChanged(nameof(IsSiteTMNFChecked));

            RandomizerEngine.Config.Save();
        }
    }
    
    public bool IsSiteTMUFChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Site.HasFlag(ESite.TMUF);
        set
        {
            if (value) RandomizerEngine.Config.Rules.RequestRules.Site |= ESite.TMUF;
            else RandomizerEngine.Config.Rules.RequestRules.Site &= ~ESite.TMUF;

            this.RaisePropertyChanged(nameof(IsSiteTMUFChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsSiteNationsChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Site.HasFlag(ESite.Nations);
        set
        {
            if (value) RandomizerEngine.Config.Rules.RequestRules.Site |= ESite.Nations;
            else RandomizerEngine.Config.Rules.RequestRules.Site &= ~ESite.Nations;

            this.RaisePropertyChanged(nameof(IsSiteNationsChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsSiteSunriseChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Site.HasFlag(ESite.Sunrise);
        set
        {
            if (value) RandomizerEngine.Config.Rules.RequestRules.Site |= ESite.Sunrise;
            else RandomizerEngine.Config.Rules.RequestRules.Site &= ~ESite.Sunrise;

            this.RaisePropertyChanged(nameof(IsSiteSunriseChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsSiteOriginalChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Site.HasFlag(ESite.Original);
        set
        {
            if (value) RandomizerEngine.Config.Rules.RequestRules.Site |= ESite.Original;
            else RandomizerEngine.Config.Rules.RequestRules.Site &= ~ESite.Original;

            this.RaisePropertyChanged(nameof(IsSiteOriginalChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsPrimaryTypeRaceChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.PrimaryType is EPrimaryType.Race;
        set
        {
            if (value) RandomizerEngine.Config.Rules.RequestRules.PrimaryType = EPrimaryType.Race;
            else RandomizerEngine.Config.Rules.RequestRules.PrimaryType = null;

            this.RaisePropertyChanged(nameof(IsPrimaryTypeRaceChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePlatformChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypeStuntsChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePuzzleChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsPrimaryTypePlatformChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.PrimaryType is EPrimaryType.Platform;
        set
        {
            if (value) RandomizerEngine.Config.Rules.RequestRules.PrimaryType = EPrimaryType.Platform;
            else RandomizerEngine.Config.Rules.RequestRules.PrimaryType = null;

            this.RaisePropertyChanged(nameof(IsPrimaryTypeRaceChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePlatformChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypeStuntsChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePuzzleChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsPrimaryTypeStuntsChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.PrimaryType is EPrimaryType.Stunts;
        set
        {
            if (value) RandomizerEngine.Config.Rules.RequestRules.PrimaryType = EPrimaryType.Stunts;
            else RandomizerEngine.Config.Rules.RequestRules.PrimaryType = null;

            this.RaisePropertyChanged(nameof(IsPrimaryTypeRaceChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePlatformChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypeStuntsChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePuzzleChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsPrimaryTypePuzzleChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.PrimaryType is EPrimaryType.Puzzle;
        set
        {
            if (value) RandomizerEngine.Config.Rules.RequestRules.PrimaryType = EPrimaryType.Puzzle;
            else RandomizerEngine.Config.Rules.RequestRules.PrimaryType = null;

            this.RaisePropertyChanged(nameof(IsPrimaryTypeRaceChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePlatformChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypeStuntsChecked));
            this.RaisePropertyChanged(nameof(IsPrimaryTypePuzzleChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsEnvironmentSnowChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Environment?.Contains(EEnvironment.Snow) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Environment ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Environment.Add(EEnvironment.Snow);
            else RandomizerEngine.Config.Rules.RequestRules.Environment.Remove(EEnvironment.Snow);

            if (RandomizerEngine.Config.Rules.RequestRules.Environment.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentSnowChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsEnvironmentDesertChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Environment?.Contains(EEnvironment.Desert) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Environment ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Environment.Add(EEnvironment.Desert);
            else RandomizerEngine.Config.Rules.RequestRules.Environment.Remove(EEnvironment.Desert);

            if (RandomizerEngine.Config.Rules.RequestRules.Environment.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentDesertChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsEnvironmentRallyChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Environment?.Contains(EEnvironment.Rally) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Environment ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Environment.Add(EEnvironment.Rally);
            else RandomizerEngine.Config.Rules.RequestRules.Environment.Remove(EEnvironment.Rally);

            if (RandomizerEngine.Config.Rules.RequestRules.Environment.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentRallyChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsEnvironmentIslandChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Environment?.Contains(EEnvironment.Island) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Environment ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Environment.Add(EEnvironment.Island);
            else RandomizerEngine.Config.Rules.RequestRules.Environment.Remove(EEnvironment.Island);

            if (RandomizerEngine.Config.Rules.RequestRules.Environment.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentIslandChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsEnvironmentCoastChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Environment?.Contains(EEnvironment.Coast) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Environment ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Environment.Add(EEnvironment.Coast);
            else RandomizerEngine.Config.Rules.RequestRules.Environment.Remove(EEnvironment.Coast);

            if (RandomizerEngine.Config.Rules.RequestRules.Environment.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentCoastChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsEnvironmentBayChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Environment?.Contains(EEnvironment.Bay) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Environment ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Environment.Add(EEnvironment.Bay);
            else RandomizerEngine.Config.Rules.RequestRules.Environment.Remove(EEnvironment.Bay);

            if (RandomizerEngine.Config.Rules.RequestRules.Environment.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentBayChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsEnvironmentStadiumChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Environment?.Contains(EEnvironment.Stadium) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Environment ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Environment.Add(EEnvironment.Stadium);
            else RandomizerEngine.Config.Rules.RequestRules.Environment.Remove(EEnvironment.Stadium);

            if (RandomizerEngine.Config.Rules.RequestRules.Environment.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Environment = null;
            }

            this.RaisePropertyChanged(nameof(IsEnvironmentStadiumChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsVehicleSnowChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Snow) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Vehicle ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Vehicle.Add(EEnvironment.Snow);
            else RandomizerEngine.Config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Snow);

            if (RandomizerEngine.Config.Rules.RequestRules.Vehicle.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleSnowChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsVehicleDesertChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Desert) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Vehicle ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Vehicle.Add(EEnvironment.Desert);
            else RandomizerEngine.Config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Desert);

            if (RandomizerEngine.Config.Rules.RequestRules.Vehicle.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleDesertChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsVehicleRallyChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Rally) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Vehicle ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Vehicle.Add(EEnvironment.Rally);
            else RandomizerEngine.Config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Rally);

            if (RandomizerEngine.Config.Rules.RequestRules.Vehicle.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleRallyChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsVehicleIslandChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Island) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Vehicle ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Vehicle.Add(EEnvironment.Island);
            else RandomizerEngine.Config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Island);

            if (RandomizerEngine.Config.Rules.RequestRules.Vehicle.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleIslandChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsVehicleCoastChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Coast) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Vehicle ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Vehicle.Add(EEnvironment.Coast);
            else RandomizerEngine.Config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Coast);

            if (RandomizerEngine.Config.Rules.RequestRules.Vehicle.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleCoastChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsVehicleBayChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Bay) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Vehicle ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Vehicle.Add(EEnvironment.Bay);
            else RandomizerEngine.Config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Bay);

            if (RandomizerEngine.Config.Rules.RequestRules.Vehicle.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleBayChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsVehicleStadiumChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Vehicle?.Contains(EEnvironment.Stadium) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Vehicle ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Vehicle.Add(EEnvironment.Stadium);
            else RandomizerEngine.Config.Rules.RequestRules.Vehicle.Remove(EEnvironment.Stadium);

            if (RandomizerEngine.Config.Rules.RequestRules.Vehicle.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Vehicle = null;
            }

            this.RaisePropertyChanged(nameof(IsVehicleStadiumChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsDifficultyBeginnerChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Difficulty?.Contains(EDifficulty.Beginner) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Difficulty ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Difficulty.Add(EDifficulty.Beginner);
            else RandomizerEngine.Config.Rules.RequestRules.Difficulty.Remove(EDifficulty.Beginner);

            if (RandomizerEngine.Config.Rules.RequestRules.Difficulty.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Difficulty = null;
            }

            this.RaisePropertyChanged(nameof(IsDifficultyBeginnerChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsDifficultyIntermediateChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Difficulty?.Contains(EDifficulty.Intermediate) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Difficulty ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Difficulty.Add(EDifficulty.Intermediate);
            else RandomizerEngine.Config.Rules.RequestRules.Difficulty.Remove(EDifficulty.Intermediate);

            if (RandomizerEngine.Config.Rules.RequestRules.Difficulty.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Difficulty = null;
            }

            this.RaisePropertyChanged(nameof(IsDifficultyIntermediateChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsDifficultyExpertChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Difficulty?.Contains(EDifficulty.Expert) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Difficulty ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Difficulty.Add(EDifficulty.Expert);
            else RandomizerEngine.Config.Rules.RequestRules.Difficulty.Remove(EDifficulty.Expert);

            if (RandomizerEngine.Config.Rules.RequestRules.Difficulty.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Difficulty = null;
            }

            this.RaisePropertyChanged(nameof(IsDifficultyExpertChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsDifficultyLunaticChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Difficulty?.Contains(EDifficulty.Lunatic) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Difficulty ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Difficulty.Add(EDifficulty.Lunatic);
            else RandomizerEngine.Config.Rules.RequestRules.Difficulty.Remove(EDifficulty.Lunatic);

            if (RandomizerEngine.Config.Rules.RequestRules.Difficulty.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Difficulty = null;
            }

            this.RaisePropertyChanged(nameof(IsDifficultyLunaticChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsRouteSingleChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Routes?.Contains(ERoutes.Single) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Routes ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Routes.Add(ERoutes.Single);
            else RandomizerEngine.Config.Rules.RequestRules.Routes.Remove(ERoutes.Single);

            if (RandomizerEngine.Config.Rules.RequestRules.Routes.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Routes = null;
            }

            this.RaisePropertyChanged(nameof(IsRouteSingleChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsRouteMultiChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Routes?.Contains(ERoutes.Multi) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Routes ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Routes.Add(ERoutes.Multi);
            else RandomizerEngine.Config.Rules.RequestRules.Routes.Remove(ERoutes.Multi);

            if (RandomizerEngine.Config.Rules.RequestRules.Routes.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Routes = null;
            }

            this.RaisePropertyChanged(nameof(IsRouteMultiChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsRouteSymmetricChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Routes?.Contains(ERoutes.Symmetric) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Routes ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Routes.Add(ERoutes.Symmetric);
            else RandomizerEngine.Config.Rules.RequestRules.Routes.Remove(ERoutes.Symmetric);

            if (RandomizerEngine.Config.Rules.RequestRules.Routes.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Routes = null;
            }

            this.RaisePropertyChanged(nameof(IsRouteSymmetricChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsMoodSunriseChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Mood?.Contains(EMood.Sunrise) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Mood ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Mood.Add(EMood.Sunrise);
            else RandomizerEngine.Config.Rules.RequestRules.Mood.Remove(EMood.Sunrise);

            if (RandomizerEngine.Config.Rules.RequestRules.Mood.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Mood = null;
            }

            this.RaisePropertyChanged(nameof(IsMoodSunriseChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsMoodDayChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Mood?.Contains(EMood.Day) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Mood ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Mood.Add(EMood.Day);
            else RandomizerEngine.Config.Rules.RequestRules.Mood.Remove(EMood.Day);

            if (RandomizerEngine.Config.Rules.RequestRules.Mood.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Mood = null;
            }

            this.RaisePropertyChanged(nameof(IsMoodDayChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsMoodSunsetChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Mood?.Contains(EMood.Sunset) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Mood ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Mood.Add(EMood.Sunset);
            else RandomizerEngine.Config.Rules.RequestRules.Mood.Remove(EMood.Sunset);

            if (RandomizerEngine.Config.Rules.RequestRules.Mood.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Mood = null;
            }

            this.RaisePropertyChanged(nameof(IsMoodSunsetChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public bool IsMoodNightChecked
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Mood?.Contains(EMood.Night) == true;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Mood ??= new();

            if (value) RandomizerEngine.Config.Rules.RequestRules.Mood.Add(EMood.Night);
            else RandomizerEngine.Config.Rules.RequestRules.Mood.Remove(EMood.Night);

            if (RandomizerEngine.Config.Rules.RequestRules.Mood.Count == 0)
            {
                RandomizerEngine.Config.Rules.RequestRules.Mood = null;
            }

            this.RaisePropertyChanged(nameof(IsMoodNightChecked));

            RandomizerEngine.Config.Save();
        }
    }

    public string? MapName
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Name;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Name = string.IsNullOrWhiteSpace(value) ? null : value;

            this.RaisePropertyChanged(nameof(MapName));

            RandomizerEngine.Config.Save();
        }
    }

    public string? MapAuthor
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Author;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Author = string.IsNullOrWhiteSpace(value) ? null : value;

            this.RaisePropertyChanged(nameof(MapAuthor));

            RandomizerEngine.Config.Save();
        }
    }

    public static string[] TagValues { get; } = Enum.GetValues<ETag>().Select(x => x.ToString())
        .Prepend("(any)").ToArray();

    public int TagIndex
    {
        get => RandomizerEngine.Config.Rules.RequestRules.Tag.HasValue ? (int)RandomizerEngine.Config.Rules.RequestRules.Tag + 1 : 0;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.Tag = value <= 0 ? null : (ETag)(value - 1);

            this.RaisePropertyChanged(nameof(TagIndex));

            RandomizerEngine.Config.Save();
        }
    }

    public static string[] LbTypeValues { get; } = Enum.GetValues<ELbType>().Select(x => x.ToString())
        .Prepend("(any)").ToArray();

    public int LbTypeIndex
    {
        get => RandomizerEngine.Config.Rules.RequestRules.LbType.HasValue ? (int)RandomizerEngine.Config.Rules.RequestRules.LbType + 1 : 0;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.LbType = value <= 0 ? null : (ELbType)(value - 1);

            this.RaisePropertyChanged(nameof(LbTypeIndex));

            RandomizerEngine.Config.Save();
        }
    }

    public int TimeLimitHour
    {
        get => RandomizerEngine.Config.Rules.TimeLimit.Hours;
        set
        {
            RandomizerEngine.Config.Rules.TimeLimit = new TimeSpan(value,
                RandomizerEngine.Config.Rules.TimeLimit.Minutes,
                RandomizerEngine.Config.Rules.TimeLimit.Seconds);

            this.RaisePropertyChanged(nameof(TimeLimitHour));

            RandomizerEngine.Config.Save();
        }
    }

    public int TimeLimitMinute
    {
        get => RandomizerEngine.Config.Rules.TimeLimit.Minutes;
        set
        {
            RandomizerEngine.Config.Rules.TimeLimit = new TimeSpan(RandomizerEngine.Config.Rules.TimeLimit.Hours,
                value,
                RandomizerEngine.Config.Rules.TimeLimit.Seconds);

            this.RaisePropertyChanged(nameof(TimeLimitMinute));

            RandomizerEngine.Config.Save();
        }
    }

    public int TimeLimitSecond
    {
        get => RandomizerEngine.Config.Rules.TimeLimit.Seconds;
        set
        {
            RandomizerEngine.Config.Rules.TimeLimit = new TimeSpan(RandomizerEngine.Config.Rules.TimeLimit.Hours,
                RandomizerEngine.Config.Rules.TimeLimit.Minutes,
                value);

            this.RaisePropertyChanged(nameof(TimeLimitSecond));

            RandomizerEngine.Config.Save();
        }
    }

    public int MinATMinute
    {
        get => RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Minutes;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin = new TimeInt32(0, 0, value,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Seconds,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Milliseconds);

            this.RaisePropertyChanged(nameof(MinATMinute));

            RandomizerEngine.Config.Save();
        }
    }

    public int MinATSecond
    {
        get => RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Seconds;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin = new TimeInt32(0, 0,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Minutes,
                value,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Milliseconds);

            this.RaisePropertyChanged(nameof(MinATSecond));

            RandomizerEngine.Config.Save();
        }
    }

    public int MinATMillisecond
    {
        get => RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Milliseconds / 10;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin = new TimeInt32(0, 0,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Minutes,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin.GetValueOrDefault().Seconds,
                value * 10);

            this.RaisePropertyChanged(nameof(MinATMillisecond));

            RandomizerEngine.Config.Save();
        }
    }

    public bool MinATEnabled
    {
        get => RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin is not null;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMin = value ? TimeInt32.Zero : null;

            this.RaisePropertyChanged(nameof(MinATEnabled));
            this.RaisePropertyChanged(nameof(MinATMinute));
            this.RaisePropertyChanged(nameof(MinATSecond));
            this.RaisePropertyChanged(nameof(MinATMillisecond));

            RandomizerEngine.Config.Save();
        }
    }

    public int MaxATMinute
    {
        get => RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Minutes;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax = new TimeInt32(0, 0, value,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Seconds,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Milliseconds);

            this.RaisePropertyChanged(nameof(MaxATMinute));

            RandomizerEngine.Config.Save();
        }
    }

    public int MaxATSecond
    {
        get => RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Seconds;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax = new TimeInt32(0, 0,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Minutes,
                value,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Milliseconds);

            this.RaisePropertyChanged(nameof(MaxATSecond));

            RandomizerEngine.Config.Save();
        }
    }

    public int MaxATMillisecond
    {
        get => RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Milliseconds / 10;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax = new TimeInt32(0, 0,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Minutes,
                RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax.GetValueOrDefault().Seconds,
                value * 10);

            this.RaisePropertyChanged(nameof(MaxATMillisecond));

            RandomizerEngine.Config.Save();
        }
    }

    public bool MaxATEnabled
    {
        get => RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax is not null;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.AuthorTimeMax = value ? TimeInt32.Zero : null;

            this.RaisePropertyChanged(nameof(MaxATEnabled));
            this.RaisePropertyChanged(nameof(MaxATMinute));
            this.RaisePropertyChanged(nameof(MaxATSecond));
            this.RaisePropertyChanged(nameof(MaxATMillisecond));

            RandomizerEngine.Config.Save();
        }
    }

    public DateTimeOffset? UploadedAfter
    {
        get => RandomizerEngine.Config.Rules.RequestRules.UploadedAfter?.ToDateTime(new());
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.UploadedAfter = value.HasValue ? DateOnly.FromDateTime(value.Value.DateTime) : null;

            this.RaisePropertyChanged(nameof(UploadedAfter));

            RandomizerEngine.Config.Save();
        }
    }

    public void UploadedAfterReset() => UploadedAfter = null;

    public DateTimeOffset? UploadedBefore
    {
        get => RandomizerEngine.Config.Rules.RequestRules.UploadedBefore?.ToDateTime(new());
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.UploadedBefore = value.HasValue ? DateOnly.FromDateTime(value.Value.DateTime) : null;

            this.RaisePropertyChanged(nameof(UploadedBefore));

            RandomizerEngine.Config.Save();
        }
    }

    public bool EqualEnvDistribution
    {
        get => RandomizerEngine.Config.Rules.RequestRules.EqualEnvironmentDistribution;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.EqualEnvironmentDistribution = value;

            this.RaisePropertyChanged(nameof(EqualEnvDistribution));

            RandomizerEngine.Config.Save();
        }
    }
    
    public bool EqualVehicleDistribution
    {
        get => RandomizerEngine.Config.Rules.RequestRules.EqualVehicleDistribution;
        set
        {
            RandomizerEngine.Config.Rules.RequestRules.EqualVehicleDistribution = value;

            this.RaisePropertyChanged(nameof(EqualVehicleDistribution));

            RandomizerEngine.Config.Save();
        }
    }

    public void UploadedBeforeReset() => UploadedBefore = null;
}
