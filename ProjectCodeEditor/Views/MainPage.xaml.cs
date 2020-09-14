using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ProjectCodeEditor.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();
        private bool reload = false;

        public MainPage()
        {
            InitializeComponent();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadRecentItems();
            ViewModel.RecentListChanged += ViewModel_RecentListChanged;
            ShowHideCommandBar();
            EditorShellViewModel.FrameChanged += EditorShellViewModel_FrameChanged;
            EditorShellViewModel.FrameNavigationCompleted += EditorShellViewModel_FrameNavigationCompleted;
        }

        private void ViewModel_RecentListChanged(object sender, EventArgs e) => ShowHideCommandBar();

        private void EditorShellViewModel_FrameNavigationCompleted(object sender, EventArgs e)
        {
            if (reload)
            {
                ViewModel.LoadRecentItems();
            }
        }

        private void EditorShellViewModel_FrameChanged(object sender, ShellView e)
        {
            if (e != null)
            {
                reload = e.DisplayName == "Recent";
            }
        }

        private void RecentList_DoubleClick(object sender, DoubleTappedRoutedEventArgs e)
        {
            var clickedItem = recentList.SelectedItem as RecentItem;
            if (clickedItem.IsWeb)
            {
                EditorShellViewModel.AddWebPage(clickedItem.Location);
            }
            else
            {
                EditorShellViewModel.AddFile(clickedItem.FileHandle);
            }
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e) => ViewModel.RemoveRecentItem(recentList.SelectedIndex);

        private void recentList_SelectionChanged(object sender, SelectionChangedEventArgs e) => ViewModel.ItemSelected = recentList.SelectedItem != null;

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e) => ShowHideCommandBar();

        private void ShowHideCommandBar()
        {
            if ((pivot.SelectedItem as PivotItem).Tag.ToString() == "Recent" && !ViewModel.IsEmpty)
            {
                RecentsCommandBar.Visibility = Visibility.Visible;
            }
            else
            {
                RecentsCommandBar.Visibility = Visibility.Collapsed;
            }
        }
    }
}
