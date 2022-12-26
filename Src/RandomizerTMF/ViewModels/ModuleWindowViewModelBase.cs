using RandomizerTMF.Logic;

namespace RandomizerTMF.ViewModels;

internal class ModuleWindowViewModelBase : WindowViewModelBase
{
    private readonly IRandomizerConfig config;

    public ModuleWindowViewModelBase(IRandomizerConfig config)
    {
        this.config = config;
    }

    internal void OnClosing(Func<ModulesConfig, ModuleConfig> func, int x, int y, double width, double height)
    {
        var module = func(config.Modules);

        module.X = x;
        module.Y = y;
        module.Width = Convert.ToInt32(width);
        module.Height = Convert.ToInt32(height);

        config.Save();
    }
}
