using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic.Services;
using System.IO.Abstractions;

namespace RandomizerTMF.Logic;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRandomizerEngine(this IServiceCollection services)
    {
        services.AddSingleton<ILogger, LoggerToFile>(provider =>
        {
            var logWriter = new StreamWriter(Constants.RandomizerTmfLog, append: true)
            {
                AutoFlush = true
            };

            return new LoggerToFile(logWriter);
        });

        services.AddSingleton<HttpClient>(provider =>
        {
            var socketHandler = new SocketsHttpHandler()
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(1),
            };

            var http = new HttpClient(socketHandler);
            http.DefaultRequestHeaders.UserAgent.TryParseAdd($"Randomizer TMF {RandomizerEngine.Version}");
            return http;
        });

        services.AddSingleton<IFileSystem, FileSystem>();

        services.AddSingleton<IRandomizerConfig, RandomizerConfig>(provider =>
        {
            return RandomizerConfig.GetOrCreate(provider.GetRequiredService<ILogger>(), provider.GetRequiredService<IFileSystem>());
        });

        services.AddSingleton<IRandomizerEngine, RandomizerEngine>();
        services.AddSingleton<IRandomizerEvents, RandomizerEvents>();
        services.AddSingleton<IValidator, Validator>();
        services.AddSingleton<IMapDownloader, MapDownloader>();
        services.AddSingleton<ITMForever, TMForever>();
        services.AddSingleton<IFilePathManager, FilePathManager>();
        services.AddSingleton<IAutosaveScanner, AutosaveScanner>();
        services.AddSingleton<IAdditionalData, AdditionalData>();
        services.AddSingleton<IDiscordRichPresence, DiscordRichPresence>();
        services.AddSingleton<DiscordRpcLogger>();
        services.AddSingleton<IRandomGenerator, RandomGenerator>();

        return services;
    }
}
