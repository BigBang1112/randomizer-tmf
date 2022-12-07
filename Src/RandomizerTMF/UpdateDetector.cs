using Microsoft.Extensions.Logging;
using RandomizerTMF.Logic;
using RandomizerTMF.Models;
using System.Net.Http.Json;

namespace RandomizerTMF;

internal static class UpdateDetector
{
    public static string? UpdateCheckResult { get; private set; }
    public static bool IsNewUpdate { get; private set; }
    public static event Action? UpdateChecked;

    public static async void RequestNewUpdateAsync()
    {
        try
        {
            // Handle rate limiting better
            var response = await RandomizerEngine.Http.GetAsync("https://api.github.com/repos/bigbang1112/randomizer-tmf/releases");

            response.EnsureSuccessStatusCode();

            var releases = await response.Content.ReadFromJsonAsync<ReleaseModel[]>();

            if (releases is null || releases.Length == 0)
            {
                UpdateCheckResult = "No releases";
                return;
            }

            var release = releases[0];
            var version = release.TagName[1..];

            if (string.Equals(version, Program.Version))
            {
                UpdateCheckResult = "Randomizer TMF is up to date";
            }
            else
            {
                UpdateCheckResult = $"Update Randomizer TMF to {version}";
                IsNewUpdate = true;
            }
        }
        catch (HttpRequestException ex)
        {
            UpdateCheckResult = ex.StatusCode is null
                ? "Unknown HTTP error"
                : $"HTTP error {(int)ex.StatusCode}";
        }
        catch (Exception ex)
        {
            RandomizerEngine.Logger.LogError(ex, "Exception when requesting an update.");
            UpdateCheckResult = ex.GetType().Name;
        }
        finally
        {
            UpdateChecked?.Invoke();
        }
    }
}
