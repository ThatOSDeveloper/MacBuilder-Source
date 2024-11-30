using MacBuilder.Core.Logger;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI;
using System.Linq;

namespace MacBuilder_GUI.UI
{
    using System;
    using System.Linq; // Ensure this is included
    using MacBuilder.Core.Global;
    using Microsoft.UI.Composition;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Hosting;
    using Windows.UI;

    public class BackgroundAnimator
    {
        private readonly Compositor _compositor;
        private CompositionLinearGradientBrush _compositionGradientBrush;
        private SpriteVisual _backgroundVisual;
        private FrameworkElement grid;
        private static readonly Random random = new Random();

        public double ColorTransitionDuration { get; set; } = Global.ColorTransitionDuritation;
        public double StopMovementDuration { get; set; } = Global.StopsMovementDuration;
        public double AnimationSpeed { get; set; } = Global.AnimationSpeed;

        public BackgroundAnimator(Compositor compositor, UIElement gridElement)
        {
            _compositor = compositor ?? throw new ArgumentNullException(nameof(compositor), "Compositor is null");

            _compositionGradientBrush = _compositor.CreateLinearGradientBrush();
            InitializeGradientStops();

            _compositionGradientBrush.StartPoint = new Vector2(0, 0);
            _compositionGradientBrush.EndPoint = new Vector2(1, 1);

            Logger.Log("Grid background brush created successfully.");

            ApplyBackgroundToGrid((FrameworkElement)gridElement);
            StartAnimationLoop();
        }

        private void InitializeGradientStops()
        {
            GenerateRandomColors(5); 
        }

        private void GenerateRandomColors(int count)
        {
            try
            {
                _compositionGradientBrush.ColorStops.Clear();

                Color previousColor = Color.FromArgb(255, 0, 0, 0);

                for (int i = 0; i < count; i++)
                {
                    var stop = _compositor.CreateColorGradientStop();
                    stop.Offset = (float)i / (count - 1);
                    stop.Color = GetRandomColor(previousColor);
                    previousColor = stop.Color;
                    _compositionGradientBrush.ColorStops.Add(stop);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to generate random colors! " + ex.Message, Logger.LogLevel.Error);
                RemoveBackgroundToGrid(grid);
            }
        }

        private Color GetRandomColor(Color previousColor)
        {
            byte r = (byte)random.Next(Math.Max(0, previousColor.R - 50), Math.Min(255, previousColor.R + 50));
            byte g = (byte)random.Next(Math.Max(0, previousColor.G - 50), Math.Min(255, previousColor.G + 50));
            byte b = (byte)random.Next(Math.Max(0, previousColor.B - 50), Math.Min(255, previousColor.B + 50));
            return Color.FromArgb(255, r, g, b);
        }


        public async void StartAnimationLoop()
        {
            while (true)
            {
                await AnimateGradientColors();
                await MoveColorStops();
                await Task.Delay((int)(1000 / AnimationSpeed));
            }
        }

        private async Task AnimateGradientColors()
        {
            try
            {
                for (int i = 0; i < _compositionGradientBrush.ColorStops.Count; i++)
                {
                    var colorStop = _compositionGradientBrush.ColorStops[i];
                    int nextIndex = (i + 1) % _compositionGradientBrush.ColorStops.Count;

                    Color startColor = colorStop.Color;

                    Color endColor = GetRandomColor(startColor);

                    var animation = _compositor.CreateColorKeyFrameAnimation();
                    animation.Duration = TimeSpan.FromSeconds(ColorTransitionDuration / AnimationSpeed);

                    animation.InsertKeyFrame(0f, startColor);
                    animation.InsertKeyFrame(1f, endColor);

                    colorStop.StartAnimation("Color", animation);
                }

                await Task.Delay((int)(ColorTransitionDuration * 1000 / AnimationSpeed));
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to animate gradient colors! " + ex.Message, Logger.LogLevel.Error);
                RemoveBackgroundToGrid(grid);
            }
        }

        private async Task MoveColorStops()
        {
            try
            {
                for (int i = 0; i < _compositionGradientBrush.ColorStops.Count; i++)
                {
                    var colorStop = _compositionGradientBrush.ColorStops[i];
                    var animation = _compositor.CreateScalarKeyFrameAnimation();
                    animation.InsertKeyFrame(0f, colorStop.Offset);

                    float direction = (float)(random.NextDouble() * 0.05 - 0.025);
                    animation.InsertKeyFrame(1f, colorStop.Offset + direction);
                    animation.Duration = TimeSpan.FromSeconds(StopMovementDuration / AnimationSpeed);

                    colorStop.StartAnimation("Offset", animation);
                }

                await Task.Delay(50); // Short delay to allow transitions to take effect
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to move color stops! " + ex.Message, Logger.LogLevel.Error);
                RemoveBackgroundToGrid(grid);
            }
        }

        public void RemoveBackgroundToGrid(FrameworkElement gridElement)
        {
            try
            {
                ElementCompositionPreview.SetElementChildVisual(gridElement, null);
                grid = null;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to remove animation from background! " + ex.Message, Logger.LogLevel.Error);
            }
        }

        public void ApplyBackgroundToGrid(FrameworkElement gridElement)
        {
            try
            {
                if (grid != null)
                {
                    Logger.Log("Animation already applied!", Logger.LogLevel.Error);
                    return;
                }
                _backgroundVisual = _compositor.CreateSpriteVisual();
                _backgroundVisual.Brush = _compositionGradientBrush;

                _backgroundVisual.Size = new Vector2((float)gridElement.ActualWidth, (float)gridElement.ActualHeight);
                _backgroundVisual.RelativeSizeAdjustment = new Vector2(1f, 1f);

                grid = gridElement;

                ElementCompositionPreview.SetElementChildVisual(gridElement, _backgroundVisual);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to apply animation to background! " + ex.Message, Logger.LogLevel.Error);
                RemoveBackgroundToGrid(grid);
            }
        }
    }

