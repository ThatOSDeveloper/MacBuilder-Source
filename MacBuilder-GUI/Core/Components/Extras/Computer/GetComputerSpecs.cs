using MacBuilder.Core.Logger;
using MacBuilder_GUI.Core.Components.Core.Apple.Types.Kexts;
using System;
using System.Collections.Generic;
using static MacBuilder_GUI.Core.Components.Core.Classes.MacClasses;

namespace MacBuilder_GUI.Core.Components.Assets.Computer
{
    public class GetComputerSpecs
    {
        public static List<MacOSVersion> GetSupportedMacOS(string supportedMacVersions)
        {
            var supportedVersions = new List<MacOSVersion>();

            string[] versions = supportedMacVersions.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var version in versions)
            {
                if (version.EndsWith("+"))
                {
                    string osVersion = version.TrimEnd('+');
                    Logger.Log($"Adding version {osVersion}+ (and above)");
                    foreach (var supportedVersion in GetAllVersionsFrom(osVersion))
                    {
                        supportedVersions.Add(supportedVersion);
                    }
                }
                else if (version.EndsWith("-"))
                {
                    string osVersion = version.TrimEnd('-');
                    Logger.Log($"Adding version {osVersion}- (and below)");
                    foreach (var supportedVersion in GetAllVersionsUpTo(osVersion))
                    {
                        supportedVersions.Add(supportedVersion);
                    }
                }
                else
                {
                    Logger.Log($"Adding specific version {version}");
                    supportedVersions.Add(new MacOSVersion { OSName = GetMacOSName(version), OSVersion = version });
                }
            }
            return supportedVersions;
        }

        private static List<MacOSVersion> GetAllVersionsFrom(string startingVersion)
        {
            var allVersions = new List<MacOSVersion>();

            string[] allMacVersions = new[]
            {
                "10.7",  // Lion
                "10.8",  // Mountain Lion
                "10.9",  // Mavericks
                "10.10", // Yosemite
                "10.11", // El Capitan
                "10.12", // Sierra
                "10.13", // High Sierra
                "10.14", // Mojave
                "10.15", // Catalina
                "11.0",  // Big Sur
                "12.0",  // Monterey
                "13.0",  // Ventura
                "14.0"   // Sonoma
            };

            bool addVersion = false;

            foreach (var version in allMacVersions)
            {
                if (version == startingVersion)
                {
                    addVersion = true;  // Start adding versions from here
                }

                if (addVersion)
                {
                    allVersions.Add(new MacOSVersion
                    {
                        OSName = GetMacOSName(version),
                        OSVersion = version,
                        IconPath = GetMacOSIcon(version)
                    });
                }
            }

            return allVersions;
        }

        private static List<MacOSVersion> GetAllVersionsUpTo(string upToVersion)
        {
            var allVersions = new List<MacOSVersion>();

            string[] allMacVersions = new[]
            {
                "10.7",  // Lion
                "10.8",  // Mountain Lion
                "10.9",  // Mavericks
                "10.10", // Yosemite
                "10.11", // El Capitan
                "10.12", // Sierra
                "10.13", // High Sierra
                "10.14", // Mojave
                "10.15", // Catalina
                "11.0",  // Big Sur
                "12.0",  // Monterey
                "13.0",  // Ventura
                "14.0"   // Sonoma
            };

            foreach (var version in allMacVersions)
            {
                allVersions.Add(new MacOSVersion
                {
                    OSName = GetMacOSName(version),
                    OSVersion = version,
                    IconPath = GetMacOSIcon(version) // Bind the icon path here
                });

                if (version == upToVersion)
                {
                    break;
                }
            }

            return allVersions;
        }

        private static string GetMacOSIcon(string version)
        {
            return version switch
            {
                "10.7" => "ms-appx:///Assets/MacOS/Lion.png",
                "10.8" => "ms-appx:///Assets/MacOS/MountainLion.png",
                "10.9" => "ms-appx:///Assets/MacOS/Mavericks.png",
                "10.10" => "ms-appx:///Assets/MacOS/Yosemite.png",
                "10.11" => "ms-appx:///Assets/MacOS/ElCapitan.png",
                "10.12" => "ms-appx:///Assets/MacOS/Sierra.png",
                "10.13" => "ms-appx:///Assets/MacOS/HighSierra.png",
                "10.14" => "ms-appx:///Assets/MacOS/Mojave.png",
                "10.15" => "ms-appx:///Assets/MacOS/Catalina.png",
                "11.0" => "ms-appx:///Assets/MacOS/BigSur.png",
                "12.0" => "ms-appx:///Assets/MacOS/Monterey.png",
                "13.0" => "ms-appx:///Assets/MacOS/Ventura.png",
                "14.0" => "ms-appx:///Assets/MacOS/Sonoma.png",
                _ => "ms-appx:///Assets/Unknown.png" // Fallback non-existing icon
            };
        }


        private static string GetMacOSName(string version)
        {
            return version switch
            {
                "10.7" => "Lion",
                "10.8" => "Mountain Lion",
                "10.9" => "Mavericks",
                "10.10" => "Yosemite",
                "10.11" => "El Capitan",
                "10.12" => "Sierra",
                "10.13" => "High Sierra",
                "10.14" => "Mojave",
                "10.15" => "Catalina",
                "11.0" => "Big Sur",
                "12.0" => "Monterey",
                "13.0" => "Ventura",
                "14.0" => "Sonoma",
                _ => "Unknown Version"
            };
        }
    }
}
