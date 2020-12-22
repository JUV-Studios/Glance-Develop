using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WinUI = Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ProjectCodeEditor.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public readonly ShellViewModel ViewModel = Singleton<ShellViewModel>.Instance;

        public readonly string OpenLabelId = "OpenOption/Label";

        public MainPage() => InitializeComponent();

        private void TabView_TabCloseRequested(WinUI.TabView sender, WinUI.TabViewTabCloseRequestedEventArgs args) => ViewModel.CloseInstance(args.Item as ShellView);

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(DragRegion);
        }

        private void CloseCurrent_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            var item = ViewModel.Instances[ViewModel.SelectedIndex];
            if (item.CanClose) ViewModel.CloseInstance(item);
        }
    }
}
