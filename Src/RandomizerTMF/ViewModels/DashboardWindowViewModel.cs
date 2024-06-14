using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic;
using RandomizerTMF.Logic.Exceptions;
using RandomizerTMF.Logic.Services;
using RandomizerTMF.Models;
using RandomizerTMF.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RandomizerTMF.ViewModels;

internal class DashboardWindowViewModel : WindowWithTopBarViewModelBase
{
    private ObservableCollection<AutosaveModel> autosaves = [];
    private ObservableCollection<SessionDataModel> sessions = [];
    private readonly IRandomizerEngine engine;
    private readonly IRandomizerConfig config;
    private readonly IValidator validator;
    private readonly IFilePathManager filePathManager;
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

    public bool HasAutosavesScanned => autosaveScanner.HasAutosavesScanned;
    public int AutosaveScanCount => autosaveScanner.AutosaveHeaders.Count;

    public bool TopSessions
    {
        get => config.TopSessions;
        set
        {
            config.TopSessions = value;

            this.RaisePropertyChanged(nameof(TopSessions));

            config.Save();
            
            Sessions = config.TopSessions
                ? new(sessions.OrderByDescending(x => x.Data.GetScore()))
                : new(sessions.OrderByDescending(x => x.Data.StartedAt));
        }
    }

    public Thickness HasAutosavesScannedThickness => HasAutosavesScanned ? new Thickness(1) : new Thickness(0);

    public DashboardWindowViewModel(TopBarViewModel topBarViewModel,
                                    IRandomizerEngine engine,
                                    IRandomizerConfig config,
                                    IValidator validator,
                                    IFilePathManager filePathManager,
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

        var sessionsTask = ScanSessionsAsync(config.TopSessions);

        var anythingChanged = await ScanAutosavesAsync();

        if (anythingChanged && !config.DisableAutosaveDetailScan)
        {
            await UpdateAutosavesWithFullParseAsync();
        }

        await sessionsTask;
    }
    
    private async Task ScanSessionsAsync(bool top)
    {
        foreach (var dir in Directory.EnumerateDirectories(FilePathManager.SessionsDirectoryPath))
        {
            var sessionBin = Path.Combine(dir, "Session.bin");
            var sessionYml = Path.Combine(dir, "Session.yml");
            var sessionBinExists = File.Exists(sessionBin);
            var sessionYmlExists = File.Exists(sessionYml);
            var hasSessionFile = sessionBinExists || sessionYmlExists;

            if (!hasSessionFile)
            {
                continue;
            }

            SessionData sessionData;
            SessionDataModel sessionDataModel;

            if (!sessionBinExists && sessionYmlExists)
            {
                var sessionYmlContent = await File.ReadAllTextAsync(sessionYml);
                sessionData = Yaml.Deserializer.Deserialize<SessionData>(sessionYmlContent);
                using var fs = File.Create(sessionBin);
                using var writer = new BinaryWriter(fs);
                sessionData.Serialize(writer);
                sessionBinExists = true;
            }

            try
            {
                if (sessionBinExists)
                {
                    using var fs = File.OpenRead(sessionBin);
                    using var reader = new BinaryReader(fs);
                    sessionData = new SessionData();
                    sessionData.Deserialize(reader);
                }
                else
                {
                    throw new Exception("Session.bin not found");
                }

                sessionDataModel = new SessionDataModel(sessionData);
            }
            catch (Exception ex)
            {
                if (sessionBinExists)
                {
                    logger.LogError(ex, "Corrupted Session.bin in '{session}'", Path.GetFileName(dir));
                }
                else
                {
                    logger.LogError(ex, "Corrupted Session.yml in '{session}'", Path.GetFileName(dir));
                }

                continue;
            }

            if (sessions.Count == 0)
            {
                sessions.Add(sessionDataModel);
                continue;
            }
            
            if (top)
            {
                var sessionDataScore = sessionData.GetScore();

                for (int i = 0; i < sessions.Count; i++)
                {
                    if (sessions[i].Data.GetScore() < sessionDataScore)
                    {
                        sessions.Insert(i, sessionDataModel);
                        break;
                    }

                    if (i == sessions.Count - 1)
                    {
                        sessions.Add(sessionDataModel);
                        break;
                    }
                }
                continue;
            }

            // insert by date
            for (int i = 0; i < sessions.Count; i++)
            {
                if (sessions[i].Data.StartedAt < sessionData.StartedAt)
                {
                    sessions.Insert(i, sessionDataModel);
                    break;
                }
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
        this.RaisePropertyChanged(nameof(HasAutosavesScannedThickness));

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

    public void StartModulesClick()
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

        App.Modules =
        [
            OpenModule<ControlModuleWindow, ControlModuleWindowViewModel>(config.Modules.Control),
            OpenModule<StatusModuleWindow, StatusModuleWindowViewModel>(config.Modules.Status),
            OpenModule<ProgressModuleWindow, ProgressModuleWindowViewModel>(config.Modules.Progress),
            OpenModule<HistoryModuleWindow, HistoryModuleWindowViewModel>(config.Modules.History)
        ];

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

    public void OpenSessionsFolderClick()
    {
        ProcessUtils.OpenDir(FilePathManager.SessionsDirectoryPath + Path.DirectorySeparatorChar);
    }
}
