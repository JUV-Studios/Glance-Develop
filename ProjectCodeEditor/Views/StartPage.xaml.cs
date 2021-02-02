using ProjectCodeEditor.Views.Start_pages;
using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectCodeEditor.Views
{
    public sealed partial class StartPage : UserControl, INotifyPropertyChanged
    {
        public Type FramePageType;

        public StartPage() => InitializeComponent();

        public event PropertyChangedEventHandler PropertyChanged;

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                FramePageType = typeof(SettingsPage);
            }

            PropertyChanged?.Invoke(this, new(nameof(FramePageType)));
        }
    }
}
