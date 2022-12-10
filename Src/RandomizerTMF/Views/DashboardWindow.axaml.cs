using Avalonia.Controls;
using RandomizerTMF.ViewModels;

namespace RandomizerTMF.Views
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
        }

        private void AutosavesDoubleTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            (DataContext as DashboardWindowViewModel)?.AutosaveDoubleClick(((ListBox)(sender ?? throw new Exception())).SelectedIndex);
        }

        private void SessionsDoubleTapped(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            (DataContext as DashboardWindowViewModel)?.SessionDoubleClick(((ListBox)(sender ?? throw new Exception())).SelectedIndex);
        }
    }
}
