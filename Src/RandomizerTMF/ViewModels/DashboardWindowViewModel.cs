using Avalonia.Controls;
using Avalonia.Data.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic;
using RandomizerTMF.Logic.Exceptions;
using RandomizerTMF.Logic.Services;
using RandomizerTMF.Models;
using RandomizerTMF.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Reactive.Linq;

namespace RandomizerTMF.ViewModels;

internal class DashboardWindowViewModel : WindowWithTopBarViewModelBase
{
    private ObservableCollection<AutosaveModel> autosaves = new();
    private ObservableCollection<SessionDataModel> sessions = new();
    private ObservableCollection<SessionDataModel> bestSessions = new();
    private ObservableCollection<PresetDataModel> presets = new();
    private readonly IRandomizerEngine engine;
    private readonly IRandomizerConfig config;
    private readonly IValidator validator;
    private readonly IFilePathManager filePathManager;
    private readonly IFileSystem? fileSystem;
    private readonly IAutosaveScanner autosaveScanner;
    private readonly ITMForever game;
    private readonly IUpdateDetector updateDetector;
    private readonly IDiscordRichPresence discord;
    private readonly ILogger logger;

    public RequestRulesControlViewModel RequestRulesControlViewModel { get; set; }

    public string? GameDirectory => config.GameDirectory;

    public ObservableCollection<AutosaveModel> Autosaves
    {
        get => autosaves;
        private set => this.RaiseAndSetIfChanged(ref autosaves, value);
    }

    public ObservableCollection<SessionDataModel> Sessions
    {
        get => sessions;
        private set => this.RaiseAndSetIfChanged(ref sessions, value);
    }

    public ObservableCollection<SessionDataModel> BestSessions
    {
        get => bestSessions;
        private set => this.RaiseAndSetIfChanged(ref bestSessions, value);
    }

    public ObservableCollection<PresetDataModel> Presets
    {
        get => presets;
        private set => this.RaiseAndSetIfChanged(ref presets, value);
    }
    private string? presetName { get; set; }

    public string? PresetName
    {
        get => presetName;
        private set
        {
            presetName = string.IsNullOrWhiteSpace(value) ? null : value;

            this.RaisePropertyChanged(nameof(PresetName));
        }
    }

    public bool HasAutosavesScanned => autosaveScanner.HasAutosavesScanned;
    public int AutosaveScanCount => autosaveScanner.AutosaveHeaders.Count;

    public DashboardWindowViewModel(TopBarViewModel topBarViewModel,
                                    IRandomizerEngine engine,
                                    IRandomizerConfig config,
                                    IValidator validator,
                                    IFilePathManager filePathManager,
                                    IFileSystem fileSystem,
                                    IAutosaveScanner autosaveScanner,
                                    ITMForever game,
                                    IUpdateDetector updateDetector,
                                    IDiscordRichPresence discord,
                                    ILogger logger) : base(topBarViewModel)
    {
        this.engine = engine;
        this.config = config;
        this.validator = validator;
        this.filePathManager = filePathManager;
        this.fileSystem = fileSystem;
        this.autosaveScanner = autosaveScanner;
        this.game = game;
        this.updateDetector = updateDetector;
        this.discord = discord;
        this.logger = logger;

        RequestRulesControlViewModel = new(config);

        discord.InDashboard();
    }

    protected internal override void OnInit()
    {
        base.OnInit();

        Window.Opened += Opened;
    }

    private async void Opened(object? sender, EventArgs e)
    {
        sessions.Clear();

        var sessionsTask = ScanSessionsAsync();

        var presetsTask = ScanPresetsAsync();

        var anythingChanged = await ScanAutosavesAsync();

        if (anythingChanged && !config.DisableAutosaveDetailScan)
        {
            await UpdateAutosavesWithFullParseAsync();
        }

        await sessionsTask;

        await presetsTask;
    }

