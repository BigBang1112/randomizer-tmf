using Avalonia.Controls;
using RandomizerTMF.Logic;

namespace RandomizerTMF.Views
{
    public partial class ProgressModuleWindow : Window
    {
        public ProgressModuleWindow()
        {
            InitializeComponent();

            Closing += (_, _) =>
            {
                RandomizerEngine.Config.Modules.Progress.X = Convert.ToInt32(Position.X);
                RandomizerEngine.Config.Modules.Progress.Y = Convert.ToInt32(Position.Y);
                RandomizerEngine.Config.Modules.Progress.Width = Convert.ToInt32(Width);
                RandomizerEngine.Config.Modules.Progress.Height = Convert.ToInt32(Height);

                RandomizerEngine.SaveConfig();
            };

            Deactivated += (_, _) => { Topmost = false; Topmost = true; };
        }
    }
}
