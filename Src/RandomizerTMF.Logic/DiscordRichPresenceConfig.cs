using YamlDotNet.Serialization;

namespace RandomizerTMF.Logic;

public class DiscordRichPresenceConfig
{
    [YamlMember(Description = "Disable Discord Rich Presence entirely.")]
    public bool Disable { get; set; }

    [YamlMember(Description = "Disable map thumbnail in Discord Rich Presence, questionmark icon will be used instead.")]
    public bool DisableMapThumbnail { get; set; }
}
