using MacBuilder.Core.Global;
using MacBuilder_GUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MacBuilder_GUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        public static Frame ShellFrame { get; private set; }

        private static Mutex mutex;
        public static bool CreatedNew { get; private set; }
        public static Grid GridBackground { get; private set; }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        public MainWindow()
        {
            InitializeComponent();
            GridBackground = this.BackgroundGrid;
            try
            {
                ShowConsole();
                // Create mutex to prevent multiple instances
                mutex = new Mutex(true, "macbuilder", out bool createdNew);
                CreatedNew = createdNew;

                // if launcher is already open, close.
                if (!CreatedNew)
                {
                    Windows.ApplicationModel.Core.CoreApplication.Exit();
                }

                InitializeWindow();
            } catch { }
        }

        private void InitializeWindow()
        {
            try
            {
                // Set window properties
                this.ExtendsContentIntoTitleBar = true;
                this.SetTitleBar(AppTitleBar);
                this.Title = "MacBuilder - By Kivie";

                // Lets set the window size depending on the screen size, which is better.
                SetWindowSize();

                this.CenterOnScreen();
                this.SetIsResizable(false);
                this.BringToFront();
                this.SetIcon("Assets\\macbuilder.ico");

                // We need to set the shellframe to use across the app to navigate.
                ShellFrame = MainWindowFrame;
                MainWindowFrame.Navigate(typeof(Initialize));
            }
            catch { }
        }

        private void ShowConsole()
        {
            try
            {
                if (AllocConsole())
                {
                    Console.Title = "MacBuilder - Console";
                }
            } catch { }
        }

        private void SetWindowSize()
        {
            try
            {
                var displayBounds = GetDisplayBounds();

                double screenWidth = displayBounds.Width;
                double screenHeight = displayBounds.Height;

                int width = screenWidth < 1600 ? 800 : 1100; // window width + low res support
                int height = screenHeight < 900 ? 600 : 700; // window height

                this.SetWindowSize(width, height);
            } catch { }
        }

        public static async void Navigate(Type page)
        {
            if (Global.Animations)
            {
                var animator = new NavigationAnimator(MainWindow.ShellFrame);

                // Slide out the current page
                await animator.NavigateWithSlideOutAsync(duration: 700, direction: "right");

                // Set the new page in the frame
                ShellFrame.Navigate(page);

                // Slide in the new page
                await animator.NavigateWithSlideInAsync(page, duration: 700, direction: "left");
            }
            else
            {
                ShellFrame.Navigate(page);
            }
        }

        // Get display info.
        private Windows.Graphics.RectInt32 GetDisplayBounds()
        {
            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(this.AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
            return displayArea.WorkArea;
        }

        private void WindowEx_Closed(object sender, WindowEventArgs args)
        {
            try
            {
                // known issue here: Does not close properly, exceiption occurs(yes the exception is handled) but an external exception occurs no idea how to fix
                mutex.ReleaseMutex();
                Windows.ApplicationModel.Core.CoreApplication.Exit();
            }
            catch { }
        }
    }
}
