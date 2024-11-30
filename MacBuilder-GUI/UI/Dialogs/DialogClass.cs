using MacBuilder.Core.Global;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Assets.Dialogs
{
    public class DialogClass
    {
        public static async void MessageBox(string Title, object Content)
        {
            try
            {
                var openedpopups = VisualTreeHelper.GetOpenPopups(Global.m_window);
                foreach (var popup in openedpopups)
                {
                    if (popup.Child is ContentDialog)
                    {
                        return;
                    }
                }
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = Global.m_window.Content.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = Title;
                dialog.Content = Content;

                dialog.CloseButtonText = "OK";

                await dialog.ShowAsync();
            }
            catch { }
        }
        public static async Task<bool> ShowYesNoDialogAsync(string title, object content)
        {
            try
            {
                var openedPopups = VisualTreeHelper.GetOpenPopups(Global.m_window);
                foreach (var popup in openedPopups)
                {
                    if (popup.Child is ContentDialog)
                    {
                        return false;
                    }
                }

                ContentDialog dialog = new ContentDialog
                {
                    XamlRoot = Global.m_window.Content.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = title,
                    Content = content,
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No",
                    CloseButtonText = null
                };

                ContentDialogResult result = await dialog.ShowAsync();

                return result == ContentDialogResult.Primary;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
