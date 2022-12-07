using System.Text.Json.Serialization;

namespace RandomizerTMF.Models;

internal class ReleaseModel
{
    [JsonPropertyName("tag_name")]
    public required string TagName { get; init; }
    public required bool Prerelease { get; init; }
}
