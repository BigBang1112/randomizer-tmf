namespace RandomizerTMF.Logic;

public class NadeoIni
{
    public required string UserSubDir { get; init; }

    public static NadeoIni Parse(string nadeoIniFilePath)
    {
        var userSubDir = "TmForever";

        foreach (var line in File.ReadLines(nadeoIniFilePath))
        {
            if (line.Length == 0 || line[0] is '#' or ';' or '[')
            {
                continue;
            }

            if (line.StartsWith("UserSubDir="))
            {
                userSubDir = line[11..];
            }
        }

        return new NadeoIni
        {
            UserSubDir = userSubDir
        };
    }
}
