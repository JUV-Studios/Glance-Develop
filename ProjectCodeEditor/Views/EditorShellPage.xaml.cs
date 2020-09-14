using ProjectCodeEditor.ViewModels;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace ProjectCodeEditor.Views
{
    public sealed partial class EditorShellPage : Page
    {
        private EditorShellViewModel ViewModel = EditorShellViewModel.Instance;

        public EditorShellPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void FastSwitch(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            var index = ViewModel.Instances.IndexOf(ViewModel.SelectedItem);
            if (ViewModel.Instances.Count != 1)
            {
                if (index == ViewModel.Instances.Count - 1) ViewModel.SelectedItem = ViewModel.Instances.First();
                else
                {
                    ViewModel.SelectedItem = ViewModel.Instances[index + 1];
                }
            }
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Browser_Click(object sender, RoutedEventArgs e) => EditorShellViewModel.AddWebPage(null);

        private void masterDetail_SelectionChanged(object sender, SelectionChangedEventArgs e) => EditorShellViewModel.InvokeFrameNavigationCompleted(this);

        private void Close_Click(object sender, RoutedEventArgs e) => EditorShellViewModel.RemoveSelectedItem();

        private void NavigateToNumberedTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {

        }
    }
}
