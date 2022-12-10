using RandomizerTMF.Models;
using System.Diagnostics;

namespace RandomizerTMF.ViewModels;

public class SessionMapViewModel : WindowWithTopBarViewModelBase
{
    public PlayedMapModel Model { get; }

    /// <summary>
    /// Should be used only for previews
    /// </summary>
    public SessionMapViewModel() : this(null!)
    {

    }

	public SessionMapViewModel(PlayedMapModel model)
    {
        Model = model;

        TopBarViewModel.Title = "Session Map";
        TopBarViewModel.MinimizeButtonEnabled = false;
    }

    public void VisitOnTmxClick()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Model.Map.TmxLink,
            UseShellExecute = true
        });
    }
}
