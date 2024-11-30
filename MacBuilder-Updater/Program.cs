using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

internal class Program
{
    public static string MacBuilderpath;
    private static async Task Main(string[] args)
    {
        Console.Title = "MacBuilder Updater";

        string macbuilderpath = Environment.GetEnvironmentVariable("MacBuilder");

        if (macbuilderpath == null)
        {
            Console.WriteLine("MacBuilder environment variable not found, please make sure you installed the software with the installer.");
            Console.ReadKey();
            return;
        }
        else
        {
            if (Directory.Exists(macbuilderpath) && File.Exists(Path.Combine(macbuilderpath, "MacBuilder-GUI.exe")))
            {
                string assemblyPath = Path.Combine(macbuilderpath, "MacBuilder-GUI.dll");
                Assembly assembly = Assembly.LoadFrom(assemblyPath);

                Version localVersion = assembly.GetName().Version;

                string localVersionString = $"mb-{TrimLastVersionPart(localVersion.ToString())}";
                Console.WriteLine($"Local Version: {localVersionString}");

                string latestVersion = await GetLatestGitHubVersionAsync("KivieDev", "MacBuilder");
                MacBuilderpath = macbuilderpath;
                if (latestVersion != null)
                {
                    Console.WriteLine($"Latest GitHub Version: {latestVersion}");

                    if (IsIllegalVersion(localVersionString, latestVersion))
                    {
                        Console.WriteLine("Illegal version detected. Updating to the latest version...");
                        await UpdateToLatestVersion();
                    }
                    else if (latestVersion == localVersionString)
                    {
                        Console.WriteLine("You are up-to-date.");
                    }
                    else if (IsNewerVersion(latestVersion, localVersionString))
                    {
                        Console.WriteLine("A newer version is available. Updating...");
                        await UpdateToLatestVersion();
                    }
                    else
                    {
                        Console.WriteLine("You are using a newer version than the latest GitHub version.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Repairing missing files...");
            }
        }
    }

    private static async Task<string> GetLatestGitHubVersionAsync(string owner, string repo)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                string url = $"https://api.github.com/repos/{owner}/{repo}/releases";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    foreach (JsonElement element in doc.RootElement.EnumerateArray())
                    {
                        string tagName = element.GetProperty("name").GetString();
                        bool isPreRelease = element.GetProperty("prerelease").GetBoolean();

                        if (!string.IsNullOrEmpty(tagName))
                        {
                            return tagName;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching the latest version: {ex.Message}");
        }

        return null;
    }
    private static string TrimLastVersionPart(string version)
    {
        string[] parts = version.Split('.');
        if (parts.Length > 3)
        {
            return string.Join(".", parts[0], parts[1], parts[2]);
        }
        return version;
    }

    private static bool IsNewerVersion(string latestVersion, string localVersion)
    {
        string latestCleaned = latestVersion.Replace("mb-", "");
        string localCleaned = localVersion.Replace("mb-", "");

        Version latest = new Version(latestCleaned);
        Version local = new Version(localCleaned);

        return latest > local;
    }

    private static bool IsIllegalVersion(string localVersion, string latestVersion)
    {
        string latestCleaned = latestVersion.Replace("mb-", "");
        string localCleaned = localVersion.Replace("mb-", "");

        Version latest = new Version(latestCleaned);
        Version local = new Version(localCleaned);

        return local.Major > latest.Major + 1;
    }

    private static async Task UpdateToLatestVersion()
    {
        await Task.Delay(1000);
        // no logic here yet.
        Console.WriteLine("Update completed successfully!");

        Process process = new Process();
        process.StartInfo.FileName = Path.Combine(MacBuilderpath, "MacBuilder-GUI.exe");
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = false;
        process.StartInfo.RedirectStandardError = false;
        process.Start();
        await process.WaitForExitAsync();
    }
}
