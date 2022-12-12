using Avalonia.Platform;
using Avalonia;
using RandomizerTMF.Logic;
using Avalonia.Media.Imaging;
using TmEssentials;
using Avalonia.Media;
using System.Diagnostics;

namespace RandomizerTMF.Models;

public class SessionDataMapModel
{    
    public SessionDataMap Map { get; }
    public Bitmap? EnvIconBitmap { get; }

    public string? TimestampText => Map.LastTimestamp?.ToString("h':'mm':'ss") ?? "-:--:--";

    public IBrush? TimestampColor => Map.Result switch
    {
        Constants.AuthorMedal => Brushes.Green,
        Constants.GoldMedal => Brushes.Gold,
        Constants.Skipped => Brushes.Orange,
        _ => Brushes.White
    };

    public SessionDataMapModel(SessionDataMap map)
    {
        Map = map;

        // EnvIconBitmap could be received from the first replay on the map by parsing async
    }

    public void TmxClick()
    {
        ProcessUtils.OpenUrl(Map.TmxLink);
    }
}
