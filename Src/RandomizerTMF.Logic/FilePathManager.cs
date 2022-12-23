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

    public static string? TmForeverExeFilePath { get; private set; }
    public static string? TmUnlimiterExeFilePath { get; private set; }

    public static string ClearFileName(string fileName)
    {
        return string.Join('_', fileName.Split(Path.GetInvalidFileNameChars()));
    }

    public static GameDirInspectResult UpdateGameDirectory(string gameDirectoryPath)
    {
        RandomizerEngine.Config.GameDirectory = gameDirectoryPath;

        var nadeoIniFilePath = Path.Combine(gameDirectoryPath, Constants.NadeoIni);
        var tmForeverExeFilePath = Path.Combine(gameDirectoryPath, Constants.TmForeverExe);
        var tmUnlimiterExeFilePath = Path.Combine(gameDirectoryPath, Constants.TmInifinityExe);

        var nadeoIniException = default(Exception);
        var tmForeverExeException = default(Exception);
        var tmUnlimiterExeException = default(Exception);

        try
        {
            var nadeoIni = NadeoIni.Parse(nadeoIniFilePath);
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var newUserDataDirectoryPath = Path.Combine(myDocuments, nadeoIni.UserSubDir);

            if (UserDataDirectoryPath != newUserDataDirectoryPath)
            {
                UserDataDirectoryPath = newUserDataDirectoryPath;

                AutosaveScanner.ResetAutosaves();
            }
        }
        catch (Exception ex)
        {
            nadeoIniException = ex;
        }

        try
        {
            using var fs = File.OpenRead(tmForeverExeFilePath);
            TmForeverExeFilePath = tmForeverExeFilePath;
        }
        catch (Exception ex)
        {
            tmForeverExeException = ex;
        }

        try
        {
            using var fs = File.OpenRead(tmUnlimiterExeFilePath);
            TmUnlimiterExeFilePath = tmUnlimiterExeFilePath;
        }
        catch (Exception ex)
        {
            tmUnlimiterExeException = ex;
        }

        return new GameDirInspectResult(nadeoIniException, tmForeverExeException, tmUnlimiterExeException);
    }
}
