using MacBuilder.Core.Global;
using MacBuilder_GUI.Core.Components.Core.Apple.PlistEditor;
using MacBuilder_GUI.Core.Components.Core.Apple.Types.Kexts;
using MacBuilder_GUI.Core.Components.Core.Apple.Types.OC;
using MacBuilder_GUI.Core.Components.Core.FileManager;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MacBuilder_GUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EFISetup : Page
    {
        public EFISetup()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            OCSnapshot.CleanOCSnapshot();

            // generate bin file calling it a proj
            string USBPath = Path.Combine(Global.SelectedUSB.RootDirectory.FullName);
            string MacBuilder = Path.Combine(USBPath, "MacBuilder");
            if (!Directory.Exists(MacBuilder))
            {
                FileManagerClass.CreateDirectory(MacBuilder);
                FileManagerClass.CreateDirectory(Path.Combine(MacBuilder, "Bin"));
            }
            FileManagerClass.CreateFile(Path.Combine(USBPath, "MacBuilder", "Bin"));
        }
    }
}
