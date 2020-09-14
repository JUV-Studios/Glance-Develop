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
    public sealed partial class BrowserPage : UserControl
    {
        public BrowserViewModel ViewModel { get; } = new BrowserViewModel();
        private Uri UriToNavigate;

        public BrowserPage()
        {
            InitializeComponent();
            App.ShellViewModel.FrameCreated += EditorShellViewModel_FrameCreated;
            App.ShellViewModel.FrameClosed += EditorShellViewModel_FrameClosed;
        }

        private void EditorShellViewModel_FrameCreated(object sender, ShellView e)
        {
            if (e.Parameter is string)
            {
                WebContent.NavigationCompleted -= WebContent_NavigationCompleted;
                var uri = new Uri(e.Parameter as string);
                if (uri != UriToNavigate)
                {
                    UriToNavigate = uri;
                    WebContent.Navigate(UriToNavigate);
                }
            }
            App.ShellViewModel.FrameCreated -= EditorShellViewModel_FrameCreated;
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
                ForwardButton.Visibility = sender.CanGoForward ? Visibility.Visible : Visibility.Collapsed;
                MainViewModel.RecentPagesContainer.Values[sender.DocumentTitle] = AddressBox.Text;
                RefreshButton.Visibility = Visibility.Visible;
                AddressBox.IsReadOnly = false;
            }
        }

        private void EditorShellViewModel_FrameClosed(object sender, ShellView e)
        {
            WebContent.NavigationCompleted -= WebContent_NavigationCompleted;
            Debug.WriteLine("Cleaning up memory");
            int count = 0;
            var timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            timer.Start();
            timer.Tick += (s, p) =>
            {
                WebContent.Source = new Uri("about:blank");
                count++;
                if (count == 20)
                {
                    timer.Stop();
                }
            };

            App.ShellViewModel.FrameClosed -= EditorShellViewModel_FrameClosed;
        }
    }
}
