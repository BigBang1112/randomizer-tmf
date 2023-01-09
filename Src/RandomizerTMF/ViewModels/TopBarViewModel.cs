using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using RandomizerTMF.Logic;
using RandomizerTMF.Views;
using ReactiveUI;
using System.Diagnostics;

namespace RandomizerTMF.ViewModels;

internal class TopBarViewModel : ViewModelBase
{
    private readonly IUpdateDetector? updateDetector;
    
    private string? title = Constants.Title;
    private bool minimizeButtonEnabled = true;

    public string? Title
    {
        get => title;
        set => this.RaiseAndSetIfChanged(ref title, value);
    }

    public bool MinimizeButtonEnabled
    {
        get => minimizeButtonEnabled;
        set => this.RaiseAndSetIfChanged(ref minimizeButtonEnabled, value);
    }

    public bool IsNewUpdate => updateDetector?.IsNewUpdate ?? false;

    public static string? Version => Program.Version;
    public static string? VersionTooltip => $"About Randomizer TMF {Program.Version}";


    public Window? WindowOwner { get; set; }

    public event Action? CloseClick;
    public event Action? MinimizeClick;

    public TopBarViewModel(IUpdateDetector? updateDetector = null)
    {
        this.updateDetector = updateDetector;

        if (updateDetector is not null)
        {
            updateDetector.UpdateChecked += () => this.RaisePropertyChanged(nameof(IsNewUpdate));
        }
    }

    public void OnCloseClick()
    {
        CloseClick?.Invoke();
    }

    public void OnMinimizeClick()
    {
        MinimizeClick?.Invoke();
    }
    
    public void DonateClick()
    {
        ProcessUtils.OpenUrl("https://paypal.me/bigbang1112");
    }

    public void VersionClick()
    {
        if (Program.ServiceProvider is null)
        {
            throw new UnreachableException("ServiceProvider is null");
        }

        var topBarViewModel = Program.ServiceProvider.GetRequiredService<TopBarViewModel>();
        var updateDetector = Program.ServiceProvider.GetRequiredService<IUpdateDetector>();

        var window = new AboutWindow();
        var viewModel = new AboutWindowViewModel(topBarViewModel, updateDetector) { Window = window };
        viewModel.OnInit();
        window.DataContext = viewModel;
        window.ShowDialog(WindowOwner ?? App.MainWindow); // The parent window
    }
}
