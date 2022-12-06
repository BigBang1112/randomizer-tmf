using Avalonia.Controls;
using RandomizerTMF.Logic;

namespace RandomizerTMF.Views
{
    public partial class HistoryModuleWindow : Window
    {
        public HistoryModuleWindow()
        {
            InitializeComponent();

            Closing += (_, _) =>
            {
                RandomizerEngine.Config.Modules.History.X = Convert.ToInt32(Position.X);
                RandomizerEngine.Config.Modules.History.Y = Convert.ToInt32(Position.Y);
                RandomizerEngine.Config.Modules.History.Width = Convert.ToInt32(Width);
                RandomizerEngine.Config.Modules.History.Height = Convert.ToInt32(Height);

                RandomizerEngine.SaveConfig();
            };

            Deactivated += (_, _) => { Topmost = false; Topmost = true; };
        }
    }
}
