
using ProjectCodeEditor.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ProjectCodeEditor.Views
{
    public sealed partial class ProjectWorkerPage : Page
    {
        public ProjectWorkerViewModel ViewModel { get; } = new ProjectWorkerViewModel();

        public ProjectWorkerPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
        }
    }
}
