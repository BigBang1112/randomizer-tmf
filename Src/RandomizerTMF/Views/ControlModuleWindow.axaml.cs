using Avalonia.Controls;
using RandomizerTMF.Logic;
using RandomizerTMF.ViewModels;

namespace RandomizerTMF.Views
{
    public partial class ControlModuleWindow : Window
    {
        public ControlModuleWindow()
        {
            InitializeComponent();

            Closing += (_, _) => (DataContext as ModuleWindowViewModelBase)?
                .OnClosing(x => x.Control, Position.X, Position.Y, Width, Height);

            Deactivated += (_, _) => { Topmost = false; Topmost = true; };
        }
    }
}
