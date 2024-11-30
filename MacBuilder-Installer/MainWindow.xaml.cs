using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.UI;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MacBuilder_Installer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        public static Frame ShellFrame { get; private set; }

        private static Mutex mutex;
        public static bool CreatedNew { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Hide default title bar
                this.SetTitleBar(CustomTitleBar);
                this.ExtendsContentIntoTitleBar = true;
                this.IsResizable = false;
                this.IsMaximizable = false;
                mutex = new Mutex(true, "macbuilderins", out bool createdNew);
                CreatedNew = createdNew;

                if (!CreatedNew)
                {
                    Windows.ApplicationModel.Core.CoreApplication.Exit();
                }

                InitializeWindow();
            }
            catch { }
        }


        private void InitializeWindow()
        {
            try
            {
                this.BringToFront();
                this.SetIcon("ms-appx:///Assets/macbuilder.ico");
                ShellFrame = MainWindowFrame;
                MainWindowFrame.Navigate(typeof(InstallerPage));
            }
            catch { }
        }
    }
}
