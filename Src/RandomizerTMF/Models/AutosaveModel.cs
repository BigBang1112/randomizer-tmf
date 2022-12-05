using Avalonia.Platform;
using Avalonia;
using RandomizerTMF.Logic;
using Avalonia.Media.Imaging;

namespace RandomizerTMF.Models;

public class AutosaveModel
{
    private static readonly Dictionary<string, Bitmap> envBitmaps = new();
    private static readonly Dictionary<string, Bitmap> carBitmaps = new();

    public string MapUid { get; }
    public AutosaveDetails Autosave { get; }

    public string Text { get; }
    public Bitmap? EnvIconBitmap { get; }
    public Bitmap? CarIconBitmap { get; }

    public AutosaveModel(string mapUid, AutosaveDetails autosave)
    {
        MapUid = mapUid;
        Autosave = autosave;

        Text = autosave.MapName ?? MapUid;

        if (autosave.MapEnvironment is not null)
        {
            try
            {
                if (!envBitmaps.TryGetValue(autosave.MapEnvironment, out Bitmap? bitmap))
                {
                    var loader = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
                    var asset = loader.Open(new Uri($"avares://RandomizerTMF/Assets/Images/Env/{autosave.MapEnvironment}.png"));
                    bitmap = new Bitmap(asset);
                    envBitmaps.Add(autosave.MapEnvironment, bitmap);
                }

                EnvIconBitmap = bitmap;
            }
            catch
            {
                // probably log something?
            }
        }

        if (autosave.MapCar is not null)
        {
            try
            {
                if (!carBitmaps.TryGetValue(autosave.MapCar, out Bitmap? bitmap))
                {
                    var loader = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
                    var asset = loader.Open(new Uri($"avares://RandomizerTMF/Assets/Images/Vehicles/{autosave.MapCar}.png"));
                    bitmap = new Bitmap(asset);
                    carBitmaps.Add(autosave.MapCar, bitmap);
                }

                CarIconBitmap = bitmap;
            }
            catch
            {
                // probably log something?
            }
        }
    }
}
