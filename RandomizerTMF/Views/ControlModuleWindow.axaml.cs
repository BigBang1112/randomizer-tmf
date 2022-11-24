using Avalonia.Controls;
using RandomizerTMF.Logic;

namespace RandomizerTMF.Views
{
    public partial class ControlModuleWindow : Window
    {
        public ControlModuleWindow()
        {
            InitializeComponent();
            
            Closing += (_, _) =>
            {
                RandomizerEngine.Config.Modules.Control.X = Convert.ToInt32(Position.X);
                RandomizerEngine.Config.Modules.Control.Y = Convert.ToInt32(Position.Y);
                RandomizerEngine.Config.Modules.Control.Width = Convert.ToInt32(Width);
                RandomizerEngine.Config.Modules.Control.Height = Convert.ToInt32(Height);

                RandomizerEngine.SaveConfig();
            };
        }
    }
}
