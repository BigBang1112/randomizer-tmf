using Avalonia.Controls;

namespace RandomizerTMF.ViewModels;

public class WindowViewModelBase : ViewModelBase
{
    public Window Window { get; init; } = default!;

    public void SwitchWindowTo<TWindow, TViewModel>()
        where TWindow : Window, new()
        where TViewModel : WindowViewModelBase, new()
    {
        _ = OpenWindow<TWindow, TViewModel>();
        Window.Close();
    }

    public static Window OpenWindow<TWindow, TViewModel>()
        where TWindow : Window, new()
        where TViewModel : WindowViewModelBase, new()
    {
        var window = new TWindow();
        var viewModel = new TViewModel() { Window = window };
        viewModel.OnInit();
        window.DataContext = viewModel;
        window.Show();
        return window;
    }

    protected internal virtual void OnInit()
    {
        
    }
}
