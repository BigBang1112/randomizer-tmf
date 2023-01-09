namespace RandomizerTMF.ViewModels;

internal class WindowWithTopBarViewModelBase : WindowViewModelBase
{
    public TopBarViewModel TopBarViewModel { get; set; }
    
    public WindowWithTopBarViewModelBase(TopBarViewModel topBarViewModel)
	{
        TopBarViewModel = topBarViewModel;
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
