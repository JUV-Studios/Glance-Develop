using ProjectCodeEditor.Models;
using ProjectCodeEditor.Services;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ProjectCodeEditor.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            ViewService.KeyShortcutPressed += ViewService_KeyShortcutPressed;
        }

        private async void CloseCurrent_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            var item = Preferences.AppShellViewModel.SelectedItem;
            if (item.Content is IClosable closable)
            {
                if (await closable.CloseAsync()) Preferences.AppShellViewModel.TerminateInstance(item);
            }
        }

        private void ViewService_KeyShortcutPressed(object sender, KeyShortcutPressedEventArgs e)
        {
            if (e.Accelerator.Key == VirtualKey.Tab && e.Accelerator.Modifiers == VirtualKeyModifiers.Control && e.Accelerator.IsEnabled)
            {
                e.SystemArgs.Handled = true;
                Preferences.AppShellViewModel.FastSwitch();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(DragRegion);
        }
    }
}
