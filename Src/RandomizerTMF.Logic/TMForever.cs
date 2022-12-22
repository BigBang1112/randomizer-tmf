using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace RandomizerTMF.Logic;

public class TMForever
{
    private readonly RandomizerConfig config;
    private readonly ILogger logger;

    public TMForever(RandomizerConfig config, ILogger logger)
    {
        this.config = config;
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

    public void OpenAutosave(string fileName)
    {
        if (FilePathManager.AutosavesDirectoryPath is null)
        {
            throw new Exception("Cannot open an autosave ingame without a valid user data directory path.");
        }

        OpenFile(Path.Combine(FilePathManager.AutosavesDirectoryPath, fileName));
    }
}
