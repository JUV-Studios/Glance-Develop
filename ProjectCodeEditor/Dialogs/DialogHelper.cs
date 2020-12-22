using System;
using System.Runtime.InteropServices;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Dialogs
{
    public static class DialogHelper
    {
        public static async void ShowPlusBlock(ContentDialog dialog, Action<object> continuation)
        {
            if (!App.AppSettings.DialogShown)
            {
                App.AppSettings.DialogShown = true;
                var result = await dialog.ShowAsync();
                if (continuation != null) continuation(result);
                App.AppSettings.DialogShown = false;
            }
        }
    }
}
