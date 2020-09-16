using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Toolkit.Uwp.Extensions;
using Microsoft.UI.Xaml.Controls;
using ProjectCodeEditor.Services;
using ProjectCodeEditor.ViewModels;
using Windows.Globalization.NumberFormatting;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WinRTXamlToolkit.Controls;

namespace ProjectCodeEditor.Views
{
    public sealed partial class LowLatencyEditorPage : CodeEditorBase
    {
        public LowLatencyEditorViewModel RichEditViewModel { get; } = new LowLatencyEditorViewModel();

        private Tuple<int, int> PreviousSelectedIndex = new Tuple<int, int>(0, 0);

        private bool DialogShown = false;

        private bool NeedRefocus = false;

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
            Editor.SelectionChanged += Editor_SelectionChanged;
            Editor.LostFocus += Editor_LostFocus;
            Editor.GotFocus += Editor_GotFocus;
            ViewModel.WorkString = "WorkStringReady".GetLocalized();
        }

        private void Editor_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NeedRefocus) Editor.TextDocument.Selection.SetRange(PreviousSelectedIndex.Item1, PreviousSelectedIndex.Item2);
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

        private async void GoTo_Click(object sender, RoutedEventArgs e)
        {
            DialogShown = true;

            var documentText = GetEditorString();

            var lineBox = new NumberBox()
            {
                Minimum = 1,
                Maximum = CalculateLineNumbers(documentText) + 1,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                AcceptsExpression = true,
                Header = "GoToLineBoxHeader".GetLocalized(),
                PlaceholderText = "GoToLineBoxPlaceholder".GetLocalized(),
            };            

            var goToDialog = new ContentDialog()
            {
                Title = "GoToDialogTitle".GetLocalized(),
                PrimaryButtonText = "GoToDialogPrimaryButtonText".GetLocalized(),
                SecondaryButtonText = "CancelText".GetLocalized(),
                DefaultButton = ContentDialogButton.Primary,
                Content = lineBox
            };

            var result = await goToDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ITextRange rangeToSelect = null;
                int line = Convert.ToInt32(lineBox.Value);
                int size = 0;
                while (documentText.Length > size)
                {
                    var range = Editor.TextDocument.GetRange(size, size + 1);

                    size += range.Expand(TextRangeUnit.Line);

                    var lineIndex = range.GetIndex(TextRangeUnit.Line);

                    if (line == lineIndex)
                    {
                        rangeToSelect = range;
                        break;
                    }

                    size += 1;
                }

                Editor.Focus(FocusState.Programmatic);

                Editor.TextDocument.Selection.SetRange(0, 0);

                if (rangeToSelect != null)
                {
                    Editor.TextDocument.Selection.SetRange(rangeToSelect.StartPosition, rangeToSelect.EndPosition);
                }
            }

            DialogShown = false;
        }


        private int CalculateLineNumbers(string documentText)
        {
            var lines = documentText.Split("\r");
            return lines.Count();
        }

        private void Editor_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!DialogShown && PreviousSelectedIndex.Item1 != 0 && PreviousSelectedIndex.Item2 != 0)
            {
                NeedRefocus = true;
            }
        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            PreviousSelectedIndex = new Tuple<int, int>(Editor.TextDocument.Selection.StartPosition, Editor.TextDocument.Selection.EndPosition);
        }

        private void Editor_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Tab && Editor.FocusState != FocusState.Programmatic && Editor.FocusState != FocusState.Unfocused)
            {
                e.Handled = true;
                Editor.TextDocument.Selection.TypeText("\t");
            }
        }
    }
}
