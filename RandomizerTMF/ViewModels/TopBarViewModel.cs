using System;
using System.Diagnostics;

namespace RandomizerTMF.ViewModels;

public class TopBarViewModel : ViewModelBase
{
    public event Action? CloseClick;
    public event Action? MinimizeClick;

    public TopBarViewModel()
    {
        
    }

    public void OnCloseClick()
    {
        CloseClick?.Invoke();
    }

    public void OnMinimizeClick()
    {
        MinimizeClick?.Invoke();
    }

    public void DonateClick()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://paypal.me/bigbang1112",
            UseShellExecute = true
        });
    }
}
