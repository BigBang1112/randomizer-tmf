using Avalonia.Media;
using RandomizerTMF.Logic;
using RandomizerTMF.Views;
using ReactiveUI;

namespace RandomizerTMF.ViewModels;

public class ControlModuleWindowViewModel : WindowViewModelBase
{
    public string PrimaryButtonText => RandomizerEngine.HasSessionRunning ? "SKIP" : "START";
    public string SecondaryButtonText => RandomizerEngine.HasSessionRunning ? "END SESSION" : "CLOSE";
    public bool PrimaryButtonEnabled => !RandomizerEngine.HasSessionRunning || (RandomizerEngine.HasSessionRunning && CanSkip);

    public IBrush PrimaryButtonBackground => RandomizerEngine.HasSessionRunning ? new SolidColorBrush(new Color(255, 127, 96, 0)) : Brushes.DarkGreen;

    public bool CanSkip => RandomizerEngine.SkipTokenSource is not null;

    public ControlModuleWindowViewModel()
    {
        RandomizerEngine.MapStarted += RandomizerMapStarted;
        RandomizerEngine.MapEnded += RandomizerMapEnded;
    }

    private void RandomizerMapStarted()
    {
        this.RaisePropertyChanged(nameof(PrimaryButtonEnabled));
    }

    private void RandomizerMapEnded()
    {
        this.RaisePropertyChanged(nameof(PrimaryButtonEnabled));
    }

    public async Task PrimaryButtonClick()
    {
        if (RandomizerEngine.HasSessionRunning)
        {
            await RandomizerEngine.SkipMapAsync();
            return;
        }
        
        await RandomizerEngine.StartSessionAsync(new RandomizerRules());

        this.RaisePropertyChanged(nameof(PrimaryButtonText));
        this.RaisePropertyChanged(nameof(SecondaryButtonText));
        this.RaisePropertyChanged(nameof(PrimaryButtonBackground));
        
        this.RaisePropertyChanged(nameof(PrimaryButtonEnabled)); // HasSessionRunning changed from false to true here
    }
    
    public async Task SecondaryButtonClick()
    {
        // TODO: Prompt "Are you sure?"

        // If yes, freeze the button

        if (RandomizerEngine.HasSessionRunning)
        {
            await RandomizerEngine.EndSessionAsync();
        }

        OpenWindow<DashboardWindow, DashboardWindowViewModel>();

        foreach (var module in App.Modules)
        {
            module.Close();
        }

        App.Modules = Array.Empty<Avalonia.Controls.Window>();
    }
}
