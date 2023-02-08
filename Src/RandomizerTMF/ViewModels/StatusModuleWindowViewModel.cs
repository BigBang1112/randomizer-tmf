using RandomizerTMF.Logic;
using RandomizerTMF.Logic.Services;
using ReactiveUI;

namespace RandomizerTMF.ViewModels;

internal class StatusModuleWindowViewModel : ModuleWindowViewModelBase
{
    private Task? updateTimeTask;
    private CancellationTokenSource? updateTimeCancellationTokenSource;

    private string statusText = "Idle";
    private float timeOpacity = 1f;
    private readonly IRandomizerEngine engine;
    private readonly IRandomizerEvents events;
    private readonly IRandomizerConfig config;

    public TimeSpan Time { get; private set; }
    public string TimeText => Time.ToString("h':'mm':'ss");

    public string StatusText
    {
        get => statusText;
        private set => this.RaiseAndSetIfChanged(ref statusText, value);
    }

    public float TimeOpacity
    {
        get => timeOpacity;
        private set => this.RaiseAndSetIfChanged(ref timeOpacity, value);
    }

    public StatusModuleWindowViewModel(IRandomizerEngine engine, IRandomizerEvents events, IRandomizerConfig config) : base(config)
    {
        this.engine = engine;
        this.events = events;
        this.config = config;
        
        Time = config.Rules.TimeLimit;

        events.MapStarted += RandomizerMapStarted;
        events.MapEnded += RandomizerMapEnded;
        events.Status += RandomizerStatus;
    }

    private void RandomizerStatus(string status)
    {
        StatusText = status;
    }

    private void RandomizerMapStarted(SessionMap map)
    {
        updateTimeCancellationTokenSource = new CancellationTokenSource();
        updateTimeTask = Task.Run(async () =>
        {
            while (true)
            {
                if (engine.CurrentSession?.Watch is not null)
                {
                    Time = config.Rules.TimeLimit - engine.CurrentSession.Watch.Elapsed;
                    this.RaisePropertyChanged(nameof(TimeText));
                }

                await Task.Delay(50, updateTimeCancellationTokenSource.Token);
            }
        }, updateTimeCancellationTokenSource.Token);

        TimeOpacity = 1f;
    }

    private void RandomizerMapEnded()
    {
        updateTimeCancellationTokenSource?.Cancel();
        updateTimeCancellationTokenSource = null;
        updateTimeTask = null;
        TimeOpacity = 0.5f;
    }
}
