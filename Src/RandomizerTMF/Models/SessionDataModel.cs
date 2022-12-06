using Avalonia.Media;
using RandomizerTMF.Logic;

namespace RandomizerTMF.Models;

public class SessionDataModel
{
    public SessionData Data { get; }

    public IBrush SkippedBrush => Data.SkippedCount == 0 ? Brushes.White : Brushes.Orange;

    public SessionDataModel(SessionData data)
    {
        Data = data;
    }
}
