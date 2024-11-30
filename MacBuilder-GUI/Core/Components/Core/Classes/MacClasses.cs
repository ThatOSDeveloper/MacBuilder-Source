using MacBuilder.Core.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Core.Classes
{
    public class MacClasses
    {
        public class HardwareInfo
        {
            public CPUInfo CPU { get; set; }
            public GPUInfo GPU { get; set; }
            public COMPUTER Computer { get; set; }
            public SupportedInfo Supported { get; set; }
        }

        public class CPUInfo
        {
            public int Cores { get; set; }
            public bool Is64Bit { get; set; }
            public bool IsAMD { get; set; }
            public bool IsCPUSupported { get; set; }
            public string Name { get; set; }
            public bool SupportsSSE3 { get; set; }
            public bool SupportsSSE4 { get; set; }
            public bool SupportsSSE42 { get; set; }
        }

        public class GPUInfo
        {
            public bool IsGPUSupported { get; set; }
            public string Model { get; set; }
        }
        public class COMPUTER
        {
            public string SystemType { get; set; }
        }
        public class SupportedInfo
        {
            public bool IsFullySupported { get; set; }
            public bool IsNotSupported { get; set; }
            public bool IsPartiallySupported { get; set; }
            public string SupportedMacVersions { get; set; }
            public bool isUEFISupported { get; set; }
        }

        public class MacOSVersion
        {
            public string OSName { get; set; }
            public string OSVersion { get; set; }
            public string IconPath { get; set; }

        }
        public class UsbDeviceInfo
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public string StorageCapacity { get; set; }
        }

        public static class HardwareInfoLoader
        {
            public static HardwareInfo LoadHardwareInfo(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    Logger.Log("Hardware dump file not found " + filePath);
                }

                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<HardwareInfo>(json);
            }
        }
    }
}
