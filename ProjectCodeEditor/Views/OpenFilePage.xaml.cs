using ProjectCodeEditor.Services;
using ProjectCodeEditor.ViewModels;
using System;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ProjectCodeEditor.Views
{
    public sealed partial class OpenFilePage : Page
    {
        private ApplicationView _CurrentView;
        private SystemNavigationManager _NavigationManager;
        public OpenFileViewModel ViewModel { get; } = new OpenFileViewModel();

        public OpenFilePage()
        {
            InitializeComponent();
        }

        private async void Browse_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            openPicker.FileTypeFilter.Add(".md");
            openPicker.FileTypeFilter.Add(".json");
            openPicker.FileTypeFilter.Add(".xml");
            openPicker.FileTypeFilter.Add(".yaml");
            openPicker.FileTypeFilter.Add(".yml");
            openPicker.FileTypeFilter.Add("*");

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                NavigationService.Navigate(typeof(EditorPage), file);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _CurrentView = ApplicationView.GetForCurrentView();
            _NavigationManager = SystemNavigationManager.GetForCurrentView();
            _CurrentView.Title = title.Text;
            _CurrentView.TitleBar.ButtonBackgroundColor = new UISettings().GetColorValue(UIColorType.Background);
            _NavigationManager.BackRequested += _NavigationManager_BackRequested;
            Window.Current.SizeChanged += Current_SizeChanged;
            UpdateBackButtonVisibility();
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            e.Handled = true;
            UpdateBackButtonVisibility();
        }

        private void UpdateBackButtonVisibility()
        {
            if (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Touch)
            {
                ViewModel.ShowBackButton = false;
            }
            else
            {
                ViewModel.ShowBackButton = true;
            }

            dataGrid.SelectedIndex = 0;
        }

        private void _NavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            NavigationService.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _CurrentView.Title = string.Empty;
            _NavigationManager.BackRequested -= _NavigationManager_BackRequested;
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.LoadSelectableFiles();
        }

        private void OpenSelected_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
