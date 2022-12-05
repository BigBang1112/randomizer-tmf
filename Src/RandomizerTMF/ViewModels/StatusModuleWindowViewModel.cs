using RandomizerTMF.Logic;
using ReactiveUI;

namespace RandomizerTMF.ViewModels;

public class StatusModuleWindowViewModel : WindowViewModelBase
{
    private Task? updateTimeTask;
    private CancellationTokenSource? updateTimeCancellationTokenSource;

    private string statusText = "Idle";
    private float timeOpacity = 1f;

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

    public StatusModuleWindowViewModel()
    {
        Time = RandomizerEngine.Config.Rules.TimeLimit;

        RandomizerEngine.MapStarted += RandomizerMapStarted;
        RandomizerEngine.MapEnded += RandomizerMapEnded;
        RandomizerEngine.Status += RandomizerStatus;
    }

    private void RandomizerStatus(string status)
    {
        StatusText = status;
    }

    private void RandomizerMapStarted()
    {
        updateTimeCancellationTokenSource = new CancellationTokenSource();
        updateTimeTask = Task.Run(async () =>
        {
            while (true)
            {
                if (RandomizerEngine.CurrentSessionWatch is not null)
                {
                    Time = RandomizerEngine.Config.Rules.TimeLimit - RandomizerEngine.CurrentSessionWatch.Elapsed;
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
