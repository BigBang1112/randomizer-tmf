using Avalonia.Media;
using RandomizerTMF.Logic.Services;
using ReactiveUI;

namespace RandomizerTMF.ViewModels;

internal class ProgressModuleWindowViewModel : ModuleWindowViewModelBase
{
    private readonly IRandomizerEngine engine;
    private readonly IRandomizerEvents events;

    public int AuthorMedalCount => engine.CurrentSession?.AuthorMaps.Count ?? 0;
    public int GoldMedalCount => engine.CurrentSession?.GoldMaps.Count ?? 0;
    public int SkipCount => engine.CurrentSession?.SkippedMaps.Count ?? 0;
    public IBrush SkipColor => SkipCount == 0 ? Brushes.LightGreen : Brushes.Orange;

    public ProgressModuleWindowViewModel(IRandomizerEngine engine, IRandomizerEvents events, IRandomizerConfig config) : base(config)
    {
        this.engine = engine;
        this.events = events;

        events.MedalUpdate += RandomizerMedalUpdate;
        events.MapSkip += RandomizerMapSkip;
    }

    private void RandomizerMedalUpdate()
    {
        this.RaisePropertyChanged(nameof(AuthorMedalCount));
        this.RaisePropertyChanged(nameof(GoldMedalCount));
    }

    private void RandomizerMapSkip()
    {
        this.RaisePropertyChanged(nameof(SkipCount));
        this.RaisePropertyChanged(nameof(SkipColor));
    }
}
