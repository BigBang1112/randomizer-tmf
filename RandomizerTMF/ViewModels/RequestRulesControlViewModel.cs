using RandomizerTMF.Logic;
using ReactiveUI;

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

            RandomizerEngine.SaveConfig();
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

            RandomizerEngine.SaveConfig();
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

            RandomizerEngine.SaveConfig();
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

            RandomizerEngine.SaveConfig();
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

            RandomizerEngine.SaveConfig();
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

            RandomizerEngine.SaveConfig();
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

            RandomizerEngine.SaveConfig();
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

            RandomizerEngine.SaveConfig();
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

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsEnvironmentSnowChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsEnvironmentDesertChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsEnvironmentRallyChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsEnvironmentIslandChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsEnvironmentCoastChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsEnvironmentBayChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsEnvironmentStadiumChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsVehicleSnowChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsVehicleDesertChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsVehicleRallyChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsVehicleIslandChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsVehicleCoastChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsVehicleBayChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsVehicleStadiumChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsDifficultyBeginnerChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsDifficultyIntermediateChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsDifficultyExpertChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsDifficultyLunaticChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsRouteSingleChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsRouteMultiChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsRouteSymmetricChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsMoodSunriseChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsMoodDayChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsMoodSunsetChecked));

            RandomizerEngine.SaveConfig();
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

            this.RaisePropertyChanged(nameof(IsMoodNightChecked));

            RandomizerEngine.SaveConfig();
        }
    }
}
