using Avalonia.Controls;
using Avalonia.Media;
using RandomizerTMF.Logic;
using RandomizerTMF.Views;
using ReactiveUI;

namespace RandomizerTMF.ViewModels;

public class MainWindowViewModel : WindowWithTopBarViewModelBase
{
    private string? gameDirectory;
    private string? userDirectory;
    private GameDirInspectResult? nadeoIni;

    public string? GameDirectory
    {
        get => gameDirectory;
        set => this.RaiseAndSetIfChanged(ref gameDirectory, value);
    }

    public string? UserDirectory
    {
        get => userDirectory;
        set => this.RaiseAndSetIfChanged(ref userDirectory, value);
    }

    public GameDirInspectResult? NadeoIni
    {
        get => nadeoIni;
        set
        {
            this.RaiseAndSetIfChanged(ref nadeoIni, value);

            this.RaisePropertyChanged(nameof(OpacityNadeoIni));
            this.RaisePropertyChanged(nameof(OpacityTmForever));
            this.RaisePropertyChanged(nameof(OpacityTmUnlimiter));

            this.RaisePropertyChanged(nameof(ColorNadeoIni));
            this.RaisePropertyChanged(nameof(ColorTmForever));
            this.RaisePropertyChanged(nameof(ColorTmUnlimiter));

            this.RaisePropertyChanged(nameof(TooltipNadeoIni));
            this.RaisePropertyChanged(nameof(TooltipTmForever));
            this.RaisePropertyChanged(nameof(TooltipTmUnlimiter));

            this.RaisePropertyChanged(nameof(IsSaveAndProceedEnabled));
        }
    }

    public bool ValidNadeoIni => NadeoIni is not null && NadeoIni.NadeoIniException is null;
    public bool ValidTmForever => NadeoIni is not null && NadeoIni.TmForeverException is null;
    public bool ValidTmUnlimiter => NadeoIni is not null && NadeoIni.TmUnlimiterException is null;

    public double OpacityNadeoIni => NadeoIni is null ? 0.25 : 0.9;
    public double OpacityTmForever => NadeoIni is null ? 0.25 : 0.9;
    public double OpacityTmUnlimiter => NadeoIni is null || NadeoIni.TmUnlimiterException is FileNotFoundException ? 0.25 : 0.9;

    public IBrush ColorNadeoIni => NadeoIni is null ? Brushes.Gray : (NadeoIni.NadeoIniException is null ? Brushes.DarkGreen : Brushes.DarkRed);
    public IBrush ColorTmForever => NadeoIni is null ? Brushes.Gray : (NadeoIni.TmForeverException is null ? Brushes.DarkGreen : Brushes.DarkRed);
    public IBrush ColorTmUnlimiter => NadeoIni is null ? Brushes.Gray : (NadeoIni.TmUnlimiterException is null ? Brushes.DarkGreen : (NadeoIni.TmUnlimiterException is FileNotFoundException ? Brushes.Gray : Brushes.DarkRed));

    public string? TooltipNadeoIni => NadeoIni?.NadeoIniException?.Message;
    public string? TooltipTmForever => NadeoIni?.TmForeverException?.Message;
    public string? TooltipTmUnlimiter => NadeoIni?.TmUnlimiterException?.Message;

    public bool IsSaveAndProceedEnabled => NadeoIni is not null && NadeoIni.NadeoIniException is null && NadeoIni.TmForeverException is null && NadeoIni.TmUnlimiterException is null or FileNotFoundException;

    public MainWindowViewModel()
    {
        if (!string.IsNullOrWhiteSpace(RandomizerEngine.Config.GameDirectory))
        {
            GameDirectory = RandomizerEngine.Config.GameDirectory;
            NadeoIni = FilePathManager.UpdateGameDirectory(RandomizerEngine.Config.GameDirectory);
            UserDirectory = FilePathManager.UserDataDirectoryPath;
        }
    }

    protected override void CloseClick()
    {
        RandomizerEngine.Exit();
    }

    protected override void MinimizeClick()
    {
        Window.WindowState = WindowState.Minimized;
    }

    public async Task SelectGameDirectoryClick()
    {
        var ofd = new OpenFolderDialog();
        var dir = await ofd.ShowAsync(App.MainWindow);

        if (dir is null)
        {
            return;
        }

        GameDirectory = dir;

        NadeoIni = FilePathManager.UpdateGameDirectory(dir);

        UserDirectory = NadeoIni.NadeoIniException is null ? FilePathManager.UserDataDirectoryPath : null;
    }

    public void SaveAndProceedClick()
    {
        RandomizerEngine.Config.Save();

        SwitchWindowTo<DashboardWindow, DashboardWindowViewModel>();
    }
}