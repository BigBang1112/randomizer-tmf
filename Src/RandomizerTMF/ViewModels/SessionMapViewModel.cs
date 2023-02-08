using RandomizerTMF.Models;

namespace RandomizerTMF.ViewModels;

internal class SessionMapViewModel : WindowWithTopBarViewModelBase
{
    public PlayedMapModel Model { get; }

    /// <summary>
    /// Should be used only for previews
    /// </summary>
    public SessionMapViewModel() : this(new(null), null!)
    {

    }

	public SessionMapViewModel(TopBarViewModel topBarViewModel, PlayedMapModel model) : base(topBarViewModel)
    {
        Model = model;

        TopBarViewModel.Title = "Session Map";
        TopBarViewModel.MinimizeButtonEnabled = false;
    }

    public void VisitOnTmxClick()
    {
        ProcessUtils.OpenUrl(Model.Map.TmxLink);
    }
}
