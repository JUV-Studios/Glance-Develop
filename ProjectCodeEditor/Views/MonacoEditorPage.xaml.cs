using System;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Views
{
    public sealed partial class MonacoEditorPage : UserControl
    {
        public MonacoEditorViewModel ViewModel { get; } = new MonacoEditorViewModel();

        public MonacoEditorPage()
        {
            InitializeComponent();
            EditorShellViewModel.FrameCreated += ShellViewModel_FrameCreated;
        }

        private void Load()
        {
            
        }

        private void ShellViewModel_FrameCreated(object sender, ShellView e)
        {
            
        }
    }
}
