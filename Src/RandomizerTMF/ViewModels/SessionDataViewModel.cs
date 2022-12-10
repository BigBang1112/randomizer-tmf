using GBX.NET.Engines.Game;
using RandomizerTMF.Logic;
using RandomizerTMF.Models;
using ReactiveUI;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using TmEssentials;

namespace RandomizerTMF.ViewModels;

public partial class SessionDataViewModel : WindowWithTopBarViewModelBase
{
    private ObservableCollection<SessionDataReplayModel> replays = new();
    private SessionDataMapModel? selectedMap;

    public SessionDataModel Model { get; }
    public ObservableCollection<string> Rules { get; }
    public ObservableCollection<SessionDataMapModel> Maps { get; }

    public ObservableCollection<SessionDataReplayModel> Replays
    {
        get => replays;
        private set => this.RaiseAndSetIfChanged(ref replays, value);
    }

    public SessionDataMapModel? SelectedMap
    {
        get => selectedMap;
        set
        {
            selectedMap = value;

            Replays.Clear();

            if (selectedMap is null)
            {
                return;
            }

            var map = default(CGameCtnChallenge);
            var first = true;

            foreach (var replay in selectedMap.Map.Replays.Reverse<SessionDataReplay>())
            {
                var replayModel = new SessionDataReplayModel(replay, Model.Data.StartedAtText, first, map);

                Replays.Add(replayModel);

                if (first)
                {
                    map = replayModel.Map;
                }

                first = false;
            }
        }
    }

    public bool IsOpenReplaysFolderEnabled => Maps.Any(x => x.Map.Replays.Any());

    /// <summary>
    /// Should be only used for window preview
    /// </summary>
    public SessionDataViewModel() : this(new(new()
    {
        StartedAt = DateTimeOffset.Now,
        Maps = new List<SessionDataMap>()
            {
                new()
                {
                    Name = "ShortBang United #025",
                    TmxLink = "https://tmuf.exchange/trackshow/4796703",
                    Uid = "tNSGl9lBHsZFCQUAOa7UyUYGLmb"
                },
                new()
                {
                    Name = "ShortBang United #032 FINALIZE",
                    TmxLink = "https://tmuf.exchange/trackshow/4888545",
                    Uid = "tNSGl9lBHsZFCQUAOa7UyUYGLmb"
                },
                new()
                {
                    Name = "ShortBang United #024",
                    TmxLink = "https://tmuf.exchange/trackshow/4796141",
                    Uid = "tNSGl9lBHsZFCQUAOa7UyUYGLmb"
                }
            }
    }))
    {
        TopBarViewModel.Title = "Session";
    }

    public SessionDataViewModel(SessionDataModel model)
    {
        Model = model;
        Rules = ConstructRules();
        Maps = new(model.Data.Maps
            .OrderByDescending(x => x.LastTimestamp is null)
            .ThenByDescending(x => x.LastTimestamp)
            .Select(x => new SessionDataMapModel(x)));

        TopBarViewModel.MinimizeButtonEnabled = false;
    }

    private ObservableCollection<string> ConstructRules()
    {
        var rules = new ObservableCollection<string>();

        if (Model.Data.Rules is null) // To handle sessions made in early Randomizer TMF version
        {
            return rules;
        }

        foreach (var prop in Model.Data.Rules.GetType().GetProperties())
        {
            if (prop.Name == nameof(RandomizerRules.RequestRules))
            {
                continue;
            }

            rules.Add($"{ToSentenceCase(prop.Name)}: {prop.GetValue(Model.Data.Rules)}");
        }

        foreach (var prop in Model.Data.Rules.RequestRules.GetType().GetProperties())
        {
            var val = prop.GetValue(Model.Data.Rules.RequestRules);

            if (val is null)
            {
                continue;
            }

            if (val is not string and IEnumerable enumerable)
            {
                if (enumerable.Cast<object>().Any())
                {
                    val = string.Join(", ", enumerable.Cast<object>().Select(x => x.ToString()));
                }
                else
                {
                    val = null;
                }
            }

            if (val is TimeInt32 timeInt32)
            {
                val = timeInt32.ToString(useHundredths: true);
            }

            rules.Add($"{ToSentenceCase(prop.Name)}: {val}");
        }

        return rules;
    }

    public void OpenSessionFolderClick()
    {
        if (RandomizerEngine.SessionsDirectoryPath is not null)
        {
            OpenFolder(Path.Combine(RandomizerEngine.SessionsDirectoryPath, Model.Data.StartedAtText) + Path.DirectorySeparatorChar);
        }
    }

    public void OpenReplaysFolderClick()
    {
        if (RandomizerEngine.SessionsDirectoryPath is not null)
        {
            var replaysDir = Path.Combine(RandomizerEngine.SessionsDirectoryPath, Model.Data.StartedAtText, Constants.Replays);

            if (Directory.Exists(replaysDir))
            {
                OpenFolder(replaysDir + Path.DirectorySeparatorChar);
            }
        }
    }

    private static void OpenFolder(string folderPath)
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = folderPath,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    [GeneratedRegex("[a-z][A-Z]")]
    private static partial Regex SentenceCaseRegex();

    // THX https://stackoverflow.com/a/1211435/3923447
    private static string ToSentenceCase(string str)
    {
        return SentenceCaseRegex().Replace(str, m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
    }
}
