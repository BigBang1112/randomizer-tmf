using Avalonia.Media;
using RandomizerTMF.Logic;
using ReactiveUI;

namespace RandomizerTMF.ViewModels;

public class ProgressModuleWindowViewModel : WindowViewModelBase
{
    public int AuthorMedalCount => RandomizerEngine.CurrentSessionAuthorMaps.Count;
    public int GoldMedalCount => RandomizerEngine.CurrentSessionGoldMaps.Count;
    public int SkipCount => RandomizerEngine.CurrentSessionSkippedMaps.Count;
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
