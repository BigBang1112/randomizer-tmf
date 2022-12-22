using System.Collections.Concurrent;

namespace RandomizerTMF.Logic;

public static class AutosaveScanner
{
    private static bool hasAutosavesScanned;
    
    /// <summary>
    /// If the map UIDs of the autosaves have been fully stored to the <see cref="AutosaveHeaders"/> and <see cref="AutosavePaths"/> dictionaries.
    /// This property is required to be true in order to start a new session. It also handles the state of <see cref="AutosaveWatcher"/>, to catch other autosaves that might be created while the program is running.
    /// </summary>
    public static bool HasAutosavesScanned
    {
        get => hasAutosavesScanned;
        private set
        {
            hasAutosavesScanned = value;
            AutosaveWatcher.EnableRaisingEvents = value;
        }
    }

    public static ConcurrentDictionary<string, AutosaveHeader> AutosaveHeaders { get; } = new();
    public static ConcurrentDictionary<string, AutosaveDetails> AutosaveDetails { get; } = new();

    public static FileSystemWatcher AutosaveWatcher { get; }

}
