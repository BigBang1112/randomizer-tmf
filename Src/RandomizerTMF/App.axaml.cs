using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RandomizerTMF.Logic;
using RandomizerTMF.ViewModels;
using RandomizerTMF.Views;
using System.Diagnostics;

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
            if (Program.ServiceProvider is null)
            {
                throw new UnreachableException("Service provider is null");
            }

            if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var config = Program.ServiceProvider.GetRequiredService<IRandomizerConfig>();
                var filePathManager = Program.ServiceProvider.GetRequiredService<IFilePathManager>();

                Window window;
                WindowViewModelBase viewModel;

                if (IsValidGameDirectory(filePathManager, config.GameDirectory))
                {
                    window = Program.ServiceProvider.GetRequiredService<DashboardWindow>();
                    viewModel = Program.ServiceProvider.GetRequiredService<DashboardWindowViewModel>();
                }
                else
                {
                    window = Program.ServiceProvider.GetRequiredService<MainWindow>();
                    viewModel = Program.ServiceProvider.GetRequiredService<MainWindowViewModel>();
                }

                desktop.MainWindow = window;
                viewModel.Window = window;
                window.DataContext = viewModel;

                viewModel.OnInit();

                MainWindow = desktop.MainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private bool IsValidGameDirectory(IFilePathManager filePathManager, string? gameDirectory)
        {
            return !string.IsNullOrWhiteSpace(gameDirectory) && filePathManager.UpdateGameDirectory(gameDirectory) is { NadeoIniException: null, TmForeverException: null };
        }
    }
}