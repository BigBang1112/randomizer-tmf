using Avalonia.Controls;
using RandomizerTMF.Logic;
using RandomizerTMF.ViewModels;

namespace RandomizerTMF.Views
{
    public partial class ProgressModuleWindow : Window
    {
        public ProgressModuleWindow()
        {
            InitializeComponent();

            Closing += (_, _) => (DataContext as ModuleWindowViewModelBase)?
                .OnClosing(x => x.Progress, Position.X, Position.Y, Width, Height);

            Deactivated += (_, _) => { Topmost = false; Topmost = true; };
        }
    }
}
