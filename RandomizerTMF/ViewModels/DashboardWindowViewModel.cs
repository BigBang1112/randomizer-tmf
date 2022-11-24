using Avalonia.Controls;
using RandomizerTMF.Logic;
using RandomizerTMF.Models;
using RandomizerTMF.Views;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace RandomizerTMF.ViewModels;

public class DashboardWindowViewModel : WindowViewModelBase
{
    private ObservableCollection<AutosaveModel> autosaves = new();

    public TopBarViewModel TopBarViewModel { get; set; }
    public RequestRulesControlViewModel RequestRulesControlViewModel { get; set; }

    public string? GameDirectory => RandomizerEngine.Config.GameDirectory;

    public ObservableCollection<AutosaveModel> Autosaves
    {
        get => autosaves;
        private set => this.RaiseAndSetIfChanged(ref autosaves, value);
    }

    public bool HasAutosavesScanned => RandomizerEngine.HasAutosavesScanned;
    public int AutosaveScanCount => RandomizerEngine.Autosaves.Count;

    public DashboardWindowViewModel()
    {
        TopBarViewModel = new();
        TopBarViewModel.CloseClick += CloseClick;
        TopBarViewModel.MinimizeClick += MinimizeClick;

        RequestRulesControlViewModel = new();
    }

    protected internal override void OnInit()
    {
        Window.Opened += Opened;
    }

    private async void Opened(object? sender, EventArgs e)
    {
        var anythingChanged = await ScanAutosavesAsync();

        if (anythingChanged)
        {
            await UpdateAutosavesWithFullParseAsync();
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

    public void CloseClick()
    {
        RandomizerEngine.Exit();
    }

    public void MinimizeClick()
    {
        Window.WindowState = WindowState.Minimized;
    }

    public void ChangeClick()
    {
        SwitchWindowTo<MainWindow, MainWindowViewModel>();
    }

    public void StartModulesClick()
    {
        var controlModuleWindow = OpenWindow<ControlModuleWindow, ControlModuleWindowViewModel>();
        controlModuleWindow.Position = new(RandomizerEngine.Config.Modules.Control.X, RandomizerEngine.Config.Modules.Control.Y);
        controlModuleWindow.Width = RandomizerEngine.Config.Modules.Control.Width;
        controlModuleWindow.Height = RandomizerEngine.Config.Modules.Control.Height;

        var statusModuleWindow = OpenWindow<StatusModuleWindow, StatusModuleWindowViewModel>();
        statusModuleWindow.Position = new(RandomizerEngine.Config.Modules.Status.X, RandomizerEngine.Config.Modules.Status.Y);
        statusModuleWindow.Width = RandomizerEngine.Config.Modules.Status.Width;
        statusModuleWindow.Height = RandomizerEngine.Config.Modules.Status.Height;
        
        var progressModuleWindow = OpenWindow<ProgressModuleWindow, ProgressModuleWindowViewModel>();
        progressModuleWindow.Position = new(RandomizerEngine.Config.Modules.Progress.X, RandomizerEngine.Config.Modules.Progress.Y);
        progressModuleWindow.Width = RandomizerEngine.Config.Modules.Progress.Width;
        progressModuleWindow.Height = RandomizerEngine.Config.Modules.Progress.Height;

        App.Modules = new[] { controlModuleWindow, statusModuleWindow, progressModuleWindow };

        Window.Close();
    }

    public void AutosaveDoubleClick(int selectedIndex)
    {
        if (selectedIndex < 0)
        {
            return;
        }

        if (!RandomizerEngine.AutosavePaths.TryGetValue(Autosaves[selectedIndex].MapUid, out string? fileName))
        {
            return;
        }

        RandomizerEngine.OpenAutosaveIngame(fileName);
    }
}
