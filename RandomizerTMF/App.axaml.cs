using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RandomizerTMF.ViewModels;
using RandomizerTMF.Views;

namespace RandomizerTMF
{
    public partial class App : Application
    {
        public static MainWindow MainWindow { get; private set; } = default!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
                desktop.MainWindow.DataContext = new MainWindowViewModel { Window = desktop.MainWindow };

                MainWindow = (MainWindow)desktop.MainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}