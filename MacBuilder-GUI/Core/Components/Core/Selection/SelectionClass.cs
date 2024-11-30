using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using MacBuilder_GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Core.Selection
{
    public class SelectionClass
    {
        public static bool Done = false;
        public static async Task RunApplicationAsync(string command)
        {
            try
            {
                MainWindow.Navigate(typeof(Downloading));

                await Task.Delay(1000);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    WorkingDirectory = Global.DownloadPath + "\\OpenCore\\Utilities\\macrecovery",
                    CreateNoWindow = false
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    Logger.Log("Download Complete!");

                    string recoveryBootPath = Global.DownloadPath + "\\OpenCore\\Utilities\\macrecovery\\com.apple.recovery.boot";
                    if (!Directory.Exists(recoveryBootPath))
                    {
                        Logger.Log("Could not find com.apple.recovery.boot. Please report this issue.", Logger.LogLevel.Error);
                        return;
                    }
                    Done = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while running the application: {ex.Message}");
                Logger.Log($"An error occurred: {ex.Message}", Logger.LogLevel.Error);
            }
        }
    }
}