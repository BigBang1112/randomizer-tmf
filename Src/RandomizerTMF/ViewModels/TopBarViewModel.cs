using Avalonia.Controls;
using RandomizerTMF.Logic;
using RandomizerTMF.Views;
using ReactiveUI;
using System.Diagnostics;

namespace RandomizerTMF.ViewModels;

public class TopBarViewModel : ViewModelBase
{
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

    public bool IsNewUpdate => UpdateDetector.IsNewUpdate;

    public static string? Version => Program.Version;
    public static string? VersionTooltip => $"About Randomizer TMF {Program.Version}";


    public Window? WindowOwner { get; set; }

    public event Action? CloseClick;
    public event Action? MinimizeClick;

    public TopBarViewModel()
    {
        UpdateDetector.UpdateChecked += () => this.RaisePropertyChanged(nameof(IsNewUpdate));
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
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://paypal.me/bigbang1112",
            UseShellExecute = true
        });
    }

    public void VersionClick()
    {
        var window = new AboutWindow();
        var viewModel = new AboutWindowViewModel() { Window = window };
        viewModel.OnInit();
        window.DataContext = viewModel;
        window.ShowDialog(WindowOwner ?? App.MainWindow); // The parent window
    }
}
