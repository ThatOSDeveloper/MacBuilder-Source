using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using MacBuilder_GUI.Core.Components.Assets.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Core.Apple.Types.Kexts
{
    public class KextClass
    {
        private static readonly Dictionary<string, string> kextUrls = new Dictionary<string, string>
        {
            { "Lilu.kext", "https://github.com/acidanthera/Lilu/releases/download/1.6.8/Lilu-1.6.8-DEBUG.zip" },
            { "VirtualSMC.kext", "https://github.com/acidanthera/VirtualSMC/releases/download/1.3.3/VirtualSMC-1.3.3-DEBUG.zip" },
            { "NVMeFix.kext", "https://github.com/acidanthera/NVMeFix/releases/download/1.1.1/NVMeFix-1.1.1-DEBUG.zip" },
            { "WhateverGreen.kext", "https://github.com/acidanthera/WhateverGreen/releases/download/1.6.7/WhateverGreen-1.6.7-DEBUG.zip" },
        };

        public static async Task Install(string kext)
        {
            string destinationFolder = Path.Combine(Global.SelectedUSB.RootDirectory.FullName, "EFI", "OC", "Kexts");

            if (kextUrls.TryGetValue(kext, out string kextUrl))
            {
                try
                {
                    if (!Directory.Exists(destinationFolder))
                    {
                        Directory.CreateDirectory(destinationFolder);
                    }

                    string zipFilePath = Path.Combine(destinationFolder, $"{kext}.zip");

                    using (WebClient client = new WebClient())
                    {
                        await client.DownloadFileTaskAsync(kextUrl, zipFilePath);
                        Logger.Log($"Download Success: {kext} downloaded successfully!");

                        string extractPath = Path.Combine(destinationFolder, kext);
                        ZipFile.ExtractToDirectory(zipFilePath, extractPath);

                        string foundPath = FindKextFile(extractPath, kext);

                        if (!string.IsNullOrEmpty(foundPath))
                        {
                            string kextDestPath = Path.Combine(destinationFolder, Path.GetFileName(foundPath));

                            CopyDirectory(foundPath, kextDestPath);

                            Directory.Delete(foundPath, true);
                        }
                        else
                        {
                            Logger.Log($"Could not find kext file for '{kext}' in extracted contents.", Logger.LogLevel.Error);
                        }

                        File.Delete(zipFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to install {kext}. Error: {ex.Message}", Logger.LogLevel.Error);
                }
            }
            else
            {
                Logger.Log($"Could not find specified kext: {kext}", Logger.LogLevel.Error);
            }
        }

        private static string FindKextFile(string rootPath, string kextName)
        {
            string kextFilePath = null;

            try
            {
                foreach (var directory in Directory.GetDirectories(rootPath))
                {
                    if (Path.GetFileName(directory).Equals(kextName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (kextFilePath != null)
                        {
                            continue;
                        }

                        kextFilePath = directory; // Set the found kext file path
                    }

                    string foundPath = FindKextFile(directory, kextName);
                    if (!string.IsNullOrEmpty(foundPath))
                    {
                        if (kextFilePath != null)
                        {
                            continue;
                        }

                        kextFilePath = foundPath; // Update the found kext file path
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to search folder. Error: {ex.Message}", Logger.LogLevel.Error);
            }

            return kextFilePath; // Return the found kext file path (or null if not found)
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);  // Overwrite if exists
            }

            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(directory));
                CopyDirectory(directory, destSubDir);
            }
        }
    }
}