    private async Task ScanSessionsAsync()
    {
        foreach (var dir in Directory.EnumerateDirectories(FilePathManager.SessionsDirectoryPath))
        {
            var sessionYml = Path.Combine(dir, "Session.yml");

            if (!File.Exists(sessionYml))
            {
                continue;
            }

            SessionData sessionData;
            SessionDataModel sessionDataModel;

            try
            {
                var sessionYmlContent = await File.ReadAllTextAsync(sessionYml);
                sessionData = Yaml.Deserializer.Deserialize<SessionData>(sessionYmlContent);
                sessionDataModel = new SessionDataModel(sessionData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Corrupted Session.yml in '{session}'", Path.GetFileName(dir));
                continue;
            }

            if (sessions.Count == 0)
            {
                sessions.Add(sessionDataModel);
                bestSessions.Add(sessionDataModel);
                continue;
            }

            // insert by date
            for (int i = 0; i < sessions.Count; i++)
            {
                if (sessions[i].Data.StartedAt < sessionData.StartedAt)
                {
                    sessions.Insert(i, sessionDataModel);
                    bestSessions.Insert(i, sessionDataModel);
                    break;
                }
            }
        }

        // ordering leaderboard by average time for geting an author.
        foreach (var bs in bestSessions.ToObservable())
        {
            if (bs.AuthorMedalCount == 0)
            {
                bestSessions.Remove(bs);
                continue;
            }

            bs.Data.AuthorRate = Math.Round(bs.Data.Rules.TimeLimit.TotalMilliseconds / bs.AuthorMedalCount);
        }

        for (int i = 0; i < bestSessions.Count; i++)
        {
            for (int j = 0; j < bestSessions.Count; j++)
            {
                double authAvg1 = bestSessions[i].Data.AuthorRate;
                double authAvg2 = bestSessions[j].Data.AuthorRate;

                if (authAvg1 < authAvg2)
                {
                    SessionDataModel temp = bestSessions[j];
                    bestSessions[j] = bestSessions[i];
                    bestSessions[i] = temp;
                }
                else if (authAvg1 == authAvg2)
                {
                    if (bestSessions[j].GoldMedalCount < bestSessions[i].GoldMedalCount)
                    {
                        SessionDataModel temp = bestSessions[j];
                        bestSessions[j] = bestSessions[i];
                        bestSessions[i] = temp;
                    }
                    else if (bestSessions[j].GoldMedalCount == bestSessions[i].GoldMedalCount)
                    {
                        if (bestSessions[j].SkippedCount > bestSessions[i].SkippedCount)
                        {
                            SessionDataModel temp = bestSessions[j];
                            bestSessions[j] = bestSessions[i];
                            bestSessions[i] = temp;
                        }
                    }
                }
            }
        }
    }

    private async Task ScanPresetsAsync()
    {
        foreach (var dir in Directory.EnumerateDirectories(FilePathManager.PresetsDirectoryPath))
        {
            var presetYml = Path.Combine(dir, Constants.PresetYml);

            if (!File.Exists(presetYml))
            {
                continue;
            }

            PresetDataModel preset;
            RandomizerRules presetData;

            try
            {
                var presetName = dir.Split(Path.DirectorySeparatorChar)[dir.Split(Path.DirectorySeparatorChar).Length - 1];
                var presetYmlContent = await File.ReadAllTextAsync(presetYml);
                presetData = Yaml.Deserializer.Deserialize<RandomizerRules>(presetYmlContent);
                preset = new PresetDataModel(presetName, presetData);

                Presets.Add(new PresetDataModel(presetName, presetData));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Corrupted Preset.yml in '{presets}'", Path.GetFileName(dir));
                continue;
            }
        }
    }

    private async Task<bool> ScanAutosavesAsync()
    {
        var cts = new CancellationTokenSource();

        var anythingChanged = Task.Run(autosaveScanner.ScanAutosaves);

        await Task.WhenAny(anythingChanged, Task.Run(async () =>
        {
            while (true)
            {
                Autosaves = new(GetAutosaveModels());
                this.RaisePropertyChanged(nameof(AutosaveScanCount));
                await Task.Delay(20, cts.Token);
            }
        }));

        cts.Cancel();

        Autosaves = new(GetAutosaveModels());

        this.RaisePropertyChanged(nameof(HasAutosavesScanned));

        return anythingChanged.Result;
    }

    private async Task UpdateAutosavesWithFullParseAsync()
    {
        var cts = new CancellationTokenSource();

        await Task.WhenAny(Task.Run(autosaveScanner.ScanDetailsFromAutosaves), Task.Run(async () =>
        {
            while (true)
            {
                Autosaves = new(GetAutosaveModels());
                await Task.Delay(20, cts.Token);
            }
        }));

        cts.Cancel();

        Autosaves = new(GetAutosaveModels());
    }

    private IEnumerable<AutosaveModel> GetAutosaveModels()
    {
        return autosaveScanner.AutosaveDetails.Select(x => new AutosaveModel(x.Key, x.Value)).OrderBy(x => x.Autosave.MapName);
    }

    protected override void CloseClick()
    {
        engine.Exit();
    }

    protected override void MinimizeClick()
    {
        Window.WindowState = WindowState.Minimized;
    }

    public void ChangeClick()
    {
        SwitchWindowTo<MainWindow, MainWindowViewModel>();
    }

    public void OpenModuleClick()
    {
        try
        {
            validator.ValidateRules();
        }
        catch (RuleValidationException ex)
        {
            OpenMessageBox("Validation problem", ex.Message);
            return;
        }

        discord.Idle();
        discord.SessionState();

        App.Modules = new Window[]
        {
            OpenModule<ControlModuleWindow, ControlModuleWindowViewModel>(config.Modules.Control),
            OpenModule<StatusModuleWindow, StatusModuleWindowViewModel>(config.Modules.Status),
            OpenModule<ProgressModuleWindow, ProgressModuleWindowViewModel>(config.Modules.Progress),
            OpenModule<HistoryModuleWindow, HistoryModuleWindowViewModel>(config.Modules.History)
        };

        Window.Close();
    }

    private static TWindow OpenModule<TWindow, TViewModel>(ModuleConfig config)
        where TWindow : Window
        where TViewModel : WindowViewModelBase
    {
        var window = OpenWindow<TWindow, TViewModel>();

        // Initial module positioning, absolute afterwards
        if (config.Relative)
        {
            if (config.X < 0)
            {
                if (config.X < -window.Screens.Primary.WorkingArea.Width)
                {
                    config.X = 0;
                }
                else
                {
                    config.X += window.Screens.Primary.WorkingArea.Width - config.Width;
                }
            }

            if (config.Y < 0)
            {
                if (config.Y < -window.Screens.Primary.WorkingArea.Height)
                {
                    config.Y = 0;
                }
                else
                {
                    config.Y += window.Screens.Primary.WorkingArea.Height - config.Height;
                }
            }

            config.Relative = false;
        }

        window.Position = new(config.X, config.Y);
        window.Width = config.Width;
        window.Height = config.Height;

        return window;
    }

    public void BestSessionDoubleClick(int selectedIndex)
    {
        if (selectedIndex < 0)
        {
            return;
        }

        var sessionModel = BestSessions[selectedIndex];

        if (Program.ServiceProvider is null)
        {
            throw new UnreachableException("ServiceProvider is null");
        }

        var topBarViewModel = Program.ServiceProvider.GetRequiredService<TopBarViewModel>();

        OpenDialog<SessionDataWindow>(window => new SessionDataViewModel(topBarViewModel, sessionModel)
        {
            Window = window
        });
    }

    public void SessionDoubleClick(int selectedIndex)
    {
        if (selectedIndex < 0)
        {
            return;
        }

        var sessionModel = Sessions[selectedIndex];

        if (Program.ServiceProvider is null)
        {
            throw new UnreachableException("ServiceProvider is null");
        }

        var topBarViewModel = Program.ServiceProvider.GetRequiredService<TopBarViewModel>();

        OpenDialog<SessionDataWindow>(window => new SessionDataViewModel(topBarViewModel, sessionModel)
        {
            Window = window
        });
    }

    public void PresetsDoubleClick(int selectedIndex)
    {
        if (selectedIndex < 0)
        {
            return;
        }

        var presetData = Presets[selectedIndex];

        if (Program.ServiceProvider is null)
        {
            throw new UnreachableException("ServiceProvider is null");
        }

        RequestRulesControlViewModel.config.Rules = presetData.Data;
        RequestRulesControlViewModel.RaisePropertyChanged(string.Empty);
    }

    public void AutosaveDoubleClick(int selectedIndex)
    {
        if (selectedIndex < 0)
        {
            return;
        }

        var autosaveModel = Autosaves[selectedIndex];

        if (!autosaveScanner.AutosaveHeaders.TryGetValue(autosaveModel.MapUid, out AutosaveHeader? autosave))
        {
            return;
        }

        OpenDialog<AutosaveDetailsWindow>(window => new AutosaveDetailsWindowViewModel(new TopBarViewModel(updateDetector), game, autosaveModel, autosave.FilePath)
        {
            Window = window
        });
    }

    public bool OpenDownloadedMapsFolderClick()
    {
        if (!Directory.Exists(filePathManager.DownloadedDirectoryPath))
        {
            OpenMessageBox("Directory not found", "Downloaded maps directory has not been yet created.");

            return false;
        }

        ProcessUtils.OpenDir(filePathManager.DownloadedDirectoryPath + Path.DirectorySeparatorChar);

        return true;
    }

    public async void SavePresetClick()
    {
        string DirectoryPath;

        try
        {
            DirectoryPath = Path.Combine(FilePathManager.PresetsDirectoryPath, PresetName!);
            fileSystem?.Directory.CreateDirectory(DirectoryPath);

        }
        catch (Exception ex)
        {
            OpenMessageBox("Error", String.Format("The foldername given is invalid.\n\n({0})", ex.Message));
            return;
        }

        fileSystem?.File.WriteAllText(Path.Combine(DirectoryPath, Constants.PresetYml), Yaml.Serializer.Serialize(RequestRulesControlViewModel.config.Rules));

        logger?.LogInformation("Preset saved.");

        PresetName = null;

        Presets.Clear();

        await ScanPresetsAsync();
    }

    public void OpenPresetsFolderClick()
    {
        ProcessUtils.OpenDir(FilePathManager.PresetsDirectoryPath + Path.DirectorySeparatorChar);
    }

    public void OpenSessionsFolderClick()
    {
        ProcessUtils.OpenDir(FilePathManager.SessionsDirectoryPath + Path.DirectorySeparatorChar);
    }
}
