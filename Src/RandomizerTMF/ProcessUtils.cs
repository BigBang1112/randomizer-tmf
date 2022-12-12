using System.Diagnostics;

namespace RandomizerTMF;

public static class ProcessUtils
{
    public static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    public static void OpenDir(string dirPath)
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = dirPath,
            UseShellExecute = true,
            Verb = "open"
        });
    }
}
