namespace RandomizerTMF.ViewModels;

public class WindowWithTopBarViewModelBase : WindowViewModelBase
{
    public TopBarViewModel TopBarViewModel { get; set; }
    
    public WindowWithTopBarViewModelBase()
	{
        TopBarViewModel = new() { MinimizeButtonEnabled = false };
        TopBarViewModel.CloseClick += CloseClick;
        TopBarViewModel.MinimizeClick += MinimizeClick;
    }

    protected internal override void OnInit()
    {
        base.OnInit();
        
        TopBarViewModel.Title = Window.Title;
        TopBarViewModel.WindowOwner = Window;
    }

    protected virtual void MinimizeClick()
    {

    }

    protected virtual void CloseClick()
    {
        Window.Close();
    }
}
