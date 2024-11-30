using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using MacBuilder_GUI.Core.Components.Core.FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Core.Apple.Types.OC
{
    public class OCClass
    {
        public static void CleanupFiles()
        {
            string OC = Global.SelectedUSB.RootDirectory.FullName + "\\EFI\\OC";
            FileManagerClass.DeleteAllFilesInDirectory(OC + "\\Drivers");
            FileManagerClass.DeleteAllFilesInDirectory(OC + "\\Tools");
            Logger.Log("Finished cleaning up files.");
        }
    }
}
