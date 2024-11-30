using MacBuilder.Core.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Core.Permission
{
    public class WindowsDefenderClass
    {
        public static bool IsExcludedFromWindowsDefender(string appPath)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-Command \"Get-MpPreference | Select-Object -ExpandProperty ExclusionPath\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    using (var reader = process.StandardOutput)
                    {
                        string output = reader.ReadToEnd();

                        string[] exclusions = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        return exclusions.Any(exclusion => exclusion.Trim().Equals(appPath.Trim(), StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error checking Windows Defender exclusions: {ex.Message}", Logger.LogLevel.Error);
            }

            return false;
        }
    }
}
