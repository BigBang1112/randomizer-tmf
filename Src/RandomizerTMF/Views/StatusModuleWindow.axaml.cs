using Avalonia.Controls;
using RandomizerTMF.Logic;

namespace RandomizerTMF.Views
{
    public partial class StatusModuleWindow : Window
    {
        public StatusModuleWindow()
        {
            InitializeComponent();

            Closing += (_, _) =>
            {
                RandomizerEngine.Config.Modules.Status.X = Convert.ToInt32(Position.X);
                RandomizerEngine.Config.Modules.Status.Y = Convert.ToInt32(Position.Y);
                RandomizerEngine.Config.Modules.Status.Width = Convert.ToInt32(Width);
                RandomizerEngine.Config.Modules.Status.Height = Convert.ToInt32(Height);

                RandomizerEngine.SaveConfig();
            };

            Deactivated += (_, _) => { Topmost = false; Topmost = true; };
        }
    }
}
