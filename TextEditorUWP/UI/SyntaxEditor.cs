// Copyright (c) Adnan Umer. All rights reserved. Follow me @aztnan
// Email: aztnan@outlook.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
using ColorCode.Styling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TextEditor.Lexer;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace TextEditor.UI
{
    public struct SelectionInfo
    {
        public uint SelectionStart;
        public uint SelectionEnd;
    }

    public delegate void HistoryItemDone(bool lastElem, string text, int index);

    public delegate bool AcceptTextChange(string text, RoutedEventArgs args);

    public sealed class SyntaxEditor : RichEditBox, INotifyPropertyChanged, IDisposable
    {
        public SyntaxEditor() { }

        public void Initialize(bool isRichText)
        {
            // Set default settings
            IsRichText = isRichText;
            AttachEvents();
            if (!isRichText)
            {
                IsSpellCheckEnabled = false;
                TextDocument.DefaultTabStop = 8;
                DisabledFormattingAccelerators = DisabledFormattingAccelerators.All;
                TextDocument.UndoLimit = 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private StyleDictionary TextStyles => ApplicationTheme.Light == ApplicationTheme.Dark ? StyleDictionary.DefaultDark : StyleDictionary.DefaultLight;

        #region Text View

        private SelectionInfo _TextSelection = new();

        public SelectionInfo TextSelection => _TextSelection;

        private bool IsRichText;

        private async void TextView_Pasting(object sender, TextControlPasteEventArgs e)
        {
            e.Handled = true;
            var clipboardContent = Clipboard.GetContent();
            if (clipboardContent.AvailableFormats.Contains("Text")) TextDocument.Selection.TypeText(await clipboardContent.GetTextAsync());
        }

        private async void HandleTextViewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Tab)
            {
                e.Handled = true;
                TextDocument.Selection.TypeText("\t");
            }
            else if (e.Key == VirtualKey.Escape)
            {
                e.Handled = true;
                await FocusManager.TryFocusAsync(FocusManager.FindFirstFocusableElement(null), FocusState.Keyboard);
            }
        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            _TextSelection.SelectionStart = Convert.ToUInt32(TextDocument.Selection.StartPosition);
            _TextSelection.SelectionEnd = Convert.ToUInt32(TextDocument.Selection.EndPosition);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectionValid)));
        }

        public bool IsSelectionValid => IsSelectionValidImpl(TextSelection.SelectionStart, TextSelection.SelectionEnd);

        private bool IsSelectionValidImpl(uint start, uint end) => start != end && !string.IsNullOrWhiteSpace(TextDocument.GetRange(Convert.ToInt32(start), Convert.ToInt32(end)).Text);

        public string Text
        {
            get
            {
                TextDocument.GetText(TextGetOptions.None, out string text);
                return text.Replace("\r", Environment.NewLine);
            }
            set
            {
                if (Text != value) TextDocument.SetText(TextSetOptions.None, value);
            }
        }

        private static readonly string[] SplitValue = new string[] { Environment.NewLine };

        public long LinesCount => Text.TrimEnd().TrimEnd('\r', '\n').Split(SplitValue, StringSplitOptions.None).Length;

        public bool AttachEvents()
        {
            TextChanged += Editor_TextChanged;
            KeyDown += HandleTextViewKeyDown;
            SelectionChanged += Editor_SelectionChanged;
            Paste += TextView_Pasting;
            return true;
        }

        public AcceptTextChange TextChangeDelegate { get; set; }

        private void Editor_TextChanged(object sender, RoutedEventArgs e)
        {
            var text = Text;
            if ((TextChangeDelegate?.Invoke(text, e)).GetValueOrDefault(false) && !IsRichText)
            {
                UndoStack.Push(new(text, TextDocument.Selection.EndPosition));
                UpdateHistoryProperties();
            }
        }

        public bool DetachEvents()
        {
            SelectionChanged -= Editor_SelectionChanged;
            TextChanged -= Editor_TextChanged;
            Paste -= TextView_Pasting;
            KeyDown -= HandleTextViewKeyDown;
            return true;
        }

        #endregion

        #region Highlighting

        readonly CancellationTokenSource HighlightCancellation = new();

        bool HighlightLock = false;

        public IAsyncOperation<bool> SyntaxHighlighting(IReadOnlyList<HighlightToken> tokens) => SyntaxHighlightingImpl(tokens, HighlightCancellation.Token).AsAsyncOperation();

        private Task<bool> SyntaxHighlightingImpl(IReadOnlyList<HighlightToken> tokens, CancellationToken cancelToken)
        {
            if (tokens.Count <= 0 || HighlightLock) return Task.FromResult(false);
            HighlightLock = true;
            while (true)
            {
                if (cancelToken.IsCancellationRequested) break;
                else
                {
                    HighlightToken token;
                    for (int i = 0; i < tokens.Count; i++)
                    {
                        token = tokens[i];
                        var range = TextDocument.GetRange(token.StartIndex, Convert.ToInt32(token.StartIndex + token.Length));
                        if (range.CharacterFormat.ForegroundColor != token.Colour) range.CharacterFormat.ForegroundColor = token.Colour;
                    }
                }
            }

            HighlightLock = false;
            return Task.FromResult(true);
        }

        #endregion

        #region Interaction

        public bool CanUndo => UndoStack.Count > 0;

        public bool CanRedo => RedoStack.Count > 0;

        public bool CanClearHistory => CanUndo || CanRedo;

        private readonly Stack<Tuple<string, int>> UndoStack = new();

        private readonly Stack<Tuple<string, int>> RedoStack = new();

        public HistoryItemDone HistoryDone { get; set; }

        public string PreviousText => UndoStack.Peek().Item1;

        private void UpdateHistoryProperties()
        {
            PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
            PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
            PropertyChanged?.Invoke(this, new(nameof(CanClearHistory)));
        }

        public void Undo()
        {
            if (!CanUndo) return;
            Tuple<string, int> retVal;
            // Remove current one first
            RedoStack.Push(UndoStack.Pop());
            // Return second one
            if (UndoStack.Count == 0) retVal = new("", -1);
            else retVal = UndoStack.Pop();
            UpdateHistoryProperties();
            HistoryDone?.Invoke(retVal.Item2 == -1, retVal.Item1, retVal.Item2);
        }

        public void Redo()
        {
            if (!CanRedo) return;
            Tuple<string, int> retVal = RedoStack.Pop();
            UpdateHistoryProperties();
            HistoryDone?.Invoke(false, retVal.Item1, retVal.Item2);
        }

        public void ClearHistory()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            UpdateHistoryProperties();
        }

        public void SelectAll()
        {
            TextDocument.Selection.StartPosition = 0;
            TextDocument.Selection.EndOf(TextRangeUnit.Story, true);
        }

        public void ClearSelection() => TextDocument.Selection.EndPosition = TextDocument.Selection.StartPosition;

        public void FindText(string text)
        {

        }

        public void ScrollToLine(int line, bool extend)
        {
            Focus(FocusState.Keyboard);
            TextDocument.Selection.HomeKey(TextRangeUnit.Story, false);
            TextDocument.Selection.MoveStart(TextRangeUnit.Line, line - 1);
            if (extend)
            {
                TextDocument.Selection.Expand(TextRangeUnit.Line);
                TextDocument.Selection.EndPosition = TextDocument.Selection.EndPosition - 1;
            }
        }

        public void Dispose()
        {
            DetachEvents();
            HighlightCancellation.Cancel();
            HighlightCancellation.Dispose();
        }

        #endregion
    }
}