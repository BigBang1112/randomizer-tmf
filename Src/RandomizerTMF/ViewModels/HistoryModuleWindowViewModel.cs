using Microsoft.Extensions.DependencyInjection;
using RandomizerTMF.Logic;
using RandomizerTMF.Models;
using RandomizerTMF.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RandomizerTMF.ViewModels;

internal class HistoryModuleWindowViewModel : ModuleWindowViewModelBase
{
    private readonly IRandomizerEngine engine;
    private readonly IRandomizerEvents events;
    
    private ObservableCollection<PlayedMapModel> playedMaps = new();

    public ObservableCollection<PlayedMapModel> PlayedMaps
    {
        get => playedMaps;
        set => this.RaiseAndSetIfChanged(ref playedMaps, value); 
    }

    public bool HasFinishedMaps => PlayedMaps.Count > 0;

    public HistoryModuleWindowViewModel(IRandomizerEngine engine, IRandomizerEvents events, IRandomizerConfig config) : base(config)
    {
        this.engine = engine;
        this.events = events;

        events.MedalUpdate += RandomizerPlayedMapUpdate;
        events.MapSkip += RandomizerPlayedMapUpdate;
    }

    private void RandomizerPlayedMapUpdate()
    {
        PlayedMaps = new(EnumerateCurrentSessionMaps().OrderByDescending(x => x.Map.ReceivedAt));
        this.RaisePropertyChanged(nameof(HasFinishedMaps));
    }

    private IEnumerable<PlayedMapModel> EnumerateCurrentSessionMaps()
    {
        if (engine.CurrentSession is null)
        {
            yield break;
        }

        foreach (var map in engine.CurrentSession.AuthorMaps)
        {
            yield return new PlayedMapModel(map.Value, EResult.AuthorMedal);
        }

        foreach (var map in engine.CurrentSession.GoldMaps)
        {
            yield return new PlayedMapModel(map.Value, EResult.GoldMedal);
        }

        foreach (var map in engine.CurrentSession.SkippedMaps)
        {
            yield return new PlayedMapModel(map.Value, EResult.Skipped);
        }
    }

    public void MapDoubleClick(PlayedMapModel? selectedItem)
    {
        if (selectedItem is null)
        {
            return;
        }

        if (Program.ServiceProvider is null)
        {
            throw new UnreachableException("Program.ServiceProvider is null");
        }

        var topBarViewModel = Program.ServiceProvider.GetRequiredService<TopBarViewModel>();

        OpenDialog<SessionMapWindow>(window => new SessionMapViewModel(topBarViewModel, selectedItem)
        {
            Window = window
        });
    }
}
