using Avalonia.Platform;
using Avalonia;
using RandomizerTMF.Logic;
using Avalonia.Media.Imaging;

namespace RandomizerTMF.Models;

public class AutosaveModel
{
    private static Dictionary<string, Bitmap> envBitmaps = new();

    public string MapUid { get; }
    public AutosaveDetails Autosave { get; }

    public string Text { get; }
    public Bitmap? EnvIconBitmap { get; }

    public AutosaveModel(string mapUid, AutosaveDetails autosave)
    {
        MapUid = mapUid;
        Autosave = autosave;

        Text = autosave.MapName ?? MapUid;

        if (autosave.MapEnvironment is not null)
        {
            if (envBitmaps.TryGetValue(autosave.MapEnvironment, out Bitmap? bitmap))
            {
                EnvIconBitmap = bitmap;
            }
            else
            {
                var loader = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
                var asset = loader.Open(new Uri($"avares://RandomizerTMF/Assets/Images/Env/{autosave.MapEnvironment}.png"));
                bitmap = new Bitmap(asset);
                envBitmaps.Add(autosave.MapEnvironment, bitmap);
            }
        }
    }
}
