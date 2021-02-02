using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ProjectCodeEditor.Dialogs
{
    public sealed partial class HelpDialog : ContentDialog
    {
        public readonly string OkayStringId = "OkayText";

        public HelpDialog()
        {
            InitializeComponent();
        }
    }
}
