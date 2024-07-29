using GBX.NET.Engines.Game;
using RandomizerTMF.Logic;
using RandomizerTMF.Logic.Services;
using RandomizerTMF.Models;
using ReactiveUI;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using TmEssentials;

namespace RandomizerTMF.ViewModels;

internal class SessionDataViewModel : WindowWithTopBarViewModelBase
{
    private ObservableCollection<SessionDataReplayModel> replays = [];
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

    public bool IsOpenReplaysFolderEnabled => Maps.Any(x => x.Map.Replays.Count != 0);

    /// <summary>
    /// Should be only used for window preview
    /// </summary>
    public SessionDataViewModel() : this(new(), new(new()
    {
        StartedAt = DateTimeOffset.Now,
        Maps =
            [
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
            ]
    }))
    {
        TopBarViewModel.Title = "Session";
    }

    public SessionDataViewModel(TopBarViewModel topBarViewModel, SessionDataModel model) : base(topBarViewModel)
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
        var rules = new ObservableCollection<string>
        {
            "Version: " + (Model.Data.Version is null ? "< 1.0.3" : Model.Data.Version)
        };

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

            AddRuleString(rules, prop, Model.Data.Rules);
        }

        foreach (var prop in Model.Data.Rules.RequestRules.GetType().GetProperties())
        {
            AddRuleString(rules, prop, Model.Data.Rules.RequestRules);
        }

        return rules;
    }

    private static void AddRuleString(IList rules, PropertyInfo prop, object owner)
    {
        var val = prop.GetValue(owner);

        if (val is null)
        {
            return;
        }

        if (val is bool valBool)
        {
            if (valBool)
            {
                rules.Add(ToSentenceCase(prop.Name));
            }

            return;
        }

        if (val is not string and IEnumerable enumerable)
        {
            if (enumerable.Cast<object>().Any())
            {
                val = string.Join(", ", enumerable.Cast<object>().Select(x => x.ToString()));
            }
            else
            {
                return;
            }
        }

        if (val is TimeInt32 timeInt32)
        {
            val = timeInt32.ToString(useHundredths: true);
        }

        rules.Add($"{ToSentenceCase(prop.Name)}: {val}");
    }

    public void OpenSessionFolderClick()
    {
        ProcessUtils.OpenDir(Path.Combine(FilePathManager.SessionsDirectoryPath, Model.Data.StartedAtText) + Path.DirectorySeparatorChar);
    }

    public void OpenReplaysFolderClick()
    {
        var replaysDir = Path.Combine(FilePathManager.SessionsDirectoryPath, Model.Data.StartedAtText, Constants.Replays);

        if (Directory.Exists(replaysDir))
        {
            ProcessUtils.OpenDir(replaysDir + Path.DirectorySeparatorChar);
        }
    }

    // THX https://stackoverflow.com/a/1211435/3923447
    private static string ToSentenceCase(string str)
    {
        return CompiledRegex.SentenceCaseRegex().Replace(str, m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
    }
}
