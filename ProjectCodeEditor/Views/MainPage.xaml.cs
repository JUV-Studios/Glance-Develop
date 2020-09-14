using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ProjectCodeEditor.Views
{
    public sealed partial class MainPage : BaseLayout
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();
        private Type SettingsPageType
        {
            get => typeof(SettingsPage);
        }

        public MainPage()
        {
            InitializeComponent();
        }

        private void ViewModel_RecentListChanged(object sender, EventArgs e) => ShowHideCommandBar();

        private void RecentList_DoubleClick(object sender, DoubleTappedRoutedEventArgs e)
        {
            var clickedItem = recentList.SelectedItem as RecentItem;
            if (clickedItem.IsWeb)
            {
                App.ShellViewModel.AddWebPage(clickedItem.Location);
            }
            else
            {
                App.ShellViewModel.AddFile(clickedItem.FileHandle);
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

        public override void OnLoad()
        {
            ViewModel.LoadRecentItems();
            ViewModel.RecentListChanged += ViewModel_RecentListChanged;
        }

        public override void OnSuspend()
        {
            ViewModel.RecentListChanged -= ViewModel_RecentListChanged;
            ViewModel.ClearRecentItems();
        }

        public override void OnResume() => OnLoad();

        public override void Dispose() => ViewModel.ClearRecentItems();

        private void BaseLayout_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ShowHideCommandBar();
        }
    }
}
