using System;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Dialogs
{
    public static class DialogHelper
    {
        public static bool PreparePresentation()
        {
            if (App.AppSettings.DialogShown) return false;
            App.AppSettings.DialogShown = true;
            return true;
        }

        public static void EndPresentation()
        {
            if (App.AppSettings.DialogShown) App.AppSettings.DialogShown = false;
        }

        public static async void ShowPlusBlock(ContentDialog dialog, Action<ContentDialogResult> continuation)
        {
            if (PreparePresentation())
            {
                var result = await dialog.ShowAsync();
                continuation?.Invoke(result);
                EndPresentation();
            }
        }
    }
}
