using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using MacBuilder_GUI.Core.Components.Assets.Dialogs;
using MacBuilder_GUI.Core.Components.Core.Apple.Types.OC;
using MacBuilder_GUI.Core.Components.Core.FileManager;
using MacBuilder_GUI.Core.Components.Core.Selection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MacBuilder_GUI
{
    public sealed partial class Downloading : Page
    {
        public double DownloadProgressValue { get; private set; } = 0;
        public bool ExtractionDone { get; private set; } = false;

        public Downloading()
        {
            this.InitializeComponent();
            StartTrackingProgress();
        }

        private async void StartTrackingProgress()
        {
            string destinationPath = Global.SelectedUSB.RootDirectory.FullName;
            string sourcePath = Path.Combine(Global.DownloadPath, "OpenCore", "Utilities", "macrecovery", "com.apple.recovery.boot");

            // Track download progress until completed
            while (!SelectionClass.Done)
            {
                await Task.Delay(500);
                await UpdateDownloadProgress(10); // Increment progress

                StatusMessage.Text = $"Download in progress... {DownloadProgressValue}%";
            }

            // Proceed to extraction if not skipping
            if (Global.SkipDevStuff)
            {
                ExecuteEFISetup(destinationPath);
            }
            else
            {
                await StartFileExtraction(sourcePath, destinationPath);
            }
        }

        private async Task StartFileExtraction(string sourcePath, string destinationPath)
        {
            await UpdateDownloadProgress(50);
            StatusMessage.Text = $"Extracting files... {DownloadProgressValue}%";

            Logger.Log($"Copying files to USB... {DownloadProgressValue}% please wait, this may take a while.");
            await ExtractFiles(sourcePath, destinationPath);
        }

        private async Task ExtractFiles(string sourcePath, string destinationPath)
        {
            try
            {
                // Validate paths
                if (!Directory.Exists(sourcePath))
                {
                    Logger.Log($"Source directory does not exist: {sourcePath}", Logger.LogLevel.Error);
                    return;
                }

                if (!Directory.Exists(destinationPath))
                {
                    Logger.Log($"Destination directory does not exist: {destinationPath}", Logger.LogLevel.Error);
                    return;
                }

                string newDestinationPath = Path.Combine(destinationPath, "com.apple.recovery.boot");
                if (Directory.Exists(newDestinationPath))
                {
                    Logger.Log($"Destination directory already exists: {newDestinationPath}", Logger.LogLevel.Error);
                    return;
                }

                // Copy files and handle cleanup
                await CopyDirectoryAsync(sourcePath, newDestinationPath);

                FileManagerClass.MoveFile(Path.Combine(Global.DownloadPath, "OpenCore", "Docs", "Sample.plist"), Path.Combine(destinationPath, "EFI", "OC", "config.plist"));

                await UpdateDownloadProgress(25); // Update progress after extraction
                StatusMessage.Text = $"Extraction complete... {DownloadProgressValue}%";

                Logger.Log("Finished copying BaseOS files.");
                ExecuteEFISetup(destinationPath); // Proceed with EFI setup
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during file extraction: {ex.Message}", Logger.LogLevel.Error);
            }
        }

        private async Task CopyDirectoryAsync(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            // Copy files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            // Copy subdirectories recursively
            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(directory));
                await CopyDirectoryAsync(directory, destSubDir);
            }
        }

        public async void ExecuteEFISetup(string destDir)
        {
            Logger.Log("Copying EFI files...");

            string efiSourcePath = Global.hardwareInfo.CPU.Is64Bit
                ? Path.Combine(Global.DownloadPath, "OpenCore", "X64")
                : Path.Combine(Global.DownloadPath, "OpenCore", "IA32");

            await CopyDirectoryAsync(efiSourcePath, destDir);
            Logger.Log("Finished copying EFI files.");

            await UpdateDownloadProgress(75); // Mark as complete
            StatusMessage.Text = $"Cleaning up... {DownloadProgressValue}%";
            Logger.Log("Cleaning up OC files...");
            OCClass.CleanupFiles();

            await UpdateDownloadProgress(100); // Final progress
            StatusMessage.Text = $"Base installation complete! {DownloadProgressValue}%";
            ShowCompletionMessage();
        }

        private async Task UpdateDownloadProgress(double increment)
        {
            DownloadProgressValue += increment;
            DownloadProgressValue = Math.Min(DownloadProgressValue, 100);
            DownloadProgressBar.Value = DownloadProgressValue;
            await Task.Delay(50); // update
        }

        private async void ShowCompletionMessage()
        {
            DialogClass.MessageBox("Note", "MacBuilder beta does not yet support fully personalized builds; the EFI will be built without extra kexts (WiFi, Ethernet, Bluetooth, etc.). In short terms, don't expect all features to work.");
            await Task.Delay(1000);
            MainWindow.Navigate(typeof(EFISetup));
        }
    }
}