    public class ButtonAnimation
    {
        private Compositor _compositor;
        private UIElement _targetElement;

        public ButtonAnimation(Compositor compositor, UIElement targetElement)
        {
            _compositor = compositor;
            _targetElement = targetElement;
        }
        // Create a scale animation
        public Vector3KeyFrameAnimation CreateScaleAnimation(float from, float to)
        {
            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0f, new Vector3(from, from, 1f));
            scaleAnimation.InsertKeyFrame(1f, new Vector3(to, to, 1f));
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(200);
            scaleAnimation.Target = "Scale";
            return scaleAnimation;
        }

        // Create a color animation
        public ColorKeyFrameAnimation CreateColorAnimation(Color fromColor, Color toColor)
        {
            var colorAnimation = _compositor.CreateColorKeyFrameAnimation();
            colorAnimation.InsertKeyFrame(0f, fromColor);
            colorAnimation.InsertKeyFrame(1f, toColor);
            colorAnimation.Duration = TimeSpan.FromMilliseconds(200);
            return colorAnimation;
        }

        public void AnimateButtonOnHover(Button button)
        {
            // Create a SolidColorBrush for the button background
            var solidColorBrush = new SolidColorBrush(Colors.Gray);
            button.Background = solidColorBrush;

            // Create a SpriteVisual for the button
            var buttonVisual = _compositor.CreateSpriteVisual();
            buttonVisual.Size = new Vector2((float)button.ActualWidth, (float)button.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(button, buttonVisual);

            // Create a CompositionColorBrush for hover animations
            var colorBrush = _compositor.CreateColorBrush(Colors.Gray);
            buttonVisual.Brush = colorBrush; // Set the visual's brush

            button.PointerEntered += (s, e) => OnPointerEntered(buttonVisual, colorBrush);
            button.PointerExited += (s, e) => OnPointerExited(buttonVisual, colorBrush);
        }

        private void OnPointerEntered(SpriteVisual buttonVisual, CompositionColorBrush colorBrush)
        {
            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0f, new Vector3(1f, 1f, 1f));
            scaleAnimation.InsertKeyFrame(1f, new Vector3(1.05f, 1.05f, 1f));
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(200);
            scaleAnimation.Target = "Scale";
            buttonVisual.StartAnimation("Scale", scaleAnimation);

            var colorAnimation = _compositor.CreateColorKeyFrameAnimation();
            colorAnimation.InsertKeyFrame(0f, Colors.Gray);
            colorAnimation.InsertKeyFrame(1f, Colors.LightGray);
            colorAnimation.Duration = TimeSpan.FromMilliseconds(200);
            colorBrush.StartAnimation("Color", colorAnimation); // Now animating the CompositionColorBrush
        }

        private void OnPointerExited(SpriteVisual buttonVisual, CompositionColorBrush colorBrush)
        {
            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0f, new Vector3(1.05f, 1.05f, 1f));
            scaleAnimation.InsertKeyFrame(1f, new Vector3(1f, 1f, 1f));
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(200);
            scaleAnimation.Target = "Scale";
            buttonVisual.StartAnimation("Scale", scaleAnimation);

