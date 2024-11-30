using MacBuilder.Core.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Security.Cryptography;
using MacBuilder.Core.Logger;
using Microsoft.UI.Xaml.Printing;
using System.Xml.Linq;

namespace MacBuilder_GUI.Core.Components.Core.Apple.PlistEditor
{
    public class PlistEditor
    {
        public static bool ModifyPlistValue(string keyToModify, object newValue)
        {
            string plistPath = Global.SelectedUSB.RootDirectory.FullName + "\\EFI\\OC\\config.plist";
            XmlDocument doc = new XmlDocument();
            doc.Load(plistPath);

            XmlNodeList keys = doc.GetElementsByTagName("key");
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].InnerText == keyToModify)
                {
                    XmlNode valueNode = keys[i].NextSibling;

                    while (valueNode != null && valueNode.NodeType != XmlNodeType.Element)
                    {
                        valueNode = valueNode.NextSibling;
                    }

                    if (valueNode != null)
                    {
                        switch (valueNode.Name)
                        {
                            case "string":
                                valueNode.InnerText = newValue.ToString();
                                break;
                            case "integer":
                                if (int.TryParse(newValue.ToString(), out int intValue))
                                    valueNode.InnerText = intValue.ToString();
                                else
                                    throw new ArgumentException("Value must be an integer.");
                                break;
                            case "real":
                                if (double.TryParse(newValue.ToString(), out double realValue))
                                    valueNode.InnerText = realValue.ToString();
                                else
                                    throw new ArgumentException("Value must be a real number.");
                                break;
                            case "true":
                            case "false":
                                bool boolValue = Convert.ToBoolean(newValue);
                                XmlNode newBoolNode = doc.CreateElement(boolValue ? "true" : "false");
                                valueNode.ParentNode.ReplaceChild(newBoolNode, valueNode);
                                break;
                            case "array":
                                ModifyArray(doc, valueNode, newValue);
                                break;
                            case "dict":
                                ModifyDictionary(doc, valueNode, newValue);
                                break;
                            default:
                                throw new NotSupportedException($"Unsupported plist value type: {valueNode.Name}");
                        }

                        doc.Save(plistPath);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool DeletePlistValue(string keyToDelete)
        {
            string plistPath = Global.SelectedUSB.RootDirectory.FullName + "\\EFI\\OC\\config.plist";
            XmlDocument doc = new XmlDocument();
            doc.Load(plistPath);

            XmlNodeList keys = doc.GetElementsByTagName("key");
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].InnerText == keyToDelete)
                {
                    XmlNode valueNode = keys[i].NextSibling;

                    while (valueNode != null && valueNode.NodeType != XmlNodeType.Element)
                    {
                        valueNode = valueNode.NextSibling;
                    }

                    if (valueNode != null)
                    {
                        XmlNode parentNode = keys[i].ParentNode;
                        parentNode.RemoveChild(keys[i]);
                        parentNode.RemoveChild(valueNode);

                        doc.Save(plistPath);
                        return true;
                    }
                }
            }
            return false;
        }

        private static void ModifyArray(XmlDocument doc, XmlNode arrayNode, object newValue)
        {
            if (newValue is System.Collections.IEnumerable newArray)
            {
                arrayNode.RemoveAll();

                foreach (var item in newArray)
                {
                    XmlElement element = doc.CreateElement("string");
                    element.InnerText = item.ToString();
                    arrayNode.AppendChild(element);
                }
            }
            else
            {
                throw new ArgumentException("Value must be an array.");
            }
        }

        private static void ModifyDictionary(XmlDocument doc, XmlNode dictNode, object newValue)
        {
            if (newValue is System.Collections.Generic.Dictionary<string, object> newDict)
            {
                dictNode.RemoveAll();

                foreach (var entry in newDict)
                {
                    XmlElement keyElement = doc.CreateElement("key");
                    keyElement.InnerText = entry.Key;
                    dictNode.AppendChild(keyElement);

                    XmlElement valueElement = doc.CreateElement("string");
                    valueElement.InnerText = entry.Value.ToString();
                    dictNode.AppendChild(valueElement);
                }
            }
            else
            {
                throw new ArgumentException("Value must be a dictionary.");
            }
        }
    }

    public class OCSnapshot
    {
        private string ocFolderPath;
        private string configPlistPath;
        private string kextsDirectory;
        private string acpiDirectory;
        private string toolsDirectory;

        public OCSnapshot(string ocFolder, string configPlist)
        {
            ocFolderPath = ocFolder;
            configPlistPath = configPlist;
            kextsDirectory = Path.Combine(ocFolder, "Kexts");
            acpiDirectory = Path.Combine(ocFolder, "ACPI");
            toolsDirectory = Path.Combine(ocFolder, "Tools");
        }
        public static void CleanOCSnapshot()
        {
            try
            {
                string OCPath = Path.Combine(Global.SelectedUSB.RootDirectory.FullName, "EFI", "OC");
                string plistPath = Path.Combine(Global.SelectedUSB.RootDirectory.FullName, "EFI", "OC", "config.plist");

                OCSnapshot snapshot = new OCSnapshot(OCPath, plistPath);
                List<KextEntry> kextsFromPlist = snapshot.GetKextsFromPlist(plistPath);
                List<AcpiEntry> acpifromplist = snapshot.GetAcpiFilesFromPlist(plistPath);
                List<ToolEntry> toolsfromplist = snapshot.GetToolsFromPlist(plistPath);
                List<DriverEntry> driverfromplist = snapshot.GetDriversFromPlist(plistPath);

                snapshot.ValidateAndCleanAcpiFiles(acpifromplist);
                snapshot.SaveAcpiFilesToPlist(acpifromplist);
                snapshot.ValidateAndCleanKexts(kextsFromPlist);
                snapshot.SaveKextsToPlist(kextsFromPlist);
                snapshot.ValidateAndCleanTools(toolsfromplist);
                snapshot.SaveToolsToPlist(toolsfromplist);
                snapshot.ValidateAndCleanDrivers(driverfromplist);
                snapshot.SaveDriversToPlist(driverfromplist);
                snapshot.InsertMissingKexts();
                snapshot.InsertMissingACPI();
                snapshot.InsertMissingDrivers();
                snapshot.InsertMissingTools();
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to create CleanOCSnapshot! " + ex, Logger.LogLevel.Error);
            }
        }


        public List<KextEntry> GetKextsFromPlist(string plistPath)
        {
            var kexts = new List<KextEntry>();

            try
            {
                XDocument plist = XDocument.Load(plistPath);
                var kernelSection = plist.Root.Element("dict")?
                    .Elements("key").FirstOrDefault(e => e.Value == "Kernel")?.NextNode as XElement;
                var addSection = kernelSection?
                    .Elements("key").FirstOrDefault(e => e.Value == "Add")?.NextNode as XElement;

                if (addSection != null)
                {
                    var dictElements = addSection.Elements("dict");
                    foreach (var dict in dictElements)
                    {
                        KextEntry kext = new KextEntry();
                        var keys = dict.Elements("key");
                        foreach (var key in keys)
                        {
                            var keyName = key.Value;
                            var nextElement = key.NextNode as XElement;

                            switch (keyName)
                            {
                                case "BundlePath":
                                    kext.BundlePath = nextElement?.Value;
                                    break;
                                case "Comment":
                                    kext.Comment = nextElement?.Value;
                                    break;
                                case "Enabled":
                                    kext.Enabled = nextElement?.Name.LocalName == "true";
                                    break;
                                case "ExecutablePath":
                                    kext.ExecutablePath = nextElement?.Value;
                                    break;
                            }
                        }

                        if (!string.IsNullOrEmpty(kext.BundlePath))
                        {
                            kexts.Add(kext);
                        }
                    }
                }
                else
                {
                    Logger.Log("Add section not found in Kernel.", Logger.LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to parse plist: {ex.Message}", Logger.LogLevel.Error);
            }

            return kexts;
        }

        public void ValidateAndCleanKexts(List<KextEntry> kexts)
        {
            foreach (var kext in kexts.ToList())
            {
                string kextPath = Path.Combine(kextsDirectory, kext.BundlePath);

                if (!Directory.Exists(kextPath))
                {
                    Logger.Log($"Kext {kext.BundlePath} does not exist in directory. Removing from the list.", Logger.LogLevel.Warning);
                    kexts.Remove(kext);
                }
                else
                {
                    string executablePath = Path.Combine(kextPath, kext.ExecutablePath);
                    if (!File.Exists(executablePath))
                    {
                        Logger.Log($"Executable for kext {kext.BundlePath} not found at {executablePath}. Removing from the list.", Logger.LogLevel.Warning);
                        kexts.Remove(kext);
                    }
                }
            }
        }

        public void SaveKextsToPlist(List<KextEntry> kexts)
        {
            try
            {
                XDocument plist = XDocument.Load(configPlistPath);
                var kernelSection = plist.Root.Element("dict")?
                    .Elements("key").FirstOrDefault(e => e.Value == "Kernel")?.NextNode as XElement;
                var addSection = kernelSection?
                    .Elements("key").FirstOrDefault(e => e.Value == "Add")?.NextNode as XElement;

                if (addSection != null)
                {
                    addSection.RemoveAll();

                    foreach (var kext in kexts)
                    {
                        XElement kextDict = new XElement("dict");
                        kextDict.Add(new XElement("key", "BundlePath"), new XElement("string", kext.BundlePath));
                        kextDict.Add(new XElement("key", "Comment"), new XElement("string", kext.Comment));
                        kextDict.Add(new XElement("key", "Enabled"), new XElement("true", kext.Enabled));
                        kextDict.Add(new XElement("key", "ExecutablePath"), new XElement("string", kext.ExecutablePath));

                        addSection.Add(kextDict);
                    }
                }
                else
                {
                    Logger.Log("Add section not found in Kernel.", Logger.LogLevel.Warning);
                }

                plist.Save(configPlistPath);
                Logger.Log("Plist updated successfully.", Logger.LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to save plist: {ex.Message}", Logger.LogLevel.Error);
            }
        }



        public List<AcpiEntry> GetAcpiFilesFromPlist(string plistPath)
        {
            var acpiEntries = new List<AcpiEntry>();

            try
            {
                XDocument plist = XDocument.Load(plistPath);

                var kernelSection = plist.Root.Element("dict")?
                    .Elements("key").FirstOrDefault(e => e.Value == "ACPI")?.NextNode as XElement;
                var addSection = kernelSection?
                    .Elements("key").FirstOrDefault(e => e.Value == "Add")?.NextNode as XElement;

                if (addSection != null)
                {
                    var dictElements = addSection.Elements("dict");
                    foreach (var dict in dictElements)
                    {
                        AcpiEntry acpiEntry = new AcpiEntry();

                        var keys = dict.Elements("key");
                        foreach (var key in keys)
                        {
                            var keyName = key.Value;
                            var nextElement = key.NextNode as XElement; // Cast to XElement

                            switch (keyName)
                            {
                                case "Comment":
                                    acpiEntry.Comment = nextElement?.Value;
                                    break;
                                case "Enabled":
                                    acpiEntry.Enabled = nextElement != null && nextElement.Name.LocalName == "true"; // Check for true/false
                                    break;
                                case "Path":
                                    acpiEntry.Path = nextElement?.Value;
                                    break;
                            }
                        }

                        if (!string.IsNullOrEmpty(acpiEntry.Path))
                        {
                            acpiEntries.Add(acpiEntry);
                        }
                    }
                }
                else
                {
                    Logger.Log("Add section not found in ACPI.", Logger.LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to parse plist: {ex.Message}", Logger.LogLevel.Error);
            }

            return acpiEntries;
        }

        public void ValidateAndCleanAcpiFiles(List<AcpiEntry> acpiEntries)
        {
            foreach (var acpiEntry in acpiEntries.ToList())
            {
                string acpiFilePath = Path.Combine(acpiDirectory, acpiEntry.Path);

                if (!File.Exists(acpiFilePath))
                {
                    Logger.Log($"ACPI file {acpiEntry.Path} does not exist in directory. Removing from the list.", Logger.LogLevel.Warning);
                    acpiEntries.Remove(acpiEntry);
                }
            }
        }

        public void SaveAcpiFilesToPlist(List<AcpiEntry> acpiEntries)
        {
            try
            {
                XDocument plist = XDocument.Load(configPlistPath);

                var kernelSection = plist.Root.Element("dict")?
                    .Elements("key").FirstOrDefault(e => e.Value == "ACPI")?.NextNode as XElement;
                var addSection = kernelSection?
                    .Elements("key").FirstOrDefault(e => e.Value == "Add")?.NextNode as XElement;

                if (addSection != null)
                {
                    addSection.RemoveAll();

                    foreach (var acpiEntry in acpiEntries)
                    {
                        XElement acpiDict = new XElement("dict");

                        acpiDict.Add(new XElement("key", "Comment"), new XElement("string", acpiEntry.Comment));
                        acpiDict.Add(new XElement("key", "Enabled"), acpiEntry.Enabled ? new XElement("true") : new XElement("false"));
                        acpiDict.Add(new XElement("key", "Path"), new XElement("string", acpiEntry.Path));

                        addSection.Add(acpiDict);
                    }
                }
                else
                {
                    Logger.Log("Add section not found in ACPI.", Logger.LogLevel.Warning);
                }

                plist.Save(configPlistPath);
                Logger.Log("Plist updated successfully.", Logger.LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to save plist: {ex.Message}", Logger.LogLevel.Error);
            }
        }



        public List<ToolEntry> GetToolsFromPlist(string plistPath)
        {
            var tools = new List<ToolEntry>();

            try
            {
                XDocument plist = XDocument.Load(plistPath);

                var miscSection = plist.Root.Element("dict")?
                    .Elements("key").FirstOrDefault(e => e.Value == "Misc")?.NextNode as XElement;
                var toolsSection = miscSection?
                    .Elements("key").FirstOrDefault(e => e.Value == "Tools")?.NextNode as XElement;

                if (toolsSection != null)
                {
                    var dictElements = toolsSection.Elements("dict");
                    foreach (var dict in dictElements)
                    {
                        ToolEntry toolEntry = new ToolEntry();

                        var keys = dict.Elements("key");
                        foreach (var key in keys)
                        {
                            var keyName = key.Value;
                            var nextElement = key.NextNode as XElement;

                            switch (keyName)
                            {
                                case "Arguments":
                                    toolEntry.Arguments = nextElement?.Value;
                                    break;
                                case "Auxiliary":
                                    toolEntry.Auxiliary = nextElement != null && nextElement.Name.LocalName == "true";
                                    break;
                                case "Comment":
                                    toolEntry.Comment = nextElement?.Value;
                                    break;
                                case "Enabled":
                                    toolEntry.Enabled = nextElement != null && nextElement.Name.LocalName == "true";
                                    break;
                                case "Flavour":
                                    toolEntry.Flavour = nextElement?.Value;
                                    break;
                                case "FullNvramAccess":
                                    toolEntry.FullNvramAccess = nextElement != null && nextElement.Name.LocalName == "true";
                                    break;
                                case "Name":
                                    toolEntry.Name = nextElement?.Value;
                                    break;
                                case "Path":
                                    toolEntry.Path = nextElement?.Value;
                                    break;
                                case "RealPath":
                                    toolEntry.RealPath = nextElement != null && nextElement.Name.LocalName == "true";
                                    break;
                                case "TextMode":
                                    toolEntry.TextMode = nextElement != null && nextElement.Name.LocalName == "true";
                                    break;
                            }
                        }

                        if (!string.IsNullOrEmpty(toolEntry.Path))
                        {
                            tools.Add(toolEntry);
                        }
                    }
                }
                else
                {
                    Logger.Log("Tools section not found in Misc.", Logger.LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to parse plist: {ex.Message}", Logger.LogLevel.Error);
            }

            return tools;
        }

        public void ValidateAndCleanTools(List<ToolEntry> tools)
        {
            foreach (var tool in tools.ToList()) 
            {
                string toolPath = Path.Combine(ocFolderPath, "Tools", tool.Path);

                if (!File.Exists(toolPath))
                {
                    Logger.Log($"Tool {tool.Name} does not exist at {toolPath}. Removing from the list.", Logger.LogLevel.Warning);
                    tools.Remove(tool);
                }
            }
        }

        public void SaveToolsToPlist(List<ToolEntry> tools)
        {
            try
            {
                XDocument plist = XDocument.Load(configPlistPath);

                var miscSection = plist.Root.Element("dict")?
                    .Elements("key").FirstOrDefault(e => e.Value == "Misc")?.NextNode as XElement;
                var toolsSection = miscSection?
                    .Elements("key").FirstOrDefault(e => e.Value == "Tools")?.NextNode as XElement;

                if (toolsSection != null)
                {
                    toolsSection.RemoveAll();

                    foreach (var tool in tools)
                    {
                        XElement toolDict = new XElement("dict");

                        toolDict.Add(new XElement("key", "Arguments"), new XElement("string", tool.Arguments));
                        toolDict.Add(new XElement("key", "Auxiliary"), new XElement(tool.Auxiliary ? "true" : "false"));
                        toolDict.Add(new XElement("key", "Comment"), new XElement("string", tool.Comment));
                        toolDict.Add(new XElement("key", "Enabled"), new XElement(tool.Enabled ? "true" : "false"));
                        toolDict.Add(new XElement("key", "Flavour"), new XElement("string", tool.Flavour));
                        toolDict.Add(new XElement("key", "FullNvramAccess"), new XElement(tool.FullNvramAccess ? "true" : "false"));
                        toolDict.Add(new XElement("key", "Name"), new XElement("string", tool.Name));
                        toolDict.Add(new XElement("key", "Path"), new XElement("string", tool.Path));
                        toolDict.Add(new XElement("key", "RealPath"), new XElement(tool.RealPath ? "true" : "false"));
                        toolDict.Add(new XElement("key", "TextMode"), new XElement(tool.TextMode ? "true" : "false"));

                        toolsSection.Add(toolDict);
                    }
                }
                else
                {
                    Logger.Log("Tools section not found in Misc.", Logger.LogLevel.Warning);
                }

                plist.Save(configPlistPath);
                Logger.Log("Plist updated successfully.", Logger.LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to save plist: {ex.Message}", Logger.LogLevel.Error);
            }
        }


        public List<DriverEntry> GetDriversFromPlist(string plistPath)
        {
            var drivers = new List<DriverEntry>();
            try
            {
                XDocument plist = XDocument.Load(plistPath);
                var kernelSection = plist.Root.Element("dict")?.Elements("key").FirstOrDefault(e => e.Value == "UEFI")?.NextNode as XElement;
                var driversSection = kernelSection?.Elements("key").FirstOrDefault(e => e.Value == "Drivers")?.NextNode as XElement;

                if (driversSection != null)
                {
                    var dictElements = driversSection.Elements("dict");
                    foreach (var dict in dictElements)
                    {
                        DriverEntry driver = new DriverEntry();
                        var keys = dict.Elements("key");
                        foreach (var key in keys)
                        {
                            var keyName = key.Value;
                            var nextElement = key.NextNode as XElement;

                            switch (keyName)
                            {
                                case "Arguments":
                                    driver.Arguments = nextElement?.Value;
                                    break;
                                case "Comment":
                                    driver.Comment = nextElement?.Value;
                                    break;
                                case "Enabled":
                                    driver.Enabled = nextElement?.Name.LocalName == "true";
                                    break;
                                case "LoadEarly":
                                    driver.LoadEarly = nextElement?.Name.LocalName == "true";
                                    break;
                                case "Path":
                                    driver.Path = nextElement?.Value;
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(driver.Path))
                        {
                            drivers.Add(driver);
                        }
                    }
                }
                else
                {
                    Logger.Log("Drivers section not found in UEFI.", Logger.LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to parse plist: {ex.Message}", Logger.LogLevel.Error);
            }
            return drivers;
        }

        public void ValidateAndCleanDrivers(List<DriverEntry> drivers)
        {
            foreach (var driver in drivers.ToList())
            {
                string driverPath = Path.Combine(toolsDirectory, driver.Path);
                if (!File.Exists(driverPath))
                {
                    Logger.Log($"Driver {driver.Path} does not exist in directory. Removing from the list.", Logger.LogLevel.Warning);
                    drivers.Remove(driver);
                }
            }
        }

        public void SaveDriversToPlist(List<DriverEntry> drivers)
        {
            try
            {
                XDocument plist = XDocument.Load(configPlistPath);
                var kernelSection = plist.Root.Element("dict")?.Elements("key").FirstOrDefault(e => e.Value == "UEFI")?.NextNode as XElement;
                var driversSection = kernelSection?.Elements("key").FirstOrDefault(e => e.Value == "Drivers")?.NextNode as XElement;

                if (driversSection != null)
                {
                    driversSection.RemoveAll();
                    foreach (var driver in drivers)
                    {
                        XElement driverDict = new XElement("dict");
                        driverDict.Add(new XElement("key", "Arguments"), new XElement("string", driver.Arguments));
                        driverDict.Add(new XElement("key", "Comment"), new XElement("string", driver.Comment));
                        driverDict.Add(new XElement("key", "Enabled"), new XElement(driver.Enabled ? "true" : "false"));
                        driverDict.Add(new XElement("key", "LoadEarly"), new XElement(driver.LoadEarly ? "true" : "false"));
                        driverDict.Add(new XElement("key", "Path"), new XElement("string", driver.Path));
                        driversSection.Add(driverDict);
                    }
                }
                else
                {
                    Logger.Log("Drivers section not found in UEFI.", Logger.LogLevel.Error);
                }

                plist.Save(configPlistPath);
                Logger.Log("Plist updated successfully.", Logger.LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to save plist: {ex.Message}", Logger.LogLevel.Error);
            }
        }

        public void InsertMissingKexts()
        {
            Logger.Log("Detecting and inserting missing kexts.", Logger.LogLevel.Info);
            var kextFiles = Directory.GetDirectories(Path.Combine(ocFolderPath, "Kexts"), "*.kext");
            XDocument plist = XDocument.Load(configPlistPath);

            var kernelSection = plist.Root.Element("dict")?
                .Elements("key").FirstOrDefault(e => e.Value == "Kernel")?.NextNode as XElement;
            var addSection = kernelSection?
                .Elements("key").FirstOrDefault(e => e.Value == "Add")?.NextNode as XElement;

            if (addSection != null)
            {
                var existingKextPaths = addSection.Elements("dict")
                    .Elements("key").Where(k => k.Value == "BundlePath")
                    .Select(p => p.NextNode as XElement)
                    .Select(e => e.Value).ToList();

                foreach (var kextDir in kextFiles)
                {
                    var kextName = Path.GetFileName(kextDir);
                    if (!existingKextPaths.Contains(kextName))
                    {
                        Logger.Log($"Kext {kextName} is missing in plist, detecting information and adding it.", Logger.LogLevel.Info);

                        string executablePath = Path.Combine(kextDir, "Contents", "MacOS");
                        if (!Directory.Exists(executablePath))
                        {
                            Logger.Log($"{kextName} does not have a valid executable path ignoring kext!", Logger.LogLevel.Error);
                            return;
                        } 
                        var executableFiles = Directory.GetFiles(executablePath, "*.kext");

                        string detectedExecutable = executableFiles.FirstOrDefault() ?? "Unknown";
                        if (detectedExecutable == "Unknown")
                        {
                            Logger.Log($"Could not find kext {kextName} executable file ignoring kext!", Logger.LogLevel.Error);
                            return;
                        }

                        XElement kextDict = new XElement("dict");
                        kextDict.Add(new XElement("key", "BundlePath"), new XElement("string", kextName));
                        kextDict.Add(new XElement("key", "Comment"), new XElement("string", "Added automatically"));
                        kextDict.Add(new XElement("key", "Enabled"), new XElement("true"));
                        kextDict.Add(new XElement("key", "ExecutablePath"), new XElement("string", detectedExecutable));

                        addSection.Add(kextDict);
                    }
                }

                plist.Save(configPlistPath);
                Logger.Log("Kext insertion completed, plist saved.", Logger.LogLevel.Info);
            }
            else
            {
                Logger.Log("Add section not found in Kernel section.", Logger.LogLevel.Error);
            }
        }

        public void InsertMissingACPI()
        {
            Logger.Log("Inserting missing ACPI files.", Logger.LogLevel.Info);
            var acpiFiles = Directory.GetFiles(Path.Combine(ocFolderPath, "ACPI"), "*.aml");
            XDocument plist = XDocument.Load(configPlistPath);

            var acpiSection = plist.Root.Element("dict")?
                .Elements("key").FirstOrDefault(e => e.Value == "ACPI")?.NextNode as XElement;
            var addSection = acpiSection?
                .Elements("key").FirstOrDefault(e => e.Value == "Add")?.NextNode as XElement;

            if (addSection != null)
            {
                var existingAcpiPaths = addSection.Elements("dict")
                    .Elements("key").Where(k => k.Value == "Path")
                    .Select(p => p.NextNode as XElement)
                    .Select(e => e.Value).ToList();

                foreach (var acpiFile in acpiFiles)
                {
                    var acpiName = Path.GetFileName(acpiFile);
                    if (!existingAcpiPaths.Contains(acpiName))
                    {
                        Logger.Log($"ACPI {acpiName} is missing in plist, adding it.", Logger.LogLevel.Info);
                        XElement acpiDict = new XElement("dict");
                        acpiDict.Add(new XElement("key", "Path"), new XElement("string", acpiName));
                        acpiDict.Add(new XElement("key", "Comment"), new XElement("string", "MacBuilder"));
                        acpiDict.Add(new XElement("key", "Enabled"), new XElement("true"));

                        addSection.Add(acpiDict);
                    }
                }

                plist.Save(configPlistPath);
                Logger.Log("ACPI insertion completed, plist saved.", Logger.LogLevel.Info);
            }
            else
            {
                Logger.Log("Add section not found in ACPI section.", Logger.LogLevel.Error);
            }
        }

        public void InsertMissingTools()
        {
            Logger.Log("Inserting missing tools.", Logger.LogLevel.Info);
            var toolFiles = Directory.GetFiles(Path.Combine(ocFolderPath, "Tools"), "*.efi");
            XDocument plist = XDocument.Load(configPlistPath);

            var miscSection = plist.Root.Element("dict")?
                .Elements("key").FirstOrDefault(e => e.Value == "Misc")?.NextNode as XElement;
            var toolsSection = miscSection?
                .Elements("key").FirstOrDefault(e => e.Value == "Tools")?.NextNode as XElement;

            if (toolsSection != null)
            {
                var existingToolPaths = toolsSection.Elements("dict")
                    .Elements("key").Where(k => k.Value == "Path")
                    .Select(p => p.NextNode as XElement)
                    .Select(e => e.Value).ToList();

                foreach (var toolFile in toolFiles)
                {
                    var toolName = Path.GetFileName(toolFile);
                    if (!existingToolPaths.Contains(toolName))
                    {
                        Logger.Log($"Tool {toolName} is missing in plist, adding it.", Logger.LogLevel.Info);
                        XElement toolDict = new XElement("dict");
                        toolDict.Add(new XElement("key", "Path"), new XElement("string", toolName));
                        toolDict.Add(new XElement("key", "Comment"), new XElement("string", "MacBuilder"));
                        toolDict.Add(new XElement("key", "Enabled"), new XElement("true"));
                        toolDict.Add(new XElement("key", "Arguments"), new XElement("string", ""));
                        toolDict.Add(new XElement("key", "Auxiliary"), new XElement("false"));
                        toolDict.Add(new XElement("key", "Flavour"), new XElement("string", "Auto"));
                        toolDict.Add(new XElement("key", "FullNvramAccess"), new XElement("false"));
                        toolDict.Add(new XElement("key", "RealPath"), new XElement("false"));
                        toolDict.Add(new XElement("key", "TextMode"), new XElement("false"));

                        toolsSection.Add(toolDict);
                    }
                }

                plist.Save(configPlistPath);
                Logger.Log("Tools insertion completed, plist saved.", Logger.LogLevel.Info);
            }
            else
            {
                Logger.Log("Tools section not found in Misc.", Logger.LogLevel.Error);
            }
        }

        public void InsertMissingDrivers()
        {
            Logger.Log("Inserting missing drivers.", Logger.LogLevel.Info);
            var driverFiles = Directory.GetFiles(Path.Combine(ocFolderPath, "Drivers"), "*.efi");
            XDocument plist = XDocument.Load(configPlistPath);

            var uefiSection = plist.Root.Element("dict")?
                .Elements("key").FirstOrDefault(e => e.Value == "UEFI")?.NextNode as XElement;
            var driversSection = uefiSection?
                .Elements("key").FirstOrDefault(e => e.Value == "Drivers")?.NextNode as XElement;

            if (driversSection != null)
            {
                var existingDriverPaths = driversSection.Elements("dict")
                    .Elements("key").Where(k => k.Value == "Path")
                    .Select(p => p.NextNode as XElement)
                    .Select(e => e.Value).ToList();

                foreach (var driverFile in driverFiles)
                {
                    var driverName = Path.GetFileName(driverFile);
                    if (!existingDriverPaths.Contains(driverName))
                    {
                        Logger.Log($"Driver {driverName} is missing in plist, adding it.", Logger.LogLevel.Info);
                        XElement driverDict = new XElement("dict");
                        driverDict.Add(new XElement("key", "Path"), new XElement("string", driverName));
                        driverDict.Add(new XElement("key", "Comment"), new XElement("string", "MacBuilder"));
                        driverDict.Add(new XElement("key", "Enabled"), new XElement("true"));
                        driverDict.Add(new XElement("key", "LoadEarly"), new XElement("false"));

                        driversSection.Add(driverDict);
                    }
                }

                plist.Save(configPlistPath);
                Logger.Log("Drivers insertion completed, plist saved.", Logger.LogLevel.Info);
            }
            else
            {
                Logger.Log("Drivers section not found in UEFI.", Logger.LogLevel.Error);
            }
        }
    }

    public class KextEntry
    {
        public string BundlePath { get; set; }
        public string ExecutablePath { get; set; }
        public string Comment { get; set; }
        public bool Enabled { get; set; }

    }
    public class AcpiEntry
    {
        public string Comment { get; set; }
        public bool Enabled { get; set; }
        public string Path { get; set; }
    }
    public class ToolEntry
    {
        public string Arguments { get; set; }
        public bool Auxiliary { get; set; }
        public string Comment { get; set; }
        public bool Enabled { get; set; }
        public string Flavour { get; set; }
        public bool FullNvramAccess { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public bool RealPath { get; set; }
        public bool TextMode { get; set; }
    }
    public class DriverEntry
    {
        public string Arguments { get; set; }
        public string Comment { get; set; }
        public bool Enabled { get; set; }
        public bool LoadEarly { get; set; }
        public string Path { get; set; }
    }
}