using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic;

namespace RandomizerTMF;

internal class Program
{
    internal static string? Version { get; } = typeof(Program).Assembly.GetName().Version?.ToString(3);

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            UpdateDetector.RequestNewUpdateAsync();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            RandomizerEngine.Logger.LogError(ex, "Global error");
        }

        RandomizerEngine.FlushLog();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
