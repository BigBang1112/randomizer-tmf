using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic;
using RandomizerTMF.Logic.Exceptions;
using RandomizerTMF.Models;
using RandomizerTMF.Views;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RandomizerTMF.ViewModels;

public class DashboardWindowViewModel : WindowWithTopBarViewModelBase
{
    private ObservableCollection<AutosaveModel> autosaves = new();
    private ObservableCollection<SessionDataModel> sessions = new();
    
    public RequestRulesControlViewModel RequestRulesControlViewModel { get; set; }

    public string? GameDirectory => RandomizerEngine.Config.GameDirectory;

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

    public bool HasAutosavesScanned => RandomizerEngine.HasAutosavesScanned;
    public int AutosaveScanCount => RandomizerEngine.AutosaveHeaders.Count;

    public DashboardWindowViewModel()
    {
        RequestRulesControlViewModel = new();
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

        var anythingChanged = await ScanAutosavesAsync();

        if (anythingChanged)
        {
            await UpdateAutosavesWithFullParseAsync();
        }

        await sessionsTask;
    }
    
    private async Task ScanSessionsAsync()
    {
        foreach (var dir in Directory.EnumerateDirectories(RandomizerEngine.SessionsDirectoryPath))
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
                sessionData = RandomizerEngine.YamlDeserializer.Deserialize<SessionData>(sessionYmlContent);
                sessionDataModel = new SessionDataModel(sessionData);
            }
            catch (Exception ex)
            {
                RandomizerEngine.Logger.LogError(ex, "Corrupted Session.yml in '{session}'", Path.GetFileName(dir));
                continue;
            }

            if (sessions.Count == 0)
            {
                sessions.Add(sessionDataModel);
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

        var anythingChanged = Task.Run(RandomizerEngine.ScanAutosaves);

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

        await Task.WhenAny(Task.Run(RandomizerEngine.ScanDetailsFromAutosaves), Task.Run(async () =>
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

    private static IEnumerable<AutosaveModel> GetAutosaveModels()
    {
        return RandomizerEngine.AutosaveDetails.Select(x => new AutosaveModel(x.Key, x.Value)).OrderBy(x => x.Autosave.MapName);
    }

    protected override void CloseClick()
    {
        RandomizerEngine.Exit();
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
            RandomizerEngine.ValidateRules();
        }
        catch (RuleValidationException ex)
        {
            OpenMessageBox("Validation problem", ex.Message);
            return;
        }

        App.Modules = new Window[]
        {
            OpenModule<ControlModuleWindow, ControlModuleWindowViewModel>(RandomizerEngine.Config.Modules.Control),
            OpenModule<StatusModuleWindow, StatusModuleWindowViewModel>(RandomizerEngine.Config.Modules.Status),
            OpenModule<ProgressModuleWindow, ProgressModuleWindowViewModel>(RandomizerEngine.Config.Modules.Progress),
            OpenModule<HistoryModuleWindow, HistoryModuleWindowViewModel>(RandomizerEngine.Config.Modules.History)
        };

        Window.Close();
    }

    private static TWindow OpenModule<TWindow, TViewModel>(ModuleConfig config)
        where TWindow : Window, new()
        where TViewModel : WindowViewModelBase, new()
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

        OpenDialog<SessionDataWindow>(window => new SessionDataViewModel(sessionModel)
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

        if (!RandomizerEngine.AutosaveHeaders.TryGetValue(autosaveModel.MapUid, out AutosaveHeader? autosave))
        {
            return;
        }

        OpenDialog<AutosaveDetailsWindow>(window => new AutosaveDetailsWindowViewModel(autosaveModel, autosave.FilePath)
        {
            Window = window
        });
    }

    public void OpenDownloadedMapsFolderClick()
    {
        if (RandomizerEngine.DownloadedDirectoryPath is not null)
        {
            ProcessUtils.OpenDir(RandomizerEngine.DownloadedDirectoryPath + Path.DirectorySeparatorChar);
        }
    }

    public void OpenSessionsFolderClick()
    {
        if (RandomizerEngine.SessionsDirectoryPath is not null)
        {
            ProcessUtils.OpenDir(RandomizerEngine.SessionsDirectoryPath + Path.DirectorySeparatorChar);
        }
    }
}
