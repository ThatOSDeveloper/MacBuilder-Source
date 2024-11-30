using MacBuilder_GUI.Core.Components.Assets.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Core.Apple.Types.EFI
{
    public class EFIClass
    {
        public static void Install(string EFIName)
        {
            switch (EFIName)
            {
                case "HfsPlus.efi":
                    break;
                case "OpenRuntime.efi":
                    break;
                case "HfsPlus32.efi":
                    break;
                default:
                    DialogClass.MessageBox("Not Found", "Could not find specified EFI: " + EFIName);
                    break;
            }
        }
    }
}
