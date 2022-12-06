using ReactiveUI;

namespace RandomizerTMF.ViewModels;

public class MessageWindowViewModel : WindowViewModelBase
{
    private string content = "";

    public TopBarViewModel TopBarViewModel { get; set; }

    public string Content
    {
        get => content;
        set => this.RaiseAndSetIfChanged(ref content, value);
    }

    public MessageWindowViewModel()
    {
        TopBarViewModel = new() { MinimizeButtonEnabled = false };
        TopBarViewModel.CloseClick += CloseClick;
        TopBarViewModel.MinimizeClick += MinimizeClick;
    }

    protected internal override void OnInit()
    {
        TopBarViewModel.Title = Window.Title;
    }

    private void CloseClick()
    {
        Window.Close();
    }

    private void MinimizeClick()
    {

    }

    public void OkClick()
    {
        Window.Close();
    }
}
