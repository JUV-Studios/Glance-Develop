using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using System;
using System.Linq;
using Windows.System;
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

        private void masterDetail_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
            }
            catch (InvalidOperationException)
            {
                // Occurs when drag and drop is not properly finished
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => ViewModel.TerminateSelected();

        private void NavigateToNumberedTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            int tabToSelect = 0;

            switch (sender.Key)
            {
                case VirtualKey.Number1:
                    tabToSelect = 0;
                    break;
                case VirtualKey.Number2:
                    tabToSelect = 1;
                    break;
                case VirtualKey.Number3:
                    tabToSelect = 2;
                    break;
                case VirtualKey.Number4:
                    tabToSelect = 3;
                    break;
                case VirtualKey.Number5:
                    tabToSelect = 4;
                    break;
                case VirtualKey.Number6:
                    tabToSelect = 5;
                    break;
                case VirtualKey.Number7:
                    tabToSelect = 6;
                    break;
                case VirtualKey.Number8:
                    tabToSelect = 7;
                    break;
                case VirtualKey.Number9:
                    // Select the last tab
                    tabToSelect = ViewModel.Instances.Count - 1;
                    break;
            }

            // Only select the tab if it is in the list
            if (tabToSelect < ViewModel.Instances.Count)
            {
                ViewModel.SelectedItem = ViewModel.Instances[tabToSelect];
            }
        }

        private void Browser(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            ViewModel.AddWebPage();
        }

        private void MemoryFree(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) => GC.Collect();

        private void Close(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            Close_Click(null, null);
        }
    }
}
