using Avalonia.Media;
using RandomizerTMF.Logic;
using RandomizerTMF.Views;
using ReactiveUI;

namespace RandomizerTMF.ViewModels;

public class ControlModuleWindowViewModel : WindowViewModelBase
{
    public string PrimaryButtonText => RandomizerEngine.HasSessionRunning ? "SKIP" : "START";
    public string SecondaryButtonText => RandomizerEngine.HasSessionRunning ? "END SESSION" : "CLOSE";

    public IBrush PrimaryButtonBackground => RandomizerEngine.HasSessionRunning ? new SolidColorBrush(new Color(255, 127, 96, 0)) : Brushes.DarkGreen;

    public async Task PrimaryButtonClick()
    {
        if (RandomizerEngine.HasSessionRunning)
        {
            await RandomizerEngine.SkipMapAsync();
            return;
        }
        
        await RandomizerEngine.StartSessionAsync();

        this.RaisePropertyChanged(nameof(PrimaryButtonText));
        this.RaisePropertyChanged(nameof(SecondaryButtonText));
        this.RaisePropertyChanged(nameof(PrimaryButtonBackground));
    }
    
    public async Task SecondaryButtonClick()
    {
        // TODO: Prompt "Are you sure?"

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
