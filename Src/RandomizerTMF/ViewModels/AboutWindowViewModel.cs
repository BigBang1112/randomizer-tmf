using RandomizerTMF.Models;

namespace RandomizerTMF.ViewModels;

public class AboutWindowViewModel : WindowViewModelBase
{
    public TopBarViewModel TopBarViewModel { get; set; }

	public AboutWindowViewModel()
    {
        TopBarViewModel = new() { Title = "About Randomizer TMF", MinimizeButtonEnabled = false };
        TopBarViewModel.CloseClick += CloseClick;
        TopBarViewModel.MinimizeClick += MinimizeClick;
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
}
