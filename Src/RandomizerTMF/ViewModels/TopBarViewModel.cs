using Avalonia.Controls;
using RandomizerTMF.Logic;
using RandomizerTMF.Views;
using ReactiveUI;
using System.Diagnostics;

namespace RandomizerTMF.ViewModels;

public class TopBarViewModel : ViewModelBase
{
    private string? title = Constants.Title;

    public string? Title
    {
        get => title;
        set => this.RaiseAndSetIfChanged(ref title, value);
    }

    public static string? Version { get; } = typeof(Program).Assembly.GetName().Version?.ToString(3);

    public Window? WindowOwner { get; set; }

    public event Action? CloseClick;
    public event Action? MinimizeClick;

    public TopBarViewModel()
    {

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
