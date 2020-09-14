using ProjectCodeEditor.ViewModels;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace ProjectCodeEditor.Views
{
    public sealed partial class EditorShellPage : Page
    {
        private readonly EditorShellViewModel ViewModel = App.ShellViewModel;

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

        private void masterDetail_SelectionChanged(object sender, SelectionChangedEventArgs e) => ViewModel.InvokeFrameNavigationCompleted(this);

        private void Close_Click(object sender, RoutedEventArgs e) => ViewModel.RemoveSelectedItem();

        private void NavigateToNumberedTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {

        }

        private void Browser(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => ViewModel.AddWebPage();

        private void MemoryFree(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => GC.Collect();

        private void Browser_Click(object sender, RoutedEventArgs e) => ViewModel.AddWebPage();
    }
}
