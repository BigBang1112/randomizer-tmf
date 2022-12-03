using Avalonia.Controls;
using RandomizerTMF.Views;

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

    public static TWindow OpenWindow<TWindow, TViewModel>()
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

    public TWindow OpenDialog<TWindow, TViewModel>()
        where TWindow : Window, new()
        where TViewModel : WindowViewModelBase, new()
    {
        var window = new TWindow();
        var viewModel = new TViewModel() { Window = window };
        viewModel.OnInit();
        window.DataContext = viewModel;
        window.ShowDialog(Window); // The parent window
        return window;
    }

    public TWindow OpenDialog<TWindow>(Func<TWindow, WindowViewModelBase> viewModelFunc)
        where TWindow : Window, new()
    {
        var window = new TWindow();
        var viewModel = viewModelFunc(window);
        viewModel.OnInit();
        window.DataContext = viewModel;
        window.ShowDialog(Window); // The parent window
        return window;
    }

    public MessageWindow OpenMessageBox(string title, string content)
    {
        var window = new MessageWindow() { Title = title };
        var viewModel = new MessageWindowViewModel() { Window = window, Content = content };
        viewModel.OnInit();
        window.DataContext = viewModel;
        window.ShowDialog(Window); // The parent window
        return window;
    }

    protected internal virtual void OnInit()
    {
        
    }
}
