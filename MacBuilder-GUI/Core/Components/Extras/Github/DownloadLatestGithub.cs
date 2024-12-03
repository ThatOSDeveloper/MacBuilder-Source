using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MacBuilder.Core.Logger;

namespace MacBuilder_GUI.Core.Components.Extras.Github
{
    public class DownloadLatestGithub
    {
        private static readonly HttpClient client = new HttpClient();

        // Fetch the latest release sorted by date
        public static async Task DownloadAsync(string url, string downloadDirectory)
        {
            try
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

                var apiUrl = $"{url}/releases";
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var latestRelease = doc.RootElement.EnumerateArray()
                        .OrderByDescending(element => element.GetProperty("published_at").GetString())
                        .FirstOrDefault();

                    if (latestRelease.ValueKind == JsonValueKind.Undefined)
                    {
                        Logger.Log("No releases found.", Logger.LogLevel.Error);
                        return;
                    }

                    var assets = latestRelease.GetProperty("assets").EnumerateArray().ToList();

                    if (!assets.Any())
                    {
                        Logger.Log("No assets found in the latest release.", Logger.LogLevel.Error);
                        return;
                    }

                    var asset = assets.First();
                    var assetUrl = asset.GetProperty("browser_download_url").GetString();
                    var assetName = asset.GetProperty("name").GetString();

                    if (string.IsNullOrEmpty(assetUrl) || string.IsNullOrEmpty(assetName))
                    {
                        Logger.Log("Asset URL or name is missing.", Logger.LogLevel.Error);
                        return;
                    }

                    Logger.Log($"Downloading {assetName}...");

                    var filePath = Path.Combine(downloadDirectory, assetName);
                    var fileBytes = await client.GetByteArrayAsync(assetUrl);

                    await File.WriteAllBytesAsync(filePath, fileBytes);

                    Logger.Log($"Download completed. File saved to {filePath}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred: {ex.Message}", Logger.LogLevel.Error);
            }
        }
    }
}
