using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MacBuilder_GUI.Core.Components.Assets.Dialogs;
using MacBuilder.Core.Logger;
using Microsoft.UI.Xaml;

namespace MacBuilder_GUI
{
    public sealed partial class USBPartitionPage : Page
    {
        public USBPartitionPage()
        {
            this.InitializeComponent();
            LoadDrives();
        }

        private void LoadDrives()
        {
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (drive.DriveType == DriveType.Fixed) // Only show fixed drives
                {
                    DriveList.Items.Add(new DriveInfoModel
                    {
                        Name = drive.Name,
                        Size = $"{drive.TotalSize / (1024 * 1024 * 1024)} GB"
                    });
                }
            }
        }

        private void DriveList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDrive = DriveList.SelectedItem as DriveInfoModel;
            if (selectedDrive != null)
            {
                LoadPartitions(selectedDrive.Name);
            }
        }

        private void LoadPartitions(string driveLetter)
        {
            driveLetter = driveLetter.TrimEnd('\\');
            Logger.Log($"Loading partitions for {driveLetter}...", Logger.LogLevel.Debug);
            PartitionList.Items.Clear();
            var partitions = GetPartitionsForDrive(driveLetter);
            foreach (var partition in partitions)
            {
                PartitionList.Items.Add(partition);
            }
        }

        private async Task<bool> PartitionDriveAsync(string driveLetter, int partitionSize)
        {
            try
            {
                string driveNumber = await GetDriveNumberFromLetterAsync(driveLetter);

                if (string.IsNullOrEmpty(driveNumber))
                {
                    Logger.Log($"Drive number is null or empty.", Logger.LogLevel.Error);
                    return false;
                }

                var partitions = GetPartitionsForDrive(driveLetter);
                var primaryPartition = partitions.FirstOrDefault(p => p.PartitionName.Contains("Primary"));

                if (primaryPartition == null)
                {
                    Logger.Log("No primary partition found on this drive.", Logger.LogLevel.Error);
                    return false;
                }

                string scriptPath = Path.GetTempFileName();
                string scriptContent = $@"
                select disk {driveNumber}
                select partition {primaryPartition.PartitionName} 
                extend size={partitionSize} 
                create partition primary size={partitionSize * 1024}
                format fs=ntfs quick
                assign letter={driveLetter}";

                await File.WriteAllTextAsync(scriptPath, scriptContent);

                bool success = await RunAsAdministratorAsync($"diskpart /s \"{scriptPath}\"");

                //File.Delete(scriptPath);

                return success;
            }
            catch (Exception ex)
            {
                Logger.Log($"PartitionDriveAsync Error: {ex.Message}", Logger.LogLevel.Error);
                return false;
            }
        }

        private async Task<string> GetDriveNumberFromLetterAsync(string driveLetter)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c wmic logicaldisk get deviceid, name",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                Logger.Log($"WMIC Output:\n{output}", Logger.LogLevel.Debug);

                string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines.Skip(1)) // Skip header line
                {
                    Logger.Log($"Processing Line: {line}", Logger.LogLevel.Debug);

                    string[] columns = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (columns.Length >= 2)
                    {
                        Logger.Log($"DeviceID: {columns[0]}, Name: {columns[1]}", Logger.LogLevel.Debug);
                        Logger.Log($"Comparing: '{columns[1]}' with '{driveLetter}'", Logger.LogLevel.Debug);
                        Logger.Log($"Length of columns[1]: {columns[1].Length}, Length of driveLetter: {driveLetter.Length}", Logger.LogLevel.Debug);

                        driveLetter = driveLetter.TrimEnd('\\');

                        if (columns[1].Trim().Equals(driveLetter, StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Log($"Match found: DeviceID {columns[0]} for Drive {columns[1]}", Logger.LogLevel.Debug);
                            return columns[0]; // Device ID
                        }
                    }
                    else
                    {
                        Logger.Log($"Skipping Line: {line} (Invalid column count)", Logger.LogLevel.Debug);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"GetDriveNumberFromLetterAsync Error: {ex.Message}", Logger.LogLevel.Error);
            }

            Logger.Log("Drive number is null or empty.", Logger.LogLevel.Error);
            return null;
        }

        private async Task<bool> RunAsAdministratorAsync(string command)
        {
            try
            {
                ProcessStartInfo procInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    Verb = "runas" // Doesnt work!!
                };

                using (Process proc = Process.Start(procInfo))
                {
                    if (proc == null)
                        return false;

                    await proc.WaitForExitAsync();
                    return proc.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"RunAsAdministratorAsync Error: {ex.Message}", Logger.LogLevel.Error);
                return false;
            }
        }

        private void PartitionSizeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DriveList.SelectedItem != null && int.TryParse(PartitionSizeInput.Text, out int size) && size > 0)
            {
                CreatePartition.IsEnabled = true;
            }
            else
            {
                CreatePartition.IsEnabled = false;
            }
        }

        private async void CreatePartition_Click(object sender, RoutedEventArgs e)
        {
            var selectedDrive = DriveList.SelectedItem as DriveInfoModel;
            if (selectedDrive == null || !int.TryParse(PartitionSizeInput.Text, out int partitionSize))
            {
                DialogClass.MessageBox("Invalid Input", "Please select a drive and enter a valid partition size.");
                return;
            }

            bool result = await DialogClass.ShowYesNoDialogAsync
            (
                "Confirm Partitioning",
                $"This will create a new partition of {partitionSize} GB on {selectedDrive.Name}. Data on the existing partition may be lost. Continue?"
            );

            if (result)
            {
                bool success = await PartitionDriveAsync(selectedDrive.Name, partitionSize);

                if (success)
                {
                    Logger.Log("The new partition was created successfully.");
                    LoadPartitions(selectedDrive.Name); // Reload partitions
                }
                else
                {
                    DialogClass.MessageBox("Error", "Failed to create partition. Please try again.");
                }
            }
        }
        private PartitionInfo[] GetPartitionsForDrive(string driveLetter)
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c wmic partition where \"DeviceID like '{driveLetter}%'\" get name, size",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string errorOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Logger.Log($"WMIC Partition Output:\n{output}", Logger.LogLevel.Debug);
                Logger.Log($"WMIC Partition Error Output:\n{errorOutput}", Logger.LogLevel.Error);

                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length <= 1)
                {
                    Logger.Log("No partition information found for the drive.", Logger.LogLevel.Error);
                    return Array.Empty<PartitionInfo>();
                }

                var partitionInfoList = lines.Skip(1) // Skip header line
                                             .Where(line => !string.IsNullOrWhiteSpace(line))
                                             .Select(line =>
                                             {
                                                 var columns = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                                 if (columns.Length >= 2) // Ensure there are at least 2 columns (name, size)
                                                 {
                                                     Logger.Log($"Parsing partition: {columns[0]} with size {columns[1]}", Logger.LogLevel.Debug);
                                                     return new PartitionInfo
                                                     {
                                                         PartitionName = columns[0].Trim(),
                                                         PartitionSize = $"{long.Parse(columns[1]) / (1024 * 1024 * 1024)} GB" // Convert size to GB
                                                     };
                                                 }
                                                 else
                                                 {
                                                     Logger.Log($"Skipping line due to insufficient columns: {line}", Logger.LogLevel.Debug);
                                                     return null;
                                                 }
                                             })
                                             .Where(partition => partition != null) // Remove any null entries
                                             .ToArray();

                if (partitionInfoList.Length == 0)
                {
                    Logger.Log("No valid partitions found.", Logger.LogLevel.Error);
                }
                else
                {
                    Logger.Log($"Found {partitionInfoList.Length} partitions.", Logger.LogLevel.Debug);
                }

                return partitionInfoList;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GetPartitionsForDrive: {ex.Message}", Logger.LogLevel.Error);
                return Array.Empty<PartitionInfo>();
            }
        }

        private class DriveInfoModel
        {
            public string Name { get; set; }
            public string Size { get; set; }
        }

        private class PartitionInfo
        {
            public string PartitionName { get; set; }
            public string PartitionSize { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(typeof(BaseSelector));
        }
    }
}
