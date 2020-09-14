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
    public sealed partial class BrowserPage : BaseLayout
    {
        public BrowserViewModel ViewModel { get; } = new BrowserViewModel();

        private Uri UriToNavigate;

        public BrowserPage()
        {
            InitializeComponent();
        }

        private Uri ConvertStringToUri(string val)
        {
            string text = AddressBox.Text.ToLower();
            if (!text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || !text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                text = "http://" + text;
            }

            return new Uri(text);
        }

        private void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                WebContent.Navigate(ConvertStringToUri((sender as TextBox).Text));
            }
        }

        private void WebContent_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            ViewModel.ProgressRingOn = true;
            ViewModel.ContentShown = true;
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
                ViewModel.ContentShown = true;
                AddressBox.Text = args.Uri.AbsoluteUri;
                BackButton.Visibility = sender.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
                ForwardButton.Visibility = sender.CanGoForward ? Visibility.Visible : Visibility.Collapsed;
                MainViewModel.RecentPagesContainer.Values[sender.DocumentTitle] = AddressBox.Text;
                RefreshButton.Visibility = Visibility.Visible;
                AddressBox.IsReadOnly = false;
                ShellInstance.Title = sender.DocumentTitle;
                ShellInstance.Caption = sender.Source.AbsoluteUri;
            }
        }

        public override void OnLoad()
        {
            WebContent.NavigationCompleted -= WebContent_NavigationCompleted;
            var uri = new Uri(ShellInstance.Parameter as string);
            if (uri != UriToNavigate)
            {
                UriToNavigate = uri;
                WebContent.Navigate(UriToNavigate);
            }
        }

        public override void OnSuspend()
        {
            WebContent.NavigationStarting -= WebContent_NavigationStarting;
            WebContent.NavigationCompleted -= WebContent_NavigationCompleted;
        }

        public override void OnResume()
        {
            WebContent.NavigationStarting += WebContent_NavigationStarting;
            WebContent.NavigationCompleted += WebContent_NavigationCompleted;
        }

        private void ReduceMemoryUsage()
        {
            WebContent.NavigationStarting -= WebContent_NavigationStarting;
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
        }

        public override void Dispose()
        {
            ReduceMemoryUsage();
            WebContent = null;
        }
    }
}
