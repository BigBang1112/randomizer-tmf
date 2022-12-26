using GBX.NET.Engines.Game;
using RandomizerTMF.Logic;
using RandomizerTMF.Models;
using TmEssentials;

namespace RandomizerTMF.ViewModels;

internal class AutosaveDetailsWindowViewModel : WindowWithTopBarViewModelBase
{
    private readonly ITMForever? game;

    public AutosaveModel AutosaveModel { get; }
    public string? AutosaveFilePath { get; }

    public string MapAuthorTime => AutosaveModel.Autosave.MapMode switch
    {
        CGameCtnChallenge.PlayMode.Stunts or CGameCtnChallenge.PlayMode.Platform => AutosaveModel.Autosave.MapAuthorScore.ToString() + $" ({AutosaveModel.Autosave.MapAuthorTime.ToString(useHundredths: true)})",
        _ => AutosaveModel.Autosave.MapAuthorTime.ToString(useHundredths: true)
    };

    public string MapGoldTime => AutosaveModel.Autosave.MapMode switch
    {
        CGameCtnChallenge.PlayMode.Stunts or CGameCtnChallenge.PlayMode.Platform => AutosaveModel.Autosave.MapGoldTime.TotalMilliseconds.ToString(),
        _ => AutosaveModel.Autosave.MapGoldTime.ToString(useHundredths: true)
    };
    
    public string MapSilverTime => AutosaveModel.Autosave.MapMode switch
    {
        CGameCtnChallenge.PlayMode.Stunts or CGameCtnChallenge.PlayMode.Platform => AutosaveModel.Autosave.MapSilverTime.TotalMilliseconds.ToString(),
        _ => AutosaveModel.Autosave.MapSilverTime.ToString(useHundredths: true)
    };

    public string MapBronzeTime => AutosaveModel.Autosave.MapMode switch
    {
        CGameCtnChallenge.PlayMode.Stunts or CGameCtnChallenge.PlayMode.Platform => AutosaveModel.Autosave.MapBronzeTime.TotalMilliseconds.ToString(),
        _ => AutosaveModel.Autosave.MapBronzeTime.ToString(useHundredths: true)
    };
    
    public string PersonalBest => AutosaveModel.Autosave.MapMode switch
    {
        CGameCtnChallenge.PlayMode.Stunts => (AutosaveModel.Autosave.Score.ToString() ?? "N/A") + $" ({AutosaveModel.Autosave.Time.ToString(useHundredths: true)})",
        CGameCtnChallenge.PlayMode.Platform => (AutosaveModel.Autosave.Respawns.ToString() ?? "N/A") + $" ({AutosaveModel.Autosave.Time.ToString(useHundredths: true)})",
        _ => AutosaveModel.Autosave.Time.ToString(useHundredths: true)
    };

    public string MapName => AutosaveModel.Text.Trim();

    /// <summary>
    /// Example view model, should not be normally used.
    /// </summary>
    public AutosaveDetailsWindowViewModel() : base(new TopBarViewModel(null))
    {
        AutosaveModel = new(mapUid: "", new(
            Time: TimeInt32.Zero,
            Score: 0,
            Respawns: 0,
            MapName: "Tonic map",
            MapEnvironment: "Stadium",
            MapCar: "StadiumCar",
            MapBronzeTime: TimeInt32.Zero,
            MapSilverTime: TimeInt32.Zero,
            MapGoldTime: TimeInt32.Zero,
            MapAuthorTime: TimeInt32.Zero,
            MapAuthorScore: 0,
            MapMode: CGameCtnChallenge.PlayMode.Race));

        TopBarViewModel.Title = AutosaveModel.Text;
        TopBarViewModel.MinimizeButtonEnabled = false;
    }

    public AutosaveDetailsWindowViewModel(TopBarViewModel topBarViewModel,
                                          ITMForever game,
                                          AutosaveModel autosaveModel,
                                          string autosaveFilePath) : base(topBarViewModel)
    {
        this.game = game;
        
        AutosaveModel = autosaveModel;
        AutosaveFilePath = autosaveFilePath;

        TopBarViewModel.Title = AutosaveModel.Text;
        TopBarViewModel.MinimizeButtonEnabled = false;
    }

    protected internal override void OnInit()
    {
        base.OnInit();
        
        Window.Title = $"Autosave: {TopBarViewModel.Title}";
    }

    protected override void CloseClick()
    {
        Window.Close();
    }

    public void OpenAutosaveIngame()
    {
        if (AutosaveFilePath is null)
        {
            return;
        }

        game?.OpenAutosave(AutosaveFilePath);
    }
}
