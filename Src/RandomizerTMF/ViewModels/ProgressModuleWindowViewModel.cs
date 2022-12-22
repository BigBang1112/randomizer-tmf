using Avalonia.Media;
using RandomizerTMF.Logic;
using ReactiveUI;

namespace RandomizerTMF.ViewModels;

public class ProgressModuleWindowViewModel : WindowViewModelBase
{
    public int AuthorMedalCount => RandomizerEngine.CurrentSession?.AuthorMaps.Count ?? 0;
    public int GoldMedalCount => RandomizerEngine.CurrentSession?.GoldMaps.Count ?? 0;
    public int SkipCount => RandomizerEngine.CurrentSession?.SkippedMaps.Count ?? 0;
    public IBrush SkipColor => SkipCount == 0 ? Brushes.LightGreen : Brushes.Orange;

    public ProgressModuleWindowViewModel()
	{
        RandomizerEngine.MedalUpdate += RandomizerMedalUpdate;
        RandomizerEngine.MapSkip += RandomizerMapSkip;
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
