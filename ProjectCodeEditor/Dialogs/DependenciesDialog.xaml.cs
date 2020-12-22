using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ProjectCodeEditor.Dialogs
{
    public sealed partial class DependenciesDialog : ContentDialog
    {
        public static readonly string DependenciesBlockTextId = "DependenciesListBlock/Text";

        public readonly string OkayStringId = "OkayText";

        public DependenciesDialog()
        {
            InitializeComponent();
        }
    }
}
