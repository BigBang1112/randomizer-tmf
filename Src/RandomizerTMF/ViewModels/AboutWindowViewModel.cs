using ReactiveUI;
using System.Diagnostics;

namespace RandomizerTMF.ViewModels;

public class AboutWindowViewModel : WindowWithTopBarViewModelBase
{
    public string VersionText => $"version {Program.Version}";

    public string UpdateText => UpdateDetector.UpdateCheckResult ?? "Checking...";
    public bool IsNewUpdate => UpdateDetector.IsNewUpdate;

    public AboutWindowViewModel()
    {
        TopBarViewModel.Title = "About Randomizer TMF";
        TopBarViewModel.MinimizeButtonEnabled = false;

        UpdateDetector.UpdateChecked += () =>
        {
            this.RaisePropertyChanged(nameof(UpdateText));
            this.RaisePropertyChanged(nameof(IsNewUpdate));
        };
    }

    protected override void CloseClick()
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
        ProcessUtils.OpenUrl(site);
    }
}
