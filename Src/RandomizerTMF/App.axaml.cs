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

        public static Window[] Modules { get; set; } = Array.Empty<Window>();

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

                    var viewModel = new DashboardWindowViewModel { Window = desktop.MainWindow };
                    desktop.MainWindow.DataContext = viewModel;

                    viewModel.OnInit();
                }
                else
                {
                    desktop.MainWindow = new MainWindow();

                    var viewModel = new MainWindowViewModel { Window = desktop.MainWindow };
                    desktop.MainWindow.DataContext = viewModel;

                    viewModel.OnInit();
                }

                MainWindow = desktop.MainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private bool IsValidGameDirectory(string? gameDirectory)
        {
            return !string.IsNullOrWhiteSpace(gameDirectory) && FilePathManager.UpdateGameDirectory(gameDirectory) is { NadeoIniException: null, TmForeverException: null };
        }
    }
}