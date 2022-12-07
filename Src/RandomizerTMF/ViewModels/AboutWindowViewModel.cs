using ReactiveUI;
using System.Diagnostics;

namespace RandomizerTMF.ViewModels;

public class AboutWindowViewModel : WindowViewModelBase
{
    public TopBarViewModel TopBarViewModel { get; set; }

    public string VersionText => $"version {Program.Version}";

    public string UpdateText => UpdateDetector.UpdateCheckResult ?? "Checking...";
    public bool IsNewUpdate => UpdateDetector.IsNewUpdate;

    public AboutWindowViewModel()
    {
        TopBarViewModel = new() { Title = "About Randomizer TMF", MinimizeButtonEnabled = false };
        TopBarViewModel.CloseClick += CloseClick;
        TopBarViewModel.MinimizeClick += MinimizeClick;

        UpdateDetector.UpdateChecked += () =>
        {
            this.RaisePropertyChanged(nameof(UpdateText));
            this.RaisePropertyChanged(nameof(IsNewUpdate));
        };
    }
    
    protected internal override void OnInit()
    {
        TopBarViewModel.WindowOwner = Window;
    }

    private void MinimizeClick()
    {

    }

    private void CloseClick()
    {
        Window.Close();
    }

    public void ProjectClick()
    {
        OpenWeb("https://bigbang1112.cz/");
    }

    public void GitHubClick()
    {
        OpenWeb("https://github.com/BigBang1112/randomizer-tmf");
    }

    public void UpdateClick()
    {
        OpenWeb("https://github.com/BigBang1112/randomizer-tmf/releases");
    }

    private void OpenWeb(string site)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = site,
            UseShellExecute = true
        });
    }
}
