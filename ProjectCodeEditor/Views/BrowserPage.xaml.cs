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
    public sealed partial class BrowserPage : UserControl, ILayoutView
    {
        public BrowserViewModel ViewModel { get; } = new BrowserViewModel();

        private ShellView InstanceShellView;

        private Uri UriToNavigate;

        private WebView WebContent;

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
                try
                {
                    WebContent.Navigate(ConvertStringToUri((sender as TextBox).Text));
                }
                catch (UriFormatException)
                {
                    WebContent.Refresh();                    
                }
            }
        }

        private void WebContent_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            ViewModel.IsLoading = true;
            ViewModel.ProgressRingOn = true;
            ViewModel.ContentShown = false;
            ViewModel.WorkString = "WorkStringTaskLoad".GetLocalized();
            AddressBox.Text = args.Uri.AbsoluteUri;
            AddressBox.IsReadOnly = true;
        }

        private void WebContent_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.IsSuccess)
            {
                ViewModel.IsLoading = false;
                ViewModel.WorkString = sender.DocumentTitle;
                ViewModel.ProgressRingOn = false;
                ViewModel.ContentShown = true;
                AddressBox.Text = args.Uri.AbsoluteUri;
                BackButton.Visibility = sender.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
                ForwardButton.Visibility = sender.CanGoForward ? Visibility.Visible : Visibility.Collapsed;
                MainViewModel.RecentPagesContainer.Values[sender.DocumentTitle] = AddressBox.Text;
                RefreshButton.Visibility = Visibility.Visible;
                AddressBox.IsReadOnly = false;
                Debug.WriteLine("Navigation completed");
                if (CommandBar.Visibility == Visibility.Collapsed) CommandBar.Visibility = Visibility.Visible;
            }
        }

        private void ReduceMemoryUsage()
        {
            WebContent.NavigationStarting -= WebContent_NavigationStarting;
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
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            WebContentBorder.Child = WebContent;
        }

        public UIElement GetUserInterface() => this;

        public void Initialize(ShellView e)
        {
            InstanceShellView = e;
            WebContent = new WebView(WebViewExecutionMode.SeparateThread);
            WebContent.NavigationStarting += WebContent_NavigationStarting;
            if (InstanceShellView.Parameter != null)
            {
                var uri = new Uri(InstanceShellView.Parameter as string);
                if (uri != UriToNavigate)
                {
                    UriToNavigate = uri;
                    WebContent.Navigate(UriToNavigate);
                }
            }
            WebContent.NavigationCompleted += WebContent_NavigationCompleted;
        }

        public void OnTabAdded()
        {
            ViewModel.ContentShown = true;
        }

        public void OnTabRemoveRequested() => App.ShellViewModel.TerminateSelected();

        public void SaveState()
        {
            if (!ViewModel.IsLoading) ViewModel.ContentShown = false;
        }

        public void RestoreState()
        {
            if (!ViewModel.IsLoading) ViewModel.ContentShown = true;
        }

        public void Dispose() => ReduceMemoryUsage();
    }
}
