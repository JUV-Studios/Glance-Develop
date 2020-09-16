using System;
using System.Diagnostics;
using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.ViewModels;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Views
{
    public sealed partial class LowLatencyEditorPage : CodeEditorBase
    {
        public LowLatencyEditorViewModel RichEditViewModel { get; } = new LowLatencyEditorViewModel();

        public LowLatencyEditorPage()
        {
            InitializeComponent();
            Loaded += BaseLayout_Loaded;
            PageXamlLoaded += LowLatencyEditorPage_Loaded;
        }

        private async void LowLatencyEditorPage_Loaded(object sender, EventArgs e)
        {
            Loaded -= BaseLayout_Loaded;
            PageXamlLoaded -= LowLatencyEditorPage_Loaded;
            await LoadFile();
            Editor.TextDocument.SetText(TextSetOptions.None, ViewModel.FileData.Text.TrimEnd());
            Editor.TextDocument.ClearUndoRedoHistory();
            Editor.TextChanged += Editor_TextChanged;
            ViewModel.WorkString = "WorkStringReady".GetLocalized();
        }

        private void Editor_TextChanged(object sender, RoutedEventArgs e)
        {
            if (App.AppSettings.AutoSave) Save_Click(null, null);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FileData.Text = GetEditorString();
            SaveFile();
        }

        private string GetEditorString()
        {
            string str;
            Editor.TextDocument.GetText(TextGetOptions.None, out str);
            return str;
        }
    }
}
