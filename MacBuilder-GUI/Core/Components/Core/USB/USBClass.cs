using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using MacBuilder_GUI.Core.Components.Core.Selection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Core.USB
{
    public class USBClass
    {
        public static async Task<bool> CheckUSBStorageAsync(DriveInfo selectedDrive)
        {
            const long minimumStorageInBytes = 4L * 1024 * 1024 * 1024; // 4GB in bytes

            Logger.Log($"Checking total storage for USB: {selectedDrive.Name}");

            if (selectedDrive.TotalSize < minimumStorageInBytes)
            {
                Logger.Log($"Error: The selected USB drive '{selectedDrive.Name}' does not have enough total space.");
                Logger.Log($"It has {selectedDrive.TotalSize / (1024 * 1024)}MB total, but 4GB is required.");
                Logger.Log("Please select a USB drive with sufficient space.");
                return false;
            }
            else
            { 
                // Fix skipdevstuff
                if (Global.SkipDevStuff)
                {
                    MainWindow.Navigate(typeof(Downloading));
                    SelectionClass.Done = true;
                    return true;
                }
                if (await FormatUSBDriveAsync(selectedDrive))
                {
                    return true;
                }
            }
            return false;
        }
        public static async Task<bool> FormatUSBDriveAsync(DriveInfo selectedDrive)
        {

            try
            {
                Logger.Log("Formatting USB...");
                string driveLetter = selectedDrive.Name.TrimEnd('\\');

                string formatCommand = $"/C format {driveLetter} /FS:FAT32 /Q /Y"; // Quick format to FAT32

                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = formatCommand;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (string.IsNullOrEmpty(error))
                {
                    Logger.Log("The USB drive has been successfully formatted.");

                    if (selectedDrive.IsReady && selectedDrive.DriveType == DriveType.Removable)
                    {
                        try
                        {
                            selectedDrive.VolumeLabel = "MacBuild";
                            return true;
                        }
                        catch (Exception renameEx)
                        {
                            Logger.Log($"An error occurred while renaming the USB drive: {renameEx.Message}", Logger.LogLevel.Error);
                        }
                    }
                    else
                    {
                        Logger.Log("The USB drive is not ready or not writable. Cannot rename.", Logger.LogLevel.Error);
                    }
                }
                else
                {
                    Logger.Log($"An error occurred while formatting the USB drive: {error}", Logger.LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred while formatting the USB drive: {ex.Message}", Logger.LogLevel.Error);
            }
            return false;
        }
    }
}
