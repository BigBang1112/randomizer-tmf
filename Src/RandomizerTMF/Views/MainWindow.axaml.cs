using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using RandomizerTMF.ViewModels;

namespace RandomizerTMF.Views;

internal partial class MainWindow : ReactiveWindow<MainWindowViewModel>, IStyleable
{
    public MainWindow()
    {
        InitializeComponent();
    }
}