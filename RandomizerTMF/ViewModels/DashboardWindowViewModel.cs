using Avalonia.Controls;
using RandomizerTMF.Logic;

namespace RandomizerTMF.ViewModels;

public class DashboardWindowViewModel : ViewModelBase
{
    public required Window Window { get; init; }

    public TopBarViewModel TopBarViewModel { get; set; }
    
    public DashboardWindowViewModel()
    {
        TopBarViewModel = new();
        TopBarViewModel.CloseClick += CloseClick;
    }

    public void CloseClick()
    {
        RandomizerEngine.Exit();
    }
}
