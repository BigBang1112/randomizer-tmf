using ReactiveUI;

namespace RandomizerTMF.ViewModels;

internal class AboutWindowViewModel : WindowWithTopBarViewModelBase
{
    private readonly IUpdateDetector updateDetector;

    public string VersionText => $"version {Program.Version}";

    public string UpdateText => updateDetector.UpdateCheckResult ?? "Checking...";
    public bool IsNewUpdate => updateDetector.IsNewUpdate;

    public AboutWindowViewModel(TopBarViewModel topBarViewModel, IUpdateDetector updateDetector) : base(topBarViewModel)
    {
        this.updateDetector = updateDetector;
        
        TopBarViewModel.Title = "About Randomizer TMF";
        TopBarViewModel.MinimizeButtonEnabled = false;

        updateDetector.UpdateChecked += () =>
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
