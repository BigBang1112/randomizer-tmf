using ReactiveUI;

namespace RandomizerTMF.ViewModels;

internal class MessageWindowViewModel : WindowWithTopBarViewModelBase
{
    private string content = "";

    public string Content
    {
        get => content;
        set => this.RaiseAndSetIfChanged(ref content, value);
    }

    public MessageWindowViewModel(TopBarViewModel topBarViewModel) : base(topBarViewModel)
    {
        TopBarViewModel.MinimizeButtonEnabled = false;
    }

    protected override void CloseClick()
    {
        Window.Close();
    }

    protected override void MinimizeClick()
    {

    }

    public void OkClick()
    {
        Window.Close();
    }
}