            var colorAnimation = _compositor.CreateColorKeyFrameAnimation();
            colorAnimation.InsertKeyFrame(0f, Colors.LightGray);
            colorAnimation.InsertKeyFrame(1f, Colors.Gray);
            colorAnimation.Duration = TimeSpan.FromMilliseconds(200);
            colorBrush.StartAnimation("Color", colorAnimation); // Now animating the CompositionColorBrush
        }
    }
    public class ScaleAnimation
    {
        private Compositor _compositor;
        private UIElement _targetElement;

        public ScaleAnimation(Compositor compositor, UIElement targetElement)
        {
            _compositor = compositor;
            _targetElement = targetElement;
        }

        public void Start()
        {
            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0f, new System.Numerics.Vector3(1f, 1f, 1f));
            scaleAnimation.InsertKeyFrame(1f, new System.Numerics.Vector3(1.05f, 1.05f, 1f)); // Scale to 105%
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(200); // Duration of the animation

            var animationGroup = _compositor.CreateAnimationGroup();
            animationGroup.Add(scaleAnimation);

            var visual = ElementCompositionPreview.GetElementVisual(_targetElement);
            visual.StartAnimation("Scale.X", scaleAnimation);
            visual.StartAnimation("Scale.Y", scaleAnimation);
        }

        public void Reverse()
        {
            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0f, new System.Numerics.Vector3(1.05f, 1.05f, 1f));
            scaleAnimation.InsertKeyFrame(1f, new System.Numerics.Vector3(1f, 1f, 1f)); // Scale back to 100%
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(200); // Duration of the animation

            var visual = ElementCompositionPreview.GetElementVisual(_targetElement);
            visual.StartAnimation("Scale.X", scaleAnimation);
            visual.StartAnimation("Scale.Y", scaleAnimation);
        }
    }
    public class NavigationAnimator
    {
        private readonly Compositor _compositor;
        private readonly Frame _frame;
        private const int DefaultDuration = 500;

        public NavigationAnimator(Frame frame)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(frame).Compositor;
            _frame = frame;
        }

        public async Task NavigateWithSlideInAsync(Type pageType, int duration = DefaultDuration, string direction = "right")
        {
            var incomingPage = new Frame();
            incomingPage.Navigate(pageType);

            var visual = ElementCompositionPreview.GetElementVisual(incomingPage);
            visual.Opacity = 0f; // Start with the page hidden

            visual.Offset = GetStartOffset(direction);

            _frame.Content = incomingPage;

            // Create animations for slide-in
            var offsetAnimation = CreateOffsetAnimation(visual, duration, direction, true);
            var opacityAnimation = CreateOpacityAnimation(visual, duration, true);

            // Start animations
            visual.StartAnimation("Offset", offsetAnimation);
            visual.StartAnimation("Opacity", opacityAnimation);

            // Wait for animation to complete
            await Task.Delay(duration);

            // Ensure the page is fully visible after animation
            visual.Opacity = 1f;
        }

        public async Task NavigateWithSlideOutAsync(int duration = DefaultDuration, string direction = "left")
        {
            var outgoingPage = _frame.Content as UIElement;

            if (outgoingPage == null) return;

            var visual = ElementCompositionPreview.GetElementVisual(outgoingPage);

            // Create animations for slide-out
            var offsetAnimation = CreateOffsetAnimation(visual, duration, direction, false);
            var opacityAnimation = CreateOpacityAnimation(visual, duration, false);

            // Start animations
            visual.StartAnimation("Offset", offsetAnimation);
            visual.StartAnimation("Opacity", opacityAnimation);

            await Task.Delay(duration);

            // Clear the frame content after the outgoing animation
            _frame.Content = null;
        }

        private ScalarKeyFrameAnimation CreateOpacityAnimation(Visual visual, int duration, bool fadeIn = true)
        {
            var opacityAnimation = _compositor.CreateScalarKeyFrameAnimation();
            opacityAnimation.InsertKeyFrame(0f, fadeIn ? 0f : 1f); // Start invisible or visible based on fadeIn
            opacityAnimation.InsertKeyFrame(1f, fadeIn ? 1f : 0f); // Fade in or out
            opacityAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            return opacityAnimation;
        }

        private Vector3KeyFrameAnimation CreateOffsetAnimation(Visual visual, int duration, string direction, bool slideIn = true)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(0f, slideIn ? GetStartOffset(direction) : GetEndOffset(direction)); // Start position based on slide direction
            offsetAnimation.InsertKeyFrame(1f, slideIn ? new Vector3(0, 0, 0) : GetEndOffset(direction)); // End position
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            return offsetAnimation;
        }

        private Vector3 GetStartOffset(string direction)
        {
            return direction switch
            {
                "right" => new Vector3((float)_frame.ActualWidth, 0, 0),
                "left" => new Vector3((float)-_frame.ActualWidth, 0, 0),
                "top" => new Vector3(0, (float)-_frame.ActualHeight, 0),
                "bottom" => new Vector3(0, (float)_frame.ActualHeight, 0),
                _ => new Vector3((float)_frame.ActualWidth, 0, 0), // Default to right
            };
        }

        private Vector3 GetEndOffset(string direction)
        {
            return direction switch
            {
                "right" => new Vector3((float)-_frame.ActualWidth, 0, 0),
                "left" => new Vector3((float)_frame.ActualWidth, 0, 0),
                "top" => new Vector3(0, (float)_frame.ActualHeight, 0),
                "bottom" => new Vector3(0, (float)-_frame.ActualHeight, 0),
                _ => new Vector3((float)-_frame.ActualWidth, 0, 0), // Default to left
            };
        }
    }
}
