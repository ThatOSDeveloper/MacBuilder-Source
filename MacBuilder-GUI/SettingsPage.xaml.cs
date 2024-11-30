using System.IO;
using MacBuilder.Core;
using MacBuilder.Core.Global;
using MacBuilder.Core.Logger;
using MacBuilder_GUI.Core.Components.Assets.Dialogs;
using MacBuilder_GUI.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Newtonsoft.Json; // Import Newtonsoft.Json
using static MacBuilder_GUI.Core.Components.Core.Classes.UserClasses;

namespace MacBuilder_GUI
{
    public sealed partial class InitialSetup : Page
    {
        private const string SettingsFileName = "UserSettings.json";

        public InitialSetup()
        {
            this.InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            string settingsPath = Path.Combine(Global.DownloadPath, SettingsFileName);
            if (File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                var settings = JsonConvert.DeserializeObject<UserSettings>(json); // Use JsonConvert

                if (settings != null)
                {
                    AnimationSpeed.Value = settings.AnimationSpeed;
                    AnimationColorTransDurit.Value = settings.ColorTransitionDuration;
                    StopMovementDuration.Value = settings.StopMovementDuration;
                    DebugLogPathTextBox.Text = settings.DebugLogPath;
                    AnimationsToggle.IsOn = settings.AnimationsEnabled;
                    DarkModeToggle.IsOn = settings.DarkModeEnabled;

                    // Update UI according to loaded settings
                    AnimationsToggle_Toggled(null, null); // Adjust visibility based on toggle
                    DarkModeToggle_Toggled(null, null); // Adjust dark mode resources
                }
            }
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var backgroundgrid = MainWindow.GridBackground;
            string logPath = DebugLogPathTextBox.Text;
            bool animationsEnabled = AnimationsToggle.IsOn;
            bool darkModeEnabled = DarkModeToggle.IsOn;

            // Update dark mode resources
            if (darkModeEnabled)
            {
                Application.Current.Resources["DefaultBackgroundColor"] = Colors.Black;
                Application.Current.Resources["DefaultForegroundColor"] = Colors.White;
            }
            else
            {
                Application.Current.Resources["DefaultBackgroundColor"] = Colors.White;
                Application.Current.Resources["DefaultForegroundColor"] = Colors.Black;
            }

            if (animationsEnabled)
            {
                if (darkModeEnabled)
                {
                    DialogClass.MessageBox("Error", "Animations are not supported in dark mode. Please disable dark mode to enable animations.");
                    return;
                }
                var compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;
                var animator = new BackgroundAnimator(compositor, backgroundgrid);
                Global.AnimationSpeed = AnimationSpeed.Value;
                Global.ColorTransitionDuritation = AnimationColorTransDurit.Value;
                Global.StopsMovementDuration = StopMovementDuration.Value;
                Logger.Log("Set Animation speed: " + Global.AnimationSpeed);
                Logger.Log("Set Color Transition Duration: " + Global.ColorTransitionDuritation);
                Logger.Log("Set Movement Stops Duration: " + Global.StopsMovementDuration);
                Logger.Log("Starting background color animation.");
                animator.StartAnimationLoop();
                DarkModeToggle.IsEnabled = false;
                Global.Animations = true;
            }
            else
            {
                var compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;
                var animator = new BackgroundAnimator(compositor, backgroundgrid);
                animator.RemoveBackgroundToGrid(backgroundgrid);
                DarkModeToggle.IsEnabled = true;
                Global.Animations = false;
            }

            // Save the settings to JSON
            SaveSettings(logPath, animationsEnabled, darkModeEnabled);

            bool yesorno = await DialogClass.ShowYesNoDialogAsync("Confirm Settings", "Would you like to continue with this configuration?");
            if (yesorno)
            {
                MainWindow.Navigate(typeof(MainMenu));
            }
        }

        private void SaveSettings(string logPath, bool animationsEnabled, bool darkModeEnabled)
        {
            var settings = new UserSettings
            {
                AnimationSpeed = AnimationSpeed.Value,
                ColorTransitionDuration = AnimationColorTransDurit.Value,
                StopMovementDuration = StopMovementDuration.Value,
                DebugLogPath = logPath,
                AnimationsEnabled = animationsEnabled,
                DarkModeEnabled = darkModeEnabled
            };

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented); // Use JsonConvert
            string settingsPath = Path.Combine(Global.DownloadPath, SettingsFileName);

            File.WriteAllText(settingsPath, json);
        }

        private void languageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = languageSelector.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
            {
                string selectedLanguage = selectedItem.Content.ToString();
                Logger.Log("Selected Language: " + selectedLanguage);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Handle language selection confirmation logic here
            Logger.Log("Language selection confirmed.");
            MainWindow.Navigate(typeof(MainMenu));
        }

        private void AnimationsToggle_Toggled(object sender, RoutedEventArgs e)
        {
            AnimationOptionsPanel3.Visibility = AnimationsToggle.IsOn ? Visibility.Visible : Visibility.Collapsed;
            AnimationOptionsPanel2.Visibility = AnimationsToggle.IsOn ? Visibility.Visible : Visibility.Collapsed;
            AnimationOptionsPanel1.Visibility = AnimationsToggle.IsOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DarkModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // Implement dark mode toggle functionality here, if needed
        }
    }
}
