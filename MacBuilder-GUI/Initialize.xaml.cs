using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using MacBuilder_GUI.Core.Components.Assets.Computer;
using MacBuilder_GUI.Core.Components.Assets.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using static MacBuilder_GUI.Core.Components.Core.Classes.MacClasses;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MacBuilder_GUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartUp : Page
    {
        public StartUp()
        {
            this.InitializeComponent();
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Log($"Initialized MacBuilder Created/Developed by Kivie. v{Global.Version}");
                Logger.Log("Kivie @ 2024 all rights reserved.");
                if (MainGrid == null)
                {
                    // init failed.
                    return;
                }
                Logger.Log("Pre-Loading UI...");
                Log("Loading UI...");
                // we should calculate the load times to know how long to delay
                // internal exceptions will cause instant crashes
                await Task.Delay(2000); // wait for window to load to avoid exceptions
                Logger.Log("Checking hardware...");

                string mhc = AppContext.BaseDirectory + "\\MHC.exe";
                string mhcdump = Global.GetDownloadPath() + "\\hardware_dump.json";

                if (!File.Exists(mhc))
                {
                    Logger.Log("File not found: " + mhc, Logger.LogLevel.Error);
                    Log("File not found: " + mhc);
                    //DialogClass.MessageBox("Missing Executable", "MacBuilder cannot run without MHC (MacBuilder Hardware Checker). Verify the directory of " + mhc);
                    //return;
                }

                // Start the process to run MHC
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = mhc,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        Arguments = "--silent --dump"
                    }
                };
                //proc.Start(); // Uncomment if you want to start the process

                // Check if the hardware dump file exists
                if (!File.Exists(mhcdump))
                {
                    Logger.Log("Could not find MHC dump file. Contact support.", Logger.LogLevel.Error);
                    Log("Could not find MHC dump file. Contact support.");
                    return;
                }

                // Read the JSON dump file
                string json = File.ReadAllText(mhcdump);

                // Deserialize the JSON into the HardwareInfo object using Json.NET
                HardwareInfo hardwareInfo = JsonConvert.DeserializeObject<HardwareInfo>(json);

                if (hardwareInfo == null)
                {
                    Logger.Log("Failed to deserialize hardware information.", Logger.LogLevel.Error);
                    return;
                }
                Global.hardwareInfo = hardwareInfo;

                // temp
                //bool isDriveSupported = GetComputerSpecs.CheckStorageSupport();
                //bool isNetworkSupported = GetComputerSpecs.CheckNetworkAdapterSupport();
                //bool isWifiSupported = GetComputerSpecs.CheckWifiSupport();



                Logger.Log("Detected CPU: " + hardwareInfo.CPU.Name + " Supported: " + hardwareInfo.CPU.IsCPUSupported);
                Logger.Log("Detected GPU: " + hardwareInfo.GPU.Model + " Supported: " + hardwareInfo.GPU.IsGPUSupported);

                if (/* isDriveSupported && isNetworkSupported && isWifiSupported && */ hardwareInfo.GPU.IsGPUSupported && hardwareInfo.CPU.IsCPUSupported && hardwareInfo.Supported.SupportedMacVersions != null && hardwareInfo.Supported.isUEFISupported)
                {
                    Log("Your PC seems supported. Loading content...");
                    Logger.Log("All checks passed, navigating...");
                    MainWindow.Navigate(typeof(MainMenu));
                }
                else
                {
                    if (!hardwareInfo.Supported.isUEFISupported)
                        Log("UEFI is not supported on this hardware.");
                    if (!hardwareInfo.CPU.IsCPUSupported)
                        Log("Your CPU is not supported.");
                    if (!hardwareInfo.GPU.IsGPUSupported)
                        Log("Your GPU is not supported.");
                    /*
                    if (!isDriveSupported)
                        Log("Your drive is not supported.", Logger.LogLevel.Error);
                    if (!isNetworkSupported)
                        Log("Your network adapter is not supported.", Logger.LogLevel.Warning);
                    if (!isWifiSupported)
                        Log("Your WiFi card is not supported.", Logger.LogLevel.Warning);
                    */
                }
            } catch (Exception ex)
            {
                // will never log unless in log.txt
                Logger.Log("STARTUP EXCEPTION! " + ex, Logger.LogLevel.Error);
            }
        }
        public void Log(string message, Logger.LogLevel level = Logger.LogLevel.Info)
        {
            // Format the message
            string formattedMessage = $"{DateTime.Now}: [{level}] {message}";

            // Append the message to the TextBlock
            Output.Text += formattedMessage + Environment.NewLine;

            // Scroll to the bottom of the ScrollViewer
            LogScrollViewer.ChangeView(null, LogScrollViewer.ExtentHeight, null);
        }
    }
}
