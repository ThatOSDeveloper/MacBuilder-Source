using CommunityToolkit.Common.Parsers;
using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using MacBuilder_GUI.Core.Components.Assets.Dialogs;
using MacBuilder_GUI.Core.Components.Assets.WebClient;
using MacBuilder_GUI.Core.Components.Core.Apple.PlistEditor;
using MacBuilder_GUI.Core.Components.Core.Apple.Types.ACPI;
using MacBuilder_GUI.Core.Components.Core.Apple.Types.Kexts;
using MacBuilder_GUI.Core.Components.Core.Selection;
using MacBuilder_GUI.Core.Components.Core.USB;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using static MacBuilder_GUI.Core.Components.Assets.Computer.GetComputerSpecs;
using static MacBuilder_GUI.Core.Components.Core.Classes.MacClasses;

namespace MacBuilder_GUI
{
    public sealed partial class BaseSelector : Page
    {
        public BaseSelector()
        {
            this.InitializeComponent();
        }
        private async void Selection_Loaded(object sender, RoutedEventArgs e)
        {
            var usbDevices = await GetConnectedUsbDevicesAsync();
            USBList.ItemsSource = usbDevices; // Set the ItemsSource after waiting for the result
            Logger.Log("Supported versions str: " + Global.hardwareInfo.Supported.SupportedMacVersions);
            List<MacOSVersion> supportedOS = GetSupportedMacOS(Global.hardwareInfo.Supported.SupportedMacVersions);

            // Bind the list of MacOSVersion objects to the OSList
            OSList.ItemsSource = supportedOS;
            Logger.Log("Pre-Downloading OpenCore...");

            await WebClientClass.DownloadLatestOpenCoreDebug();
        }
        private async Task<List<UsbDeviceInfo>> GetConnectedUsbDevicesAsync()
        {
            try
            {
                List<UsbDeviceInfo> usbDevices = new List<UsbDeviceInfo>();
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                const long bytesToGB = 1024 * 1024 * 1024; // Conversion factor
                int usbCount = 0;

                foreach (DriveInfo drive in allDrives)
                {
                    if (drive.DriveType == DriveType.Removable && drive.IsReady)
                    {
                        usbCount++;
                        usbDevices.Add(new UsbDeviceInfo
                        {
                            Name = string.IsNullOrEmpty(drive.VolumeLabel) ? "Unnamed Drive" : drive.VolumeLabel, // Handle empty volume label
                            Id = drive.Name, // Drive letter
                            StorageCapacity = $"{drive.TotalSize / bytesToGB} GB" // Convert bytes to GB
                        });

                        Logger.Log($"{usbCount}. {drive.Name} - {drive.VolumeLabel ?? "Unnamed Drive"} ({drive.TotalSize / bytesToGB} GB)");
                    }
                }

                if (usbCount == 0)
                {
                    Logger.Log("No USB drives detected.");
                }

                return usbDevices; // Return the list of UsbDeviceInfo
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Logger.LogLevel.Error);
                return new List<UsbDeviceInfo>(); // Return an empty list on error to avoid null reference
            }
        }
        private async void USBList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (USBList.SelectedItem is UsbDeviceInfo selectedUsbDevice)
            {
                string message = $"Selected USB Device: {selectedUsbDevice.Name} ({selectedUsbDevice.StorageCapacity})";
                Logger.Log(selectedUsbDevice.Id + " pending deletion.");
                if (await DialogClass.ShowYesNoDialogAsync("WARNING", $"The USB device {selectedUsbDevice.Name} will be wiped. Please backup any important data."))
                {
                    DriveInfo driveInfo = new DriveInfo(selectedUsbDevice.Id);

                    if (await USBClass.CheckUSBStorageAsync(driveInfo))
                    {
                        Global.SelectedUSB = driveInfo;
                        Page2.Visibility = Visibility.Visible;
                        Page1.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        private async void OSList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if an item is selected
            if (OSList.SelectedItem is MacOSVersion selectedVersion)
            {
                string selectedOSName = selectedVersion.OSName;
                string selectedOSVersion = selectedVersion.OSVersion;
                string macrecoveryfile = Global.DownloadPath+ "\\OpenCore\\Utilities\\macrecovery\\macrecovery.py";

                Logger.Log($"User selected: {selectedOSName} (Version {selectedOSVersion})");
                if (Global.SkipDevStuff)
                {
                    if (Directory.Exists(Global.SelectedUSB.RootDirectory.FullName + "\\com.apple.recovery.boot"))
                    {

                        return;
                    }
                }
                switch (selectedOSName) 
                {
                    case "Lion":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-2E6FAB96566FE58C -m 00000000000F25Y00 download");
                        break;
                    case "Mountain Lion":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-7DF2A3B5E5D671ED -m 00000000000F65100 download");
                        break;
                    case "Mavericks":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-F60DEB81FF30ACF6 -m 00000000000FNN100 download");
                        break;
                    case "Yosemite":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-E43C1C25D4880AD6 -m 00000000000GDVW00 download");
                        break;
                    case "El Capitan":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-FFE5EF870D7BA81A -m 00000000000GQRX00 download");
                        break;
                    case "Sierra":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-77F17D7DA9285301 -m 00000000000J0DX00 download");
                        break;
                    case "High Sierra":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-7BA5B2D9E42DDD94 -m 00000000000J80300 download");
                        break;
                    case "Mojave":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-7BA5B2DFE22DDD8C -m 00000000000KXPG00 download");
                        break;
                    case "Catalina":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-00BE6ED71E35EB86 -m 00000000000000000 download");
                        break;
                    case "Big Sur":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-42FD25EABCABB274 -m 00000000000000000 download");
                        break;
                    case "Monterey":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-FFE5EF870D7BA81A -m 00000000000000000 download");
                        break;
                    case "Ventura":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-4B682C642B45593E -m 00000000000000000 download");
                        break;  
                    case "Sonoma":
                        SelectionClass.RunApplicationAsync("python " + macrecoveryfile + " -b Mac-937A206F2EE63C01 -m 00000000000000000 download");
                        break;
                    default:
                        DialogClass.MessageBox("Not Found", $"Could not find selected OS {selectedOSVersion} please report this issue.");
                        break;
                }
            }
            else
            {
                Logger.Log("No valid OS selected or selection is null.");
            }
        }
        private async void RefreshDevices_Click(object sender, RoutedEventArgs e)
        {
            var usbDevices = await GetConnectedUsbDevicesAsync();
            USBList.ItemsSource = usbDevices;
        }
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(typeof(USBPartitionPage));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(typeof(MainMenu));
        }
    }
}
