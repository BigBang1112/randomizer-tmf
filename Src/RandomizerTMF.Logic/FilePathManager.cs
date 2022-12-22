namespace RandomizerTMF.Logic;

public static class FilePathManager
{
    private static string? userDataDirectoryPath;
    
    /// <summary>
    /// General directory of the user data. It also sets the <see cref="AutosavesDirectoryPath"/>, <see cref="DownloadedDirectoryPath"/>, and <see cref="AutosaveWatcher"/> path with it.
    /// </summary>
    public static string? UserDataDirectoryPath
    {
        get => userDataDirectoryPath;
        set
        {
            userDataDirectoryPath = value;

            AutosavesDirectoryPath = userDataDirectoryPath is null ? null : Path.Combine(userDataDirectoryPath, Constants.Tracks, Constants.Replays, Constants.Autosaves);
            DownloadedDirectoryPath = userDataDirectoryPath is null ? null
                : Path.Combine(userDataDirectoryPath, Constants.Tracks, Constants.Challenges, Constants.Downloaded, string.IsNullOrWhiteSpace(RandomizerEngine.Config.DownloadedMapsDirectory)
                    ? Constants.DownloadedMapsDirectory
                    : RandomizerEngine.Config.DownloadedMapsDirectory);

            AutosaveScanner.AutosaveWatcher.Path = AutosavesDirectoryPath ?? "";
        }
    }

    public static string? AutosavesDirectoryPath { get; private set; }
    public static string? DownloadedDirectoryPath { get; private set; }
    public const string SessionsDirectoryPath = Constants.Sessions;

    public static string ClearFileName(string fileName)
    {
        return string.Join('_', fileName.Split(Path.GetInvalidFileNameChars()));
    }
}
