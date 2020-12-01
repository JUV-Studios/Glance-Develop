using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ProjectCodeEditor.Dialogs
{
    public sealed partial class FindDialog : ContentDialog
    {
        internal ContentDialogResult Result;

        internal string FindText;

        private string TitleStringId = "FindItem/Text";

        private string CancelStringId = "CancelText";

        public FindDialog() => InitializeComponent();

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            FindText = Input.Text;
            Result = ContentDialogResult.Primary;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Result = ContentDialogResult.None;
    }
}
