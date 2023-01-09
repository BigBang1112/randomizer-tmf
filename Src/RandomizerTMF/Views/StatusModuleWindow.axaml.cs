using Avalonia.Controls;
using RandomizerTMF.Logic;
using RandomizerTMF.ViewModels;

namespace RandomizerTMF.Views
{
    public partial class StatusModuleWindow : Window
    {
        public StatusModuleWindow()
        {
            InitializeComponent();
            
            Closing += (_, _) => (DataContext as ModuleWindowViewModelBase)?
                .OnClosing(x => x.Status, Position.X, Position.Y, Width, Height);

            Deactivated += (_, _) => { Topmost = false; Topmost = true; };
        }
    }
}
