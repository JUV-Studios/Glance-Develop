using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ProjectCodeEditor.Dialogs
{
    public sealed partial class UnsavedChangesDialog : IModalDialog
    {
        internal ContentDialogResult Result;

        private string CancelTextId = "CancelText";

        public UnsavedChangesDialog()
        {
            InitializeComponent();
        }

        private void UnsavedChangesDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            Singleton<SettingsViewModel>.Instance.DialogShown = false
            Closing -= UnsavedChangesDialog_Closing;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Result = ContentDialogResult.Primary;

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Result = ContentDialogResult.Secondary;

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Result = ContentDialogResult.None;

        public async Task<bool> Show()
        {
            Closing += UnsavedChangesDialog_Closing;
            var appSettings = Singleton<SettingsViewModel>.Instance;
            if (appSettings.DialogShown) return false;
            else appSettings.DialogShown = true;
            try
            {
                await ShowAsync();
            }
            catch (Exception)
            {
                Closing -= UnsavedChangesDialog_Closing;
                Hide();
                return false;
            }

            return true;
        }
    }
}
