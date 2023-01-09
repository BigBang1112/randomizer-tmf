using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace RandomizerTMF.Logic.Services;

public interface ITMForever
{
    void OpenAutosave(string relativeFileName);
    void OpenFile(string filePath);
}

public class TMForever : ITMForever
{
    private readonly IRandomizerConfig config;
    private readonly IFilePathManager filePathManager;
    private readonly ILogger logger;

    public TMForever(IRandomizerConfig config, IFilePathManager filePathManager, ILogger logger)
    {
        this.config = config;
        this.filePathManager = filePathManager;
        this.logger = logger;
    }

    public void OpenFile(string filePath)
    {
        if (config.GameDirectory is null)
        {
            throw new Exception("Game directory is null");
        }

        logger.LogInformation("Opening {filePath} in TMForever...", filePath);

        var startInfo = new ProcessStartInfo(Path.Combine(config.GameDirectory, Constants.TmForeverExe), $"/useexedir /singleinst /file=\"{filePath}\"")
        {

        };

        var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        try
        {
            process.WaitForInputIdle();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Could not wait for input.");
        }
    }

    /// <summary>
    /// Opens the autosave ingame.
    /// </summary>
    /// <param name="relativeFileName">File name relative to the Autosaves folder.</param>
    public void OpenAutosave(string relativeFileName)
    {
        if (filePathManager.AutosavesDirectoryPath is null)
        {
            throw new Exception("Cannot open an autosave ingame without a valid user data directory path.");
        }

        OpenFile(Path.Combine(filePathManager.AutosavesDirectoryPath, relativeFileName));
    }
}
