using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using System;
using System.IO;
using Windows.System;
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
            Loaded += BaseLayout_Loaded;
        }

        private void ViewModel_RecentListChanged(object sender, EventArgs e) => ShowHideCommandBar();

        private void RecentList_DoubleClick(object sender, DoubleTappedRoutedEventArgs e)
        {
            var clickedItem = recentList.SelectedItem as RecentItem;
            if (clickedItem != null)
            {
                if (clickedItem.IsWeb)
                {
                    App.ShellViewModel.AddWebPage(clickedItem.Location);
                }
                else
                {
                    App.ShellViewModel.AddFile(clickedItem.FileHandle);
                }
            }
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            ShowHideCommandBar();
            ViewModel.RemoveRecentItem(recentList.SelectedIndex);
        }

        private void recentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (recentList.SelectedItem != null)
            {
                ViewModel.ItemSelected = true;
                if (!(recentList.SelectedItem as RecentItem).IsWeb) ViewModel.CanOpenLocation = true;
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e) => ShowHideCommandBar();

        private void ShowHideCommandBar()
        {
            if (pivot.SelectedItem != null)
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

        protected override void OnLoad()
        {
            ViewModel.LoadRecentItems();
            ViewModel.RecentListChanged += ViewModel_RecentListChanged;
        }

        public override void OnSuspend()
        {
            ViewModel.RecentListChanged -= ViewModel_RecentListChanged;
            ViewModel.DisposeRecentItems();
        }

        public override void OnResume() => OnLoad();

        public override void Dispose() => OnSuspend();

        protected override void OnXamlLoad()
        {
            ShowHideCommandBar();
            Loaded -= BaseLayout_Loaded;
        }

        private async void OpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            if (recentList.SelectedItem != null)
            {
                var clickedItem = recentList.SelectedItem as RecentItem;
                await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(clickedItem.FileHandle?.Path));
            }
        }
    }
}
