using Avalonia.Controls;
using RandomizerTMF.Logic;
using RandomizerTMF.Models;
using RandomizerTMF.ViewModels;

namespace RandomizerTMF.Views
{
    public partial class HistoryModuleWindow : Window
    {
        public HistoryModuleWindow()
        {
            InitializeComponent();

            Closing += (_, _) => (DataContext as ModuleWindowViewModelBase)?
                .OnClosing(x => x.History, Position.X, Position.Y, Width, Height);

            Deactivated += (_, _) => { Topmost = false; Topmost = true; };
        }

        private void MapsDoubleTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            (DataContext as HistoryModuleWindowViewModel)?.MapDoubleClick(((ListBox)(sender ?? throw new Exception())).SelectedItem as PlayedMapModel);
        }
    }
}
