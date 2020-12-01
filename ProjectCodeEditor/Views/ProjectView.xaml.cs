using Windows.Storage;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectCodeEditor.Views
{
    public sealed partial class ProjectView : UserControl
    {
        public ProjectView(StorageFolder folder)
        {
            InitializeComponent();
        }
    }
}
