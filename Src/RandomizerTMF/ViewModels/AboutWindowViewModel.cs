using ReactiveUI;

namespace RandomizerTMF.ViewModels;

internal class AboutWindowViewModel : WindowWithTopBarViewModelBase
{
    private readonly IUpdateDetector updateDetector;

    public static string VersionText => $"version {Program.Version}";

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

    public static void ProjectClick()
    {
        OpenWeb("https://bigbang1112.cz/");
    }

    public static void GitHubClick()
    {
        OpenWeb("https://github.com/BigBang1112/randomizer-tmf");
    }

    public static void UpdateClick()
    {
        OpenWeb("https://github.com/BigBang1112/randomizer-tmf/releases");
    }

    private static void OpenWeb(string site)
    {
        ProcessUtils.OpenUrl(site);
    }
}
