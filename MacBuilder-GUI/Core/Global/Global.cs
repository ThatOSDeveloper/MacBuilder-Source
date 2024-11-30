using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using static MacBuilder_GUI.Core.Components.Assets.Computer.GetComputerSpecs;
using static MacBuilder_GUI.Core.Components.Core.Classes.MacClasses;

namespace MacBuilder.Core.Global
{
    public class Global
    {
        // Entry
        public static string Version = "mb-1.0.0"; // macbuilder-1.0.0
        public static bool EnableLogging = true;

        // UI
        public static bool Animations = false;
        public static double ColorTransitionDuritation = 4.0;
        public static double StopsMovementDuration = 3.0;
        public static double AnimationSpeed = 3.0;
        public UIElement mainwindow; //ui element or sum instead of window

        // MacBuilder
        public static string DownloadPath = GetDownloadPath(); // Something wasnt working right so i had to do this.
        public static DriveInfo SelectedUSB;
        public static bool SkipDevStuff = false; // skips the erase usb and download if already downloaded
        public static Window m_window;
        public static HardwareInfo hardwareInfo;
        public static string GetDownloadPath()
        {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloadsPath = Path.Combine(userProfilePath, "Documents", "MacBuilder");

            if (!Directory.Exists(downloadsPath))
            {
                Directory.CreateDirectory(downloadsPath);
            }
            if (Directory.Exists(downloadsPath))
            {
                return downloadsPath;
            }

            return "Downloads folder does not exist.";
        }
    }
}
