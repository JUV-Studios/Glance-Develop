using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using System;
using System.Diagnostics;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ProjectCodeEditor.Views
{
    public sealed partial class BrowserPage : Page
    {
        public BrowserViewModel ViewModel { get; } = new BrowserViewModel();
        private Uri UriToNavigate;

        public BrowserPage()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            WebContent.GoBack();
        }

        private void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                string text = AddressBox.Text.ToLower();
                if (!text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || !text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    text = "http://" + text;
                }

                WebContent.Navigate(new Uri(text));
            }
        }

        private void WebContent_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            ViewModel.ProgressRingOn = true;
            WebContent.Visibility = Visibility.Collapsed;
            ViewModel.WorkString = "WorkStringTaskLoad".GetLocalized();
            AddressBox.Text = args.Uri.AbsoluteUri;
            AddressBox.IsReadOnly = true;
        }

        private void WebContent_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.IsSuccess)
            {
                ViewModel.WorkString = sender.DocumentTitle;
                ViewModel.ProgressRingOn = true;
                WebContent.Visibility = Visibility.Visible;
                AddressBox.Text = args.Uri.AbsoluteUri;
                BackButton.Visibility = sender.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
                MainViewModel.RecentPagesContainer.Values[sender.DocumentTitle] = AddressBox.Text;
                RefreshButton.Visibility = Visibility.Visible;
                AddressBox.IsReadOnly = false;
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            WebContent.Refresh();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (EditorShellViewModel.Instance.SelectedItem.Parameter is string)
            {
                WebContent.NavigationCompleted -= WebContent_NavigationCompleted;
                var uri = new Uri(EditorShellViewModel.Instance.SelectedItem.Parameter as string);
                if (uri != UriToNavigate)
                {
                    UriToNavigate = uri;
                    WebContent.Navigate(UriToNavigate);
                }
            }

            EditorShellViewModel.FrameClosed += EditorShellViewModel_FrameClosed;
        }

        private void EditorShellViewModel_FrameClosed(object sender, ShellView e)
        {
            WebContent.NavigationCompleted -= WebContent_NavigationCompleted;
            Debug.WriteLine("Cleaning up memory");
            int count = 0;
            var timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1) };
            timer.Start();
            timer.Tick += (s, p) =>
            {
                WebContent.Source = new Uri("about:blank");
                count++;
                if (count == 40)
                {
                    timer.Stop();
                }
            };

            EditorShellViewModel.FrameClosed -= EditorShellViewModel_FrameClosed;
        }
    }
}
