using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RandomizerTMF.Logic;
using RandomizerTMF.ViewModels;
using RandomizerTMF.Views;

namespace RandomizerTMF
{
    public partial class App : Application
    {
        public static Window MainWindow { get; private set; } = default!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (IsValidGameDirectory(RandomizerEngine.Config.GameDirectory))
                {
                    desktop.MainWindow = new DashboardWindow();
                    desktop.MainWindow.DataContext = new DashboardWindowViewModel { Window = desktop.MainWindow };
                }
                else
                {
                    desktop.MainWindow = new MainWindow();
                    desktop.MainWindow.DataContext = new MainWindowViewModel { Window = desktop.MainWindow };
                }

                MainWindow = desktop.MainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private bool IsValidGameDirectory(string? gameDirectory)
        {
            return !string.IsNullOrWhiteSpace(gameDirectory) && RandomizerEngine.UpdateGameDirectory(gameDirectory) is { NadeoIniException: null, TmForeverException: null };
        }
    }
}