using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Assets.WebClient
{
    public class WebClientClass
    {
        public static void DownloadFile(string url)
        {
            try
            {
                System.Net.WebClient webclient = new System.Net.WebClient();
                webclient.DownloadFile(url, Global.DownloadPath);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Logger.LogLevel.Error);
            }
        }

        public static async Task DownloadLatestOpenCoreDebug()
        {
            string apiUrl = "https://api.github.com/repos/acidanthera/OpenCorePkg/releases";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "MacBuilder");

                try
                {
                    var response = await client.GetStringAsync(apiUrl);
                    var releases = JsonConvert.DeserializeObject<GitHubRelease[]>(response);

                    if (releases != null && releases.Length > 0)
                    {
                        foreach (var release in releases)
                        {
                            var debugZip = release.Assets?.FirstOrDefault(a => a.Name.EndsWith("-DEBUG.zip", StringComparison.OrdinalIgnoreCase));

                            if (debugZip != null)
                            {
                                string zipUrl = debugZip.BrowserDownloadUrl;
                                Logger.Log("Downloading latest OpenCore DEBUG zip...");

                                string filePath = Path.Combine(Global.DownloadPath, "OpenCore-Latest-DEBUG.zip");
                                if (File.Exists(filePath)) { File.Delete(filePath); }
                                var zipResponse = await client.GetAsync(zipUrl);
                                zipResponse.EnsureSuccessStatusCode();

                                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    await zipResponse.Content.CopyToAsync(fs);
                                }
                                if (Directory.Exists(Path.Combine(Global.DownloadPath, "OpenCore")))
                                {
                                    Directory.Delete(Path.Combine(Global.DownloadPath, "OpenCore"), true);
                                }
                                Directory.CreateDirectory(Path.Combine(Global.DownloadPath, "OpenCore"));
                                ZipFile.ExtractToDirectory(filePath, Path.Combine(Global.DownloadPath, "OpenCore"));
                                Logger.Log("Downloaded the latest OpenCore Debug Release!");
                                return;
                            }
                        }

                        Logger.Log("No latest OpenCore DEBUG zip found.", Logger.LogLevel.Error);
                    }
                    else
                    {
                        Logger.Log("Failed to download OpenCore; no releases found.", Logger.LogLevel.Error);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"An error occurred: {ex.Message}");
                }
            }
        }

        public class GitHubRelease
        {
            [JsonProperty("assets")]
            public GitHubAsset[] Assets { get; set; }
        }

        public class GitHubAsset
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("browser_download_url")]
            public string BrowserDownloadUrl { get; set; }
        }
    }
}
