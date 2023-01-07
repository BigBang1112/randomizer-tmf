using DiscordRPC;
using System.Text;

namespace RandomizerTMF.Logic.Services;

public interface IDiscordRichPresence
{
    void Configuring();
    void Idle();
    void InDashboard();
    void SessionStart(DateTime start);
    void SessionPredictEnd(DateTime end);
    void SessionMap(string mapName, string imageUrl, string env);
    void SessionDetails(string details);
    void AddToSessionPredictEnd(TimeSpan pausedTime);
    void SessionState(int atCount = 0, int goldCount = 0, int skipCount = 0);
    void SessionDefaultAsset();
}

internal class DiscordRichPresence : IDiscordRichPresence, IDisposable
{    
    private readonly DiscordRpcClient client;

    public DiscordRichPresence(DiscordRpcLogger discordLogger)
    {
        client = new DiscordRpcClient("1048435107494637618", logger: discordLogger);
        client.Initialize();
    }

    public void InDashboard()
    {
        Default("In dashboard");
    }

    public void Configuring()
    {
        Default("Configuring the app");
    }

    public void Idle()
    {
        Default("Idle");
    }

    public void SessionDetails(string details)
    {
        client.UpdateDetails(details);
    }

    public void SessionStart(DateTime start)
    {
        client.UpdateStartTime(start);
    }

    public void SessionPredictEnd(DateTime end)
    {
        client.UpdateEndTime(end);
    }

    public void AddToSessionPredictEnd(TimeSpan addition)
    {
        if (client.CurrentPresence.Timestamps.End.HasValue)
        {
            SessionPredictEnd(client.CurrentPresence.Timestamps.End.Value + addition);
        }
    }

    public void SessionMap(string mapName, string imageUrl, string env)
    {
        var envRemap = env.ToLower();

        switch (envRemap)
        {
            case "snow": envRemap = "alpine"; break;
            case "speed": envRemap = "desert"; break;
        }

        client.UpdateLargeAsset(imageUrl, mapName);
        client.UpdateSmallAsset(envRemap, env);
    }

    public void SessionState(int atCount = 0, int goldCount = 0, int skipCount = 0)
    {
        client.UpdateState(BuildState(atCount, goldCount, skipCount));
    }

    internal static string BuildState(int atCount, int goldCount, int skipCount)
    {
        var builder = new StringBuilder();

        builder.Append(atCount);
        builder.Append(" AT");

        if (atCount != 1)
        {
            builder.Append('s');
        }

        builder.Append(", ");
        builder.Append(goldCount);
        builder.Append(" gold");

        if (goldCount != 1)
        {
            builder.Append('s');
        }

        builder.Append(", ");
        builder.Append(skipCount);
        builder.Append(" skip");

        if (skipCount != 1)
        {
            builder.Append('s');
        }

        return builder.ToString();
    }

    public void SessionDefaultAsset()
    {
        client.UpdateLargeAsset("primary", "");
        client.UpdateSmallAsset();
    }

    private void Default(string details)
    {
        client.SetPresence(new RichPresence()
        {
            Details = details,
            Timestamps = new Timestamps(DateTime.UtcNow),
            Assets = new Assets()
            {
                LargeImageKey = "primary"
            }
        });
    }

    public void Dispose()
    {
        client.Deinitialize();
        client.ClearPresence();
        client.Dispose();
    }
}
