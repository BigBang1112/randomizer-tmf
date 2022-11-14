using System;

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
}
