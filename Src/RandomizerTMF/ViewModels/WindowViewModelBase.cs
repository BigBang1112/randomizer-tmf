using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using RandomizerTMF.Views;
using System.Diagnostics;

namespace RandomizerTMF.ViewModels;

internal class WindowViewModelBase : ViewModelBase
{
    public Window Window { get; internal set; } = default!;

    public void SwitchWindowTo<TWindow, TViewModel>()
        where TWindow : Window
        where TViewModel : WindowViewModelBase
    {
        _ = OpenWindow<TWindow, TViewModel>();
        Window.Close();
    }

    public static TWindow OpenWindow<TWindow, TViewModel>()
        where TWindow : Window
        where TViewModel : WindowViewModelBase
    {
        if (Program.ServiceProvider is null)
        {
            throw new UnreachableException("ServiceProvider is null");
        }

        var window = Program.ServiceProvider.GetRequiredService<TWindow>();
        var viewModel = Program.ServiceProvider.GetRequiredService<TViewModel>();
        viewModel.Window = window;
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
        if (Program.ServiceProvider is null)
        {
            throw new UnreachableException("ServiceProvider is null");
        }
        
        var topBarViewModel = Program.ServiceProvider.GetRequiredService<TopBarViewModel>();
        
        var window = new MessageWindow() { Title = title };
        var viewModel = new MessageWindowViewModel(topBarViewModel) { Window = window, Content = content };
        viewModel.OnInit();
        window.DataContext = viewModel;
        window.ShowDialog(Window); // The parent window
        return window;
    }

    protected internal virtual void OnInit()
    {
        
    }
}
