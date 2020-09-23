using System;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Views
{
    public sealed partial class TextModeBrowserPage : UserControl, ILayoutView
    {
        private ShellView InstanceShellView;
        public TextModeBrowserPage()
        {
            InitializeComponent();
        }

        public UIElement GetUserInterface() => this;

        public void Initialize(ShellView e)
        {
            InstanceShellView = e;
        }

        public void OnTabAdded()
        {
        }

        public void OnTabRemoveRequested() => App.ShellViewModel.TerminateSelected();

        public void SaveState()
        {
        }

        public void RestoreState()
        {
        }

        public void Dispose()
        {
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewer.Html = "<h1>Hello World</h1>";
        }
    }
}
