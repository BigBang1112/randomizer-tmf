using Avalonia.Media;
using RandomizerTMF.Logic;

namespace RandomizerTMF.Models;

public class SessionDataModel
{
    public SessionData Data { get; }
    
    public int AuthorMedalCount => Data.Maps.Count(m => string.Equals(m.Result, Constants.AuthorMedal));
    public int GoldMedalCount => Data.Maps.Count(m => string.Equals(m.Result, Constants.GoldMedal));
    public int SkippedCount => Data.Maps.Count(m => string.Equals(m.Result, Constants.Skipped));
    
    public IBrush SkippedBrush => SkippedCount == 0 ? Brushes.White : Brushes.Orange;

    public SessionDataModel(SessionData data)
    {
        Data = data;
    }
}
