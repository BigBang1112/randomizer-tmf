using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic;
using RandomizerTMF.ViewModels;
using RandomizerTMF.Views;

namespace RandomizerTMF;

internal class Program
{
    internal static string? Version { get; } = typeof(Program).Assembly.GetName().Version?.ToString(3);

    internal static ServiceProvider? ServiceProvider { get; private set; }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();
        
        services.AddRandomizerEngine();
        
        services.AddSingleton<IUpdateDetector, UpdateDetector>();

        services.AddTransient<DashboardWindow>();
        services.AddTransient<DashboardWindowViewModel>();
        
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();
        
        services.AddTransient<ControlModuleWindow>();
        services.AddTransient<ControlModuleWindowViewModel>();

        services.AddTransient<StatusModuleWindow>();
        services.AddTransient<StatusModuleWindowViewModel>();

        services.AddTransient<ProgressModuleWindow>();
        services.AddTransient<ProgressModuleWindowViewModel>();

        services.AddTransient<HistoryModuleWindow>();
        services.AddTransient<HistoryModuleWindowViewModel>();

        services.AddTransient<TopBarViewModel>();

        var provider = services.BuildServiceProvider();

        var logger = provider.GetRequiredService<ILogger>();
        var updateDetector = provider.GetRequiredService<IUpdateDetector>();

        ServiceProvider = provider;

        try
        {
            updateDetector.RequestNewUpdateAsync();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Global error");

#if DEBUG
            throw;
#endif
        }
        finally
        {
            // RandomizerEngine.FlushLog();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
