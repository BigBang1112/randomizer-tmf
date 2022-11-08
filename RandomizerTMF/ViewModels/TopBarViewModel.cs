using System;

namespace RandomizerTMF.ViewModels;

public class TopBarViewModel : ViewModelBase
{
    public event Action? CloseClick;

    public TopBarViewModel()
    {
        
    }

    public void OnCloseClick()
    {
        CloseClick?.Invoke();
    }

    public void OnMinimizeClick()
    {
        
    }
}
