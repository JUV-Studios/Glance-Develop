using ProjectCodeEditor.ViewModels;
using Windows.UI.Xaml.Controls;

namespace ProjectCodeEditor.Views
{
    public sealed partial class EditorPage : UserControl
    {
        public EditorViewModel ViewModel { get; } = new EditorViewModel();

        public EditorPage()
        {
            InitializeComponent();
        }


        /* private async void Load()
        {
            Debug.WriteLine("Loading file");
            ViewModel.WorkString = "WorkStringTaskLoad".GetLocalized();
            MainViewModel.RecentlyUsedList.AddOrReplace(ViewModel.WorkingFile.Path.Replace('\\', '-'), ViewModel.WorkingFile);
            var fileReadData = await ViewModel.WorkingFile.ReadCodeFile();
            ViewModel.Encoding = fileReadData.Encoding;
            Editor.TextDocument.SetText(TextSetOptions.None, fileReadData.Text.TrimEnd());
            Editor.TextDocument.ClearUndoRedoHistory();
            WriteLineNumbers();
            Editor.TextChanged += Editor_TextChacccnged;
            ViewModel.WorkString = "WorkStringReady".GetLocalized();
        }

        private void WriteLineNumbers()
        {
        }

        private void GoToLineDialog_Submitted(object sender, bool e)
        {
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

        private void GoToLine_Click(object sender, RoutedEventArgs e) => GoToLineDialog.Show(ViewModel.LineCount); */
    }
}
