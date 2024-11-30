using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MacBuilder_Installer
{
    public sealed partial class InstallerPage : Page
    {
        private Compositor _compositor;
        private Visual _buttonVisual;
        private Visual _successTextVisual;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string[] quotes =
        {
            "Hackintosh: Easier than ever before!",
            "Build your Mac your way.",
            "Transform your PC into a Mac.",
            "Unlock the power of Hackintosh."
        };

        public InstallerPage()
        {
            InitializeComponent();
            InitializeCompositor();
            DisplayRandomQuote();
            PathTextBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "MacBuilder");
        }

        private void InitializeCompositor()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _buttonVisual = ElementCompositionPreview.GetElementVisual(InstallButton);
            _successTextVisual = ElementCompositionPreview.GetElementVisual(SuccessText);
        }

        private void DisplayRandomQuote()
        {
            Random random = new Random();
            QuoteTextBlock.Text = quotes[random.Next(quotes.Length)];
        }

        private async void BrowsePathButton_Click(object sender, RoutedEventArgs e)
        {
            var folder = await PickFolderAsync();
            if (folder != null)
            {
                PathTextBox.Text = folder.Path;
            }
        }

        private async Task<StorageFolder> PickFolderAsync()
        {
            var picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            picker.FileTypeFilter.Add("*");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            return await picker.PickSingleFolderAsync();
        }

        private void InstallButton_PointerEntered(object sender, PointerRoutedEventArgs e) => AnimateButtonScale(1.1f);
        private void InstallButton_PointerExited(object sender, PointerRoutedEventArgs e) => AnimateButtonScale(1f);

        private void AnimateButtonScale(float scale)
        {
            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(1f, new System.Numerics.Vector3(scale, scale, 1));
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(200);
            _buttonVisual.CenterPoint = new System.Numerics.Vector3((float)InstallButton.ActualWidth / 2, (float)InstallButton.ActualHeight / 2, 0);
            _buttonVisual.StartAnimation("Scale", scaleAnimation);
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = false;

            string realpath = GetInstallationPath();
            if (!await CheckConnection()) return;

            if (!await SetEnvironmentVariable("MacBuilder", realpath)) return;

            if (await ShowELUADialog())
            {
                if (!Directory.Exists(realpath)) Directory.CreateDirectory(realpath);

                await Notify("Downloading content...");
                await DownloadLatestMHCC(realpath);
                await DownloadLatestUpdater(realpath);
                await DownloadLatestMacBuilder(realpath);
                await Notify("Installation completed successfully!");
            }
            else
            {
                await ShowErrorDialog("EULA Agreement", "You must accept the End User License Agreement to proceed with using MacBuilder.", "Error Code: MB-002");
                InstallButton.IsEnabled = true;
            }
        }

        private string GetInstallationPath()
        {
            return !string.IsNullOrEmpty(PathTextBox.Text)
                ? Path.Combine(PathTextBox.Text, "MacBuilder")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "MacBuilder");
        }

        private async Task<bool> SetEnvironmentVariable(string variableName, string value)
        {
            string existingPath = Environment.GetEnvironmentVariable(variableName);

            if (!string.IsNullOrEmpty(existingPath))
            {
                if (!await ShowYesNoDialogAsync("Warning", $"The '{variableName}' environment variable already exists with the path: {existingPath}. If you continue, the old installation will be removed."))
                {
                    InstallButton.IsEnabled = true;
                    return false;
                }

                try
                {
                    Directory.Delete(existingPath, true);
                }
                catch (Exception ex)
                {
                    await ShowErrorDialog("Installation Interrupted", $"Failed to delete old installation: {ex.Message}", "Error Code: MB-003");
                    InstallButton.IsEnabled = true;
                    return false;
                }
            }

            Environment.SetEnvironmentVariable(variableName, value, EnvironmentVariableTarget.User);
            return true;
        }

        private async Task<bool> CheckConnection()
        {
            await Notify("Connecting...");
            string url = "http://would-yards.gl.at.ply.gg:15348";

            try
            {
                var response = await _httpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (TaskCanceledException)
            {
                await ShowErrorDialog("Error", "Connection timed out. Please check your internet connection or try again later.", "Error Code: MB-006");
            }
            catch (HttpRequestException)
            {
                await ShowErrorDialog("Error", "MacBuilder servers are currently offline or an update is required!", "Error Code: MB-005");
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Error", $"An unexpected error occurred: {ex.Message}", "Error Code: MB-007");
            }
            finally
            {
                InstallButton.IsEnabled = true;
            }
            return false;
        }

        private async Task<bool> ShowELUADialog()
        {
            string eula = GenerateEulaText();
            return await ShowYesNoDialogAsync("EULA", eula);
        }

        private string GenerateEulaText()
        {
            return "End-User License Agreement (EULA)\r\n\r\n" +
                   "PLEASE READ THIS AGREEMENT CAREFULLY BEFORE USING THE SOFTWARE.\r\n\r\n" +
                   "By downloading, installing, or using the MacBuilder software (hereafter referred to as \"Software\"), you agree to the terms and conditions of this End-User License Agreement (\"EULA\"). If you do not agree to these terms, do not install or use the Software.\r\n\r\n" +
                   "1. License Grant\r\nThis EULA grants you a limited, non-exclusive, non-transferable, revocable license to use the Software strictly in accordance with the terms of this Agreement.\r\n\r\n" +
                   "2. Restrictions\r\nYou agree not to:\r\nReverse engineer, decompile, disassemble, or otherwise attempt to discover the source code of the Software.\r\nModify, adapt, create derivative works based upon, or translate the Software.\r\nCopy, distribute, or make the Software available to any third party without express permission.\r\nUse the Software to create any software or service that is competitive with MacBuilder.\r\nRequest support for this Software from any source outside of official MacBuilder channels.\r\nShare any part of the Software’s code or proprietary information without prior written consent from the MacBuilder team.\r\nRemove or alter any copyright, trademark, or other proprietary notices contained in the Software.\r\n3. Ownership\r\nAll rights, title, and interest in and to the Software, including but not limited to code, content, trademarks, and documentation, are the exclusive property of the MacBuilder team. This EULA does not convey any rights of ownership to you.\r\n\r\n" +
                   "4. Disclaimer of Warranty\r\nThe Software is provided \"as is,\" without warranty of any kind, either express or implied, including, but not limited to, warranties of merchantability, fitness for a particular purpose, or non-infringement.\r\n\r\n" +
                   "5. Limitation of Liability\r\nIn no event will the MacBuilder team be liable for any damages (including, without limitation, lost profits, business interruption, or lost information) arising from the use of or inability to use the Software, even if the MacBuilder team has been advised of the possibility of such damages.\r\n\r\n" +
                   "6. Termination\r\nThis Agreement is effective until terminated. Your rights under this Agreement will terminate automatically without notice from the MacBuilder team if you fail to comply with any of the terms and conditions of this EULA.\r\n\r\n" +
                   "7. Governing Law\r\nThis Agreement will be governed by and construed in accordance with the laws of the State of California, without regard to its conflicts of law provisions.\r\n\r\n" +
                   "8. Entire Agreement\r\nThis EULA constitutes the entire agreement between you and the MacBuilder team and supersedes all prior understandings or agreements, written or oral, regarding the Software.\r\n\r\n" +
                   "By installing or using the Software, you acknowledge that you have read this EULA, understand it, and agree to be bound by its terms.";
        }

        private async Task DownloadLatestMHCC(string path)
        {
            await DownloadFileAsync("http://would-yards.gl.at.ply.gg:15348/downloads/MHCC", path);
        }

        private async Task DownloadLatestUpdater(string path)
        {
            await DownloadFileAsync("http://would-yards.gl.at.ply.gg:15348/downloads/Updater", path);
        }

        private async Task DownloadLatestMacBuilder(string path)
        {
            await DownloadFileAsync("http://would-yards.gl.at.ply.gg:15348/downloads/MacBuilder", path);
        }

        private async Task DownloadFileAsync(string url, string path)
        {
            var fileName = Path.GetFileName(url);
            var filePath = Path.Combine(path, fileName);

            using (var response = await _httpClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }

        private async Task Notify(string message)
        {
            SuccessText.Text = message;

            // Set initial opacity to 0 before starting animation
            SuccessText.Opacity = 0;

            // Create fade-in animation
            var fadeInAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeInAnimation.InsertKeyFrame(1f, 1f);
            fadeInAnimation.Duration = TimeSpan.FromMilliseconds(500);

            // Create fade-out animation
            var fadeOutAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeOutAnimation.InsertKeyFrame(1f, 0f);
            fadeOutAnimation.Duration = TimeSpan.FromMilliseconds(500);

            // Start the fade-in animation
            _successTextVisual.StartAnimation("Opacity", fadeInAnimation);

            // Wait for the fade-in to complete
            await Task.Delay(500); // Wait for fade-in duration

            // Start the fade-out animation
            _successTextVisual.StartAnimation("Opacity", fadeOutAnimation);

            // Wait for the fade-out to complete
            await Task.Delay(500); // Wait for fade-out duration
        }

        private ScalarKeyFrameAnimation CreateFadeAnimation(float finalValue)
        {
            var fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1f, finalValue);
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(200);
            return fadeAnimation;
        }


        public static async Task<bool> ShowYesNoDialogAsync(string title, string content)
        {
            try
            {
                var openedPopups = VisualTreeHelper.GetOpenPopups(App.MainWindow);
                foreach (var popup in openedPopups)
                {
                    if (popup.Child is ContentDialog)
                    {
                        return false; // A dialog is already open
                    }
                }

                ContentDialog dialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No",
                    CloseButtonText = null,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
                };

                dialog.XamlRoot = App.MainWindow.Content.XamlRoot;

                ContentDialogResult result = await dialog.ShowAsync();
                return result == ContentDialogResult.Primary;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        private async Task ShowErrorDialog(string title, string message, string errorCode)
        {
            try
            {
                var openedPopups = VisualTreeHelper.GetOpenPopups(App.MainWindow);
                foreach (var popup in openedPopups)
                {
                    if (popup.Child is ContentDialog)
                    {
                        return; // Return if a dialog is already open
                    }
                }

                ContentDialog dialog = new ContentDialog
                {
                    Title = title,
                    CloseButtonText = "OK",
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
                };

                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

                var messageTextBlock = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                grid.Children.Add(messageTextBlock);

                var errorCodeTextBlock = new TextBlock
                {
                    Text = errorCode,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    FontSize = 12,
                    Opacity = 0.7
                };
                Grid.SetRow(errorCodeTextBlock, 1);
                grid.Children.Add(errorCodeTextBlock);

                dialog.Content = grid;
                dialog.XamlRoot = App.MainWindow.Content.XamlRoot;

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to show error dialog: " + ex.Message);
            }

        }
    }
}
