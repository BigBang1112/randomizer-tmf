using Avalonia.Platform;
using Avalonia;
using RandomizerTMF.Logic;
using Avalonia.Media.Imaging;
using TmEssentials;
using Avalonia.Media;

namespace RandomizerTMF.Models;

public class PlayedMapModel
{
    private static readonly Dictionary<string, Bitmap> envBitmaps = new();

    public string MapName { get; }
    public Bitmap? EnvIconBitmap { get; }
    public SessionMap Map { get; }
    public EResult Result { get; }
    
    public string? TimestampText => Map.LastTimestamp?.ToString("h':'mm':'ss");
    
    public IBrush? TimestampColor => Result switch
    {
        EResult.AuthorMedal => Brushes.Green,
        EResult.GoldMedal => Brushes.Gold,
        EResult.Skipped => Brushes.Orange,
        _ => null
    };

    public PlayedMapModel(SessionMap map, EResult result)
    {
        Map = map;
        Result = result;
        MapName = TextFormatter.Deformat(map.Map.MapName).Trim();

        string env = map.Map.Collection;

        if (!envBitmaps.TryGetValue(env, out Bitmap? bitmap))
        {
            var loader = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
            var asset = loader.Open(new Uri($"avares://RandomizerTMF/Assets/Images/Env/{env}.png"));
            bitmap = new Bitmap(asset);
            envBitmaps.Add(env, bitmap);
        }

        EnvIconBitmap = bitmap;
    }
}
