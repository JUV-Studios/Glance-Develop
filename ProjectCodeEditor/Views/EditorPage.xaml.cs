using Microsoft.Toolkit.Uwp.Extensions;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace ProjectCodeEditor.Views
{
    public sealed partial class EditorPage : Page
    {
        public EditorViewModel ViewModel { get; } = new EditorViewModel();
        private int allLines = 0;
        private bool controlPressed = false;
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public EditorPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.WorkingFile = e.Parameter as StorageFile;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async void Load()
        {
            ViewModel.WorkString = "WorkStringTaskLoad".GetLocalized();
            MainViewModel.RecentlyUsedList.AddOrReplace(ViewModel.WorkingFile.Path.Replace('\\', '-'), ViewModel.WorkingFile);
            var fileReadData = await ViewModel.WorkingFile.ReadCodeFile();
            ViewModel.Encoding = fileReadData.Encoding;
            Editor.TextDocument.SetText(TextSetOptions.None, fileReadData.Text.TrimEnd());
            Editor.TextDocument.ClearUndoRedoHistory();
            WriteLineNumbers();
            Editor.TextChanged += Editor_TextChanged;
            ViewModel.WorkString = "WorkStringReady".GetLocalized();
        }

        private void WriteLineNumbers()
        {
            if (LineNumbers.Visibility == Visibility.Visible)
            {
                string text;
                Editor.TextDocument.GetText(TextGetOptions.None, out text);
                var lines = text.TrimEnd().Split('\r');
                if (allLines == lines.Count())
                {
                    // Don't do anything
                }
                else
                {
                    LineNumbers.Items.Clear();
                    for (int i = 0; i < lines.Count(); i++)
                    {
                        LineNumbers.Items.Add((i + 1).ToString());
                    }

                    allLines = lines.Count();
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            EditorShellViewModel.FrameClosed += EditorShellViewModel_FrameClosed;
            EditorShellViewModel.FrameNavigationCompleted += EditorShellViewModel_FrameNavigationCompleted;
            Load();
        }

        private void EditorShellViewModel_FrameNavigationCompleted(object sender, EventArgs e)
        {
        }

        private void EditorShellViewModel_FrameClosed(object sender, Models.ShellView e)
        {
            SaveFile();
            EditorShellViewModel.FrameClosed -= EditorShellViewModel_FrameClosed;
            EditorShellViewModel.FrameNavigationCompleted -= EditorShellViewModel_FrameNavigationCompleted;
        }

        private void Editor_TextChanged(object sender, RoutedEventArgs e)
        {
            WriteLineNumbers();
        }

        private void Editor_KeyDown(object sender, KeyRoutedEventArgs e)
        {
        }

        private async void SaveFile()
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                ViewModel.WorkString = "WorkStringTaskSave".GetLocalized();
                string text;
                Editor.TextDocument.GetText(TextGetOptions.None, out text);
                await FileIO.WriteBytesAsync(ViewModel.WorkingFile, ViewModel.Encoding.GetBytes(text));
            }
            catch (IOException)
            {

            }
            finally
            {
                _semaphoreSlim.Release();
                ViewModel.WorkString = "WorkStringReady".GetLocalized();
            }
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
