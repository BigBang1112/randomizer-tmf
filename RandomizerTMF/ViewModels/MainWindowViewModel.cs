using Avalonia.Controls;
using RandomizerTMF.Logic;
using ReactiveUI;

namespace RandomizerTMF.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string? gameDirectory;
    private string? nadeoIni;
    private string? userDirectory;

    public TopBarViewModel TopBarViewModel { get; set; }

    public string? GameDirectory
    {
        get => gameDirectory;
        set => this.RaiseAndSetIfChanged(ref gameDirectory, value);
    }

    public string? NadeoIni
    {
        get => nadeoIni;
        set => this.RaiseAndSetIfChanged(ref nadeoIni, value);
    }
    
    public string? UserDirectory
    {
        get => userDirectory;
        set => this.RaiseAndSetIfChanged(ref userDirectory, value);
    }

    public MainWindowViewModel()
    {
        TopBarViewModel = new();
        TopBarViewModel.CloseClick += CloseClick;
    }

    public void CloseClick()
    {
        RandomizerEngine.Exit();
    }

    public async Task SelectGameDirectoryClick()
    {
        var ofd = new OpenFolderDialog();
        var dir = await ofd.ShowAsync(App.MainWindow);
        
        if (dir is not null)
        {
            GameDirectory = dir;
        }
    }
}