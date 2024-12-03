using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using Windows.UI;
using MacBuilder_GUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using MacBuilder.Core.Logger;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using System;
using MacBuilder_GUI.Core.Components.Extras.Updater;

namespace MacBuilder_GUI
{
    public sealed partial class MainMenu : Page
    {
        private ButtonAnimation _buttonanimator;

        private Compositor _compositor;
        public MainMenu()
        {
            this.InitializeComponent();
            Loaded += Page_Loaded;
            UpdaterClass.IsUpdateAvailableAsync();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;
            _buttonanimator = new ButtonAnimation(_compositor, MainGrid);
            // TODO: MAKE GLOBAL IF ANIMATIONS
            // Animate buttons on hover
            AnimateButtonOnHover(newproj);
            AnimateButtonOnHover(loadproj);
            AnimateButtonOnHover(settings);
            AnimateButtonOnHover(exit);
        }

        private void AnimateButtonOnHover(Button button)
        {
            // Create a CompositionColorBrush for the button background
            var colorBrush = _compositor.CreateColorBrush(Colors.Gray);
            button.Background = new SolidColorBrush(Colors.Transparent); // Placeholder for UI element

            // Handle pointer entered and exited events
            button.PointerEntered += (s, e) => OnPointerEntered(button, colorBrush);
            button.PointerExited += (s, e) => OnPointerExited(button, colorBrush);
        }

        private void OnPointerEntered(Button button, CompositionColorBrush colorBrush)
        {
            var visual = ElementCompositionPreview.GetElementVisual(button);
            var scaleAnimation = _buttonanimator.CreateScaleAnimation(1f, 1.05f);
            visual.StartAnimation("Scale", scaleAnimation);

            // Create a color animation for the button background
            var colorAnimation = _compositor.CreateColorKeyFrameAnimation();
            colorAnimation.InsertKeyFrame(0f, colorBrush.Color);
            colorAnimation.InsertKeyFrame(1f, Colors.LightGray);
            colorAnimation.Duration = TimeSpan.FromMilliseconds(200);
            colorBrush.StartAnimation("Color", colorAnimation);
        }

        private void OnPointerExited(Button button, CompositionColorBrush colorBrush)
        {
            var visual = ElementCompositionPreview.GetElementVisual(button);
            var scaleAnimation = _buttonanimator.CreateScaleAnimation(1.05f, 1f);
            visual.StartAnimation("Scale", scaleAnimation);

            // Create a color animation for the button background
            var colorAnimation = _compositor.CreateColorKeyFrameAnimation();
            colorAnimation.InsertKeyFrame(0f, Colors.LightGray);
            colorAnimation.InsertKeyFrame(1f, colorBrush.Color);
            colorAnimation.Duration = TimeSpan.FromMilliseconds(200);
            colorBrush.StartAnimation("Color", colorAnimation);
        }

        private void Button_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var scaleAnimation = new ScaleAnimation(_compositor, button);
                scaleAnimation.Start();
            }
        }

        private void Button_MouseLeave(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var scaleAnimation = new ScaleAnimation(_compositor, button);
                scaleAnimation.Reverse();
            }
        }
        private void newproj_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(typeof(BaseSelector));
        }

        private void loadproj_Click(object sender, RoutedEventArgs e)
        {

        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(typeof(InitialSetup));
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("Goodbye!", Logger.LogLevel.Error);
            Task.Delay(1000);
            Windows.ApplicationModel.Core.CoreApplication.Exit();
        }
    }
}
