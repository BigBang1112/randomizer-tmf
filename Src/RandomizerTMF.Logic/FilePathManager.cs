namespace RandomizerTMF.Logic;

public interface IFilePathManager
{
    string? AutosavesDirectoryPath { get; }
    string? DownloadedDirectoryPath { get; }
    string? TmForeverExeFilePath { get; }
    string? TmUnlimiterExeFilePath { get; }
    string? UserDataDirectoryPath { get; set; }

    event Action UserDataDirectoryPathUpdated;

    GameDirInspectResult UpdateGameDirectory(string gameDirectoryPath);
}

public class FilePathManager : IFilePathManager
{
    private readonly IRandomizerConfig config;
    private string? userDataDirectoryPath;

    /// <summary>
    /// General directory of the user data. It also sets the <see cref="AutosavesDirectoryPath"/>, <see cref="DownloadedDirectoryPath"/>, and <see cref="AutosaveWatcher"/> path with it.
    /// </summary>
    public string? UserDataDirectoryPath
    {
        get => userDataDirectoryPath;
        set
        {
            userDataDirectoryPath = value;

            AutosavesDirectoryPath = userDataDirectoryPath is null ? null : Path.Combine(userDataDirectoryPath, Constants.Tracks, Constants.Replays, Constants.Autosaves);
            DownloadedDirectoryPath = userDataDirectoryPath is null ? null
                : Path.Combine(userDataDirectoryPath, Constants.Tracks, Constants.Challenges, Constants.Downloaded, string.IsNullOrWhiteSpace(config.DownloadedMapsDirectory)
                    ? Constants.DownloadedMapsDirectory
                    : config.DownloadedMapsDirectory);

            UserDataDirectoryPathUpdated?.Invoke();
        }
    }

    public string? AutosavesDirectoryPath { get; private set; }
    public string? DownloadedDirectoryPath { get; private set; }
    public static string SessionsDirectoryPath = Constants.Sessions;

    public string? TmForeverExeFilePath { get; private set; }
    public string? TmUnlimiterExeFilePath { get; private set; }

    public event Action? UserDataDirectoryPathUpdated;

    public FilePathManager(IRandomizerConfig config)
    {
        this.config = config;
    }

    public static string ClearFileName(string fileName)
    {
        return string.Join('_', fileName.Split(Path.GetInvalidFileNameChars()));
    }

    public GameDirInspectResult UpdateGameDirectory(string gameDirectoryPath)
    {
        config.GameDirectory = gameDirectoryPath;

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
