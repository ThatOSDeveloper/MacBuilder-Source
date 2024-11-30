using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using MacBuilder_GUI.Core.Components.Assets.Dialogs;
using MacBuilder_GUI.Core.Components.Core.FileManager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Extras.Updater
{
    public class UpdaterClass
    {
        private static readonly string GitHubRepoOwner = "KivieDev";
        private static readonly string GitHubRepoName = "MacBuilder";
        private static readonly string CurrentVersion = Global.Version;

        public static async Task<bool> IsUpdateAvailableAsync()
        {
            string latestVersion = await GetLatestVersionFromGitHubAsync();

            if (latestVersion == null)
            {
                Logger.Log("Failed to retrieve the latest version from GitHub.", Logger.LogLevel.Error);
                return false;
            }

            bool isUpdateAvailable = string.Compare(CurrentVersion, latestVersion, StringComparison.Ordinal) < 0;

            if (isUpdateAvailable)
            {
                Logger.Log($"Update available. Current version: {CurrentVersion}, Latest version: {latestVersion}.", Logger.LogLevel.Info);
                bool update = await DialogClass.ShowYesNoDialogAsync("Update Available", $"MacBuilder {latestVersion} is now available would you like to update now?");
                if (update)
                {
                    await DownloadLatestReleaseAsync(latestVersion);

                }
            }
            else
            {
                Logger.Log($"No updates available. Current version: {CurrentVersion} is up-to-date.", Logger.LogLevel.Info);
            }

            return isUpdateAvailable;
        }

        private static async Task<string> GetLatestVersionFromGitHubAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = $"https://api.github.com/repos/{GitHubRepoOwner}/{GitHubRepoName}/releases";
                    client.DefaultRequestHeaders.Add("User-Agent", "MacBuilder");

                    Logger.Log($"Fetching all releases from GitHub URL: {apiUrl}", Logger.LogLevel.Info);

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Log($"Error: GitHub API returned status code {response.StatusCode}.", Logger.LogLevel.Error);
                        return null;
                    }

                    string responseBody = await response.Content.ReadAsStringAsync();
                    JArray releases = JArray.Parse(responseBody);

                    if (!releases.Any())
                    {
                        Logger.Log("No releases found for this repository.", Logger.LogLevel.Warning);
                        return null;
                    }

                    var latestRelease = releases.FirstOrDefault();
                    string latestVersion = latestRelease?["name"]?.ToString();

                    Logger.Log($"Latest version (including pre-releases) retrieved from GitHub: {latestVersion}", Logger.LogLevel.Info);
                    return latestVersion;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error fetching latest release from GitHub: {ex.Message}", Logger.LogLevel.Error);
                return null;
            }
        }

        private static async Task DownloadLatestReleaseAsync(string version)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = $"https://api.github.com/repos/{GitHubRepoOwner}/{GitHubRepoName}/releases";
                    client.DefaultRequestHeaders.Add("User-Agent", "MacBuilder");

                    Logger.Log($"Fetching all releases (including pre-releases) from GitHub at {apiUrl}", Logger.LogLevel.Info);

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Log($"Error: GitHub API returned status code {response.StatusCode}.", Logger.LogLevel.Error);
                        return;
                    }

                    string responseBody = await response.Content.ReadAsStringAsync();
                    JArray releases = JArray.Parse(responseBody);

                    if (!releases.Any())
                    {
                        Logger.Log("No releases found for this repository.", Logger.LogLevel.Warning);
                        return;
                    }

                    var latestRelease = releases.FirstOrDefault();
                    string latestVersion = latestRelease?["tag_name"]?.ToString();

                    var zipAsset = latestRelease["assets"]?.FirstOrDefault();
                    if (zipAsset == null)
                    {
                        Logger.Log("No assets found for this release.", Logger.LogLevel.Warning);
                        return;
                    }

                    string downloadUrl = zipAsset["browser_download_url"]?.ToString();
                    string assetName = zipAsset["name"]?.ToString();

                    Logger.Log($"Found asset to download: {assetName}", Logger.LogLevel.Info);

                    if (string.IsNullOrWhiteSpace(downloadUrl))
                    {
                        Logger.Log("Download URL not found.", Logger.LogLevel.Error);
                        return;
                    }

                    string downloadFileName = $"{version}-release.zip";
                    string filePath = Path.Combine(Global.DownloadPath, downloadFileName);
                    string extractedPath = Path.Combine(Global.DownloadPath, "MacBuilder");

                    Logger.Log($"Downloading release ZIP to {filePath}", Logger.LogLevel.Info);

                    using (HttpResponseMessage zipResponse = await client.GetAsync(downloadUrl))
                    {
                        zipResponse.EnsureSuccessStatusCode();
                        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await zipResponse.Content.CopyToAsync(fileStream);
                        }
                    }
                    FileManagerClass.ExtractZipFile(filePath);

                    Logger.Log($"Download complete: {filePath}", Logger.LogLevel.Info);

                    if (File.Exists(Path.Combine(extractedPath, "MacBuilder-GUI.exe")))
                    {
                        try
                        {
                            Process proc = new Process();
                            proc.StartInfo.FileName = Path.Combine(extractedPath, "MacBuilder-GUI.exe");
                            proc.Start();
                        } catch (Exception ex)
                        {
                            Logger.Log($"Failed to launch updated program {ex.Message}", Logger.LogLevel.Error);
                            DialogClass.MessageBox("Launch Failed", "Failed to launch updated MacBuilder reach out to support if issue persists.");
                            return;
                        }

                        Environment.Exit(1);
                    } else
                    {
                        Logger.Log($"Failed to launch update {extractedPath}.");
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error downloading the latest release ZIP: {ex.Message}", Logger.LogLevel.Error);
            }
        }
    }
}
