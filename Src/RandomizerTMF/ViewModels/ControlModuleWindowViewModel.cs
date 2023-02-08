using Avalonia.Media;
using RandomizerTMF.Logic;
using RandomizerTMF.Logic.Services;
using RandomizerTMF.Views;
using ReactiveUI;

namespace RandomizerTMF.ViewModels;

internal class ControlModuleWindowViewModel : ModuleWindowViewModelBase
{
    private readonly IRandomizerEngine engine;
    private readonly IRandomizerEvents events;

    public string PrimaryButtonText => engine.HasSessionRunning ? "SKIP" : "START";
    public string SecondaryButtonText => engine.HasSessionRunning ? "END SESSION" : "CLOSE";
    public bool PrimaryButtonEnabled => !engine.HasSessionRunning || (engine.HasSessionRunning && CanSkip);
    public bool ReloadMapButtonEnabled => engine.HasSessionRunning && CanSkip; // CanSkip is mostly a hack

    public IBrush PrimaryButtonBackground => engine.HasSessionRunning ? new SolidColorBrush(new Color(255, 127, 96, 0)) : Brushes.DarkGreen;

    public bool CanSkip => engine.CurrentSession?.SkipTokenSource is not null;

    public ControlModuleWindowViewModel(IRandomizerEngine engine, IRandomizerEvents events, IRandomizerConfig config) : base(config)
    {
        this.engine = engine;
        this.events = events;
        
        events.MapStarted += RandomizerMapStarted;
        events.MapEnded += RandomizerMapEnded;
    }

    private void RandomizerMapStarted(SessionMap map)
    {
        this.RaisePropertyChanged(nameof(PrimaryButtonEnabled));
        this.RaisePropertyChanged(nameof(ReloadMapButtonEnabled));
    }

    private void RandomizerMapEnded()
    {
        this.RaisePropertyChanged(nameof(PrimaryButtonEnabled));
        this.RaisePropertyChanged(nameof(ReloadMapButtonEnabled));
    }

    public async Task PrimaryButtonClick()
    {
        if (engine.CurrentSession is not null)
        {
            await engine.CurrentSession.SkipMapAsync();
            return;
        }
        
        engine.StartSession();

        this.RaisePropertyChanged(nameof(PrimaryButtonText));
        this.RaisePropertyChanged(nameof(SecondaryButtonText));
        this.RaisePropertyChanged(nameof(PrimaryButtonBackground));
        
        this.RaisePropertyChanged(nameof(PrimaryButtonEnabled)); // HasSessionRunning changed from false to true here
        this.RaisePropertyChanged(nameof(ReloadMapButtonEnabled));
    }

    public void ReloadMapButtonClick()
    {
        engine.CurrentSession?.ReloadMap();
    }

    public async Task SecondaryButtonClick()
    {
        // TODO: Prompt "Are you sure?"

        // If yes, freeze the button

        if (engine.HasSessionRunning)
        {
            await engine.EndSessionAsync();
        }

        OpenWindow<DashboardWindow, DashboardWindowViewModel>();

        foreach (var module in App.Modules)
        {
            module.Close();
        }

        App.Modules = Array.Empty<Avalonia.Controls.Window>();
    }
}
