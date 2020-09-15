using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.ViewModels;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Views
{
    public sealed partial class MonacoEditorPage : CodeEditorBase
    {
        private string GetLanguageStringForFileType()
        {
            switch (ViewModel.WorkingFile.FileType)
            {
                case ".c" or ".h": return "c";
                case ".cpp" or ".hpp" or ".cxx" or ".ixx": return "cpp";
                case ".cs": return "csharp";
                case ".css": return ".css";
                case ".go": return ".go";
                case ".html" or ".html": return "html";
                case ".java": return "java";
                case ".js" or "jsx": return "javascript";
                case ".json": return "json";
                case ".lua": return "lua";
                case ".md": return "markdown";
                case ".php": return "php";
                case ".py": return "python";
                case ".ts" or ".tsx": return "typescript";
                case ".vb": return "vb";
                case ".xml": return "xml";
                case ".yaml" or ".yml": return "yaml";
                default: return string.Empty;
            }
        }

        public MonacoEditorViewModel MonacoViewModel { get; } = new MonacoEditorViewModel();

        public MonacoEditorPage()
        {
            InitializeComponent();
            Loaded += BaseLayout_Loaded;
            PageXamlLoaded += MonacoEditor_Loaded;
        }

        private async void MonacoEditor_Loaded(object sender, EventArgs e)
        {
            Loaded -= BaseLayout_Loaded;
            PageXamlLoaded -= MonacoEditor_Loaded;
            await LoadFile();
            Editor.Text = ViewModel.FileData.Text;
            Editor.CodeLanguage = GetLanguageStringForFileType();
            ViewModel.WorkString = "WorkStringReady".GetLocalized();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("saving");
            ViewModel.FileData.Text = Editor.Text;
            SaveFile();
        }

        private void Share_Click(object sender, RoutedEventArgs e) => ShareFile();

        private void Editor_OpenLinkRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            App.ShellViewModel.AddWebPage(args.Uri.AbsoluteUri);
        }

        private void Save(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            Save_Click(null, null);
        }
    }
}
