using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ProjectCodeEditor.Dialogs
{
    public sealed partial class FindDialog : ContentDialog, IModalDialog
    {
        internal ContentDialogResult Result;

        internal string FindText;

        private string TitleStringId = "FindItem/Text";

        private string CancelStringId = "CancelText";

        public FindDialog() => InitializeComponent();

        public async Task<bool> Show()
        {
            Closing += FindDialog_Closing;
            var appSettings = Singleton<SettingsViewModel>.Instance;
            if (appSettings.DialogShown) return false;
            else appSettings.DialogShown = true;
            try
            {
                await ShowAsync();
            }
            catch (Exception)
            {
                Closing -= FindDialog_Closing;
                Hide();
                return false;
            }

            return true;
        }

        private void FindDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            Singleton<SettingsViewModel>.Instance.DialogShown = false;
            Closing -= FindDialog_Closing;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            FindText = Input.Text;
            Result = ContentDialogResult.Primary;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Result = ContentDialogResult.None;
    }
}
