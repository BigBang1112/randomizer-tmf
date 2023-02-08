using Microsoft.Extensions.Logging;
using RandomizerTMF.Models;
using System.Net.Http.Json;

namespace RandomizerTMF;

internal interface IUpdateDetector
{
    bool IsNewUpdate { get; }
    string? UpdateCheckResult { get; }

    event Action? UpdateChecked;

    void RequestNewUpdateAsync();
}

internal class UpdateDetector : IUpdateDetector
{
    private readonly HttpClient http;
    private readonly ILogger logger;

    public string? UpdateCheckResult { get; private set; }
    public bool IsNewUpdate { get; private set; }
    public event Action? UpdateChecked;

    public UpdateDetector(HttpClient http, ILogger logger)
    {
        this.http = http;
        this.logger = logger;
    }

    public async void RequestNewUpdateAsync()
    {
        try
        {
            // Handle rate limiting better
            var response = await http.GetAsync("https://api.github.com/repos/bigbang1112/randomizer-tmf/releases");

            response.EnsureSuccessStatusCode();

            var releases = await response.Content.ReadFromJsonAsync<ReleaseModel[]>();

            if (releases is null || releases.Length == 0)
            {
                UpdateCheckResult = "No releases";
                return;
            }

            var release = releases.First(r => !r.Prerelease);
            var version = release.TagName[1..]; // stips v from vX.X.X

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
            logger.LogError(ex, "Exception when requesting an update.");
            UpdateCheckResult = ex.GetType().Name;
        }
        finally
        {
            UpdateChecked?.Invoke();
        }
    }
}
