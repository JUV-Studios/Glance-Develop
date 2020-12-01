using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ProjectCodeEditor.Dialogs
{
    public sealed partial class UnsavedChangesDialog : ContentDialog
    {
        internal ContentDialogResult Result;

        private string CancelTextId = "CancelText";

        public UnsavedChangesDialog() => InitializeComponent();

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Result = ContentDialogResult.Primary;

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Result = ContentDialogResult.Secondary;

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Result = ContentDialogResult.None;
    }
}
