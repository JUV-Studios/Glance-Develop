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
using System.Threading;
using System.Threading.Tasks;
using TextEditor.Lexer;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;

namespace TextEditor.UI
{
    internal readonly struct FileHistoryData
    {
        internal FileHistoryData(string text, int selectionIndex)
        {
            Text = text;
            SelectionIndex = selectionIndex;
        }

        public readonly string Text;
        public readonly int SelectionIndex;
    }

    public struct SelectionInfo
    {
        public uint SelectionStart;
        public uint SelectionEnd;
    }

    public delegate void HistoryItemDone(bool lastElem, string text, int index);

    public delegate bool AcceptTextChange(string text, RoutedEventArgs args);

    public sealed class SyntaxEditor : RichEditBox, INotifyPropertyChanged, IDisposable
    {
        private static string EditorStylesString;

        public SyntaxEditor()
        {
            // DefaultStyleKey = typeof(SyntaxEditor);
            TextDocument.DefaultTabStop = 4;
        }

        public IAsyncAction InitializeAsync(bool isRichText) => InitializeAsyncImpl(isRichText).AsAsyncAction();

        private async Task InitializeAsyncImpl(bool isRichText)
        {
            // Set default settings
            _IsRichText = isRichText;
            TextChanged += Editor_TextChanged;
            KeyDown += HandleTextViewKeyDown;
            SelectionChanged += Editor_SelectionChanged;
            if (!isRichText)
            {
                UndoStack = new();
                RedoStack = new();
                IsSpellCheckEnabled = false;
                TextWrapping = TextWrapping.NoWrap;
                ClipboardCopyFormat = RichEditClipboardFormat.PlainText;
                DisabledFormattingAccelerators = DisabledFormattingAccelerators.All;
                TextDocument.UndoLimit = 0;
            }

            if (string.IsNullOrEmpty(EditorStylesString)) EditorStylesString = await PathIO.ReadTextAsync("ms-appx:///TextEditor/Themes/Styles.xaml");
            Resources.MergedDictionaries.Add(XamlReader.Load(EditorStylesString) as ResourceDictionary);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private StyleDictionary TextStyles => ApplicationTheme.Light == ApplicationTheme.Dark ? StyleDictionary.DefaultDark : StyleDictionary.DefaultLight;

        #region Text View

        private SelectionInfo _TextSelection = new();

        public SelectionInfo TextSelection => _TextSelection;

        private bool _IsRichText = true;

        public bool IsRichText => _IsRichText;

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
                await FocusManager.TryFocusAsync(FocusManager.FindNextFocusableElement(FocusNavigationDirection.Down), FocusState.Keyboard);
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
                TextDocument.GetText(TextGetOptions.UseCrlf, out string text);
                return text;
            }
            set
            {
                if (Text != value) TextDocument.SetText(TextSetOptions.None, value);
            }
        }

        private readonly SemaphoreSlim WriteFileSemaphore = new(1, 1);

        public IAsyncOperation<bool> WriteFileAsync(StorageFile file, BinaryStringEncoding encoding) => WriteFile(file, encoding).AsAsyncOperation();

        private async Task<bool> WriteFile(StorageFile file, BinaryStringEncoding encoding)
        {
            await WriteFileSemaphore.WaitAsync();
            bool result;
            IRandomAccessStream stream = null;
            try
            {
                CachedFileManager.DeferUpdates(file);
                stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                if (_IsRichText) TextDocument.SaveToStream(TextGetOptions.UseCrlf, stream);
                else
                {
                    await stream.WriteAsync(CryptographicBuffer.ConvertStringToBinary(Text, encoding));
                    await stream.FlushAsync();
                }

                var writeResult = await CachedFileManager.CompleteUpdatesAsync(file);
                if (writeResult != FileUpdateStatus.Complete) result = false;
                else result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                stream?.Dispose();
            }

            WriteFileSemaphore.Release();
            return result;
        }

        private static readonly string[] SplitValue = new string[] { "\r\n" };

        public int LinesCount => Text.TrimEnd().Split(SplitValue, StringSplitOptions.None).Length;

        public AcceptTextChange TextChangeDelegate { get; set; }

        private void Editor_TextChanged(object sender, RoutedEventArgs e)
        {
            var text = Text;
            if ((TextChangeDelegate?.Invoke(text, e)).GetValueOrDefault(false) && !_IsRichText)
            {
                UndoStack.Push(new(text, TextDocument.Selection.EndPosition));
                UpdateHistoryProperties();
            }
        }

        public void DetachEvents()
        {
            SelectionChanged -= Editor_SelectionChanged;
            TextChanged -= Editor_TextChanged;
            KeyDown -= HandleTextViewKeyDown;
        }

        #endregion

        #region Highlighting

        bool HighlightLock = false;

        public bool SyntaxHighlighting(IReadOnlyList<HighlightToken> tokens)
        {
            if (tokens.Count <= 0 || HighlightLock) return false;
            HighlightLock = true;
            foreach (var token in tokens)
            {
                var range = TextDocument.GetRange(token.StartIndex, Convert.ToInt32(token.StartIndex + token.Length));
                if (range.CharacterFormat.ForegroundColor != token.Colour) range.CharacterFormat.ForegroundColor = token.Colour;
            }

            HighlightLock = false;
            return true;
        }

        #endregion

        #region Interaction

        public bool CanUndo => IsRichText ? Document.CanUndo() : UndoStack.Count > 0;

        public bool CanRedo => IsRichText ? Document.CanRedo() : RedoStack.Count > 0;

        public bool CanClearHistory => CanUndo || CanRedo;

        private Stack<FileHistoryData> UndoStack;

        private Stack<FileHistoryData> RedoStack;

        public HistoryItemDone HistoryDone { get; set; }

        public string PreviousText => !IsRichText ? UndoStack.Peek().Text : string.Empty;

        private void UpdateHistoryProperties()
        {
            PropertyChanged?.Invoke(this, new(nameof(CanUndo)));
            PropertyChanged?.Invoke(this, new(nameof(CanRedo)));
            PropertyChanged?.Invoke(this, new(nameof(CanClearHistory)));
        }

        public void Undo()
        {
            if (!CanUndo) return;
            if (IsRichText) Document.Undo();
            else
            {
                FileHistoryData retVal;
                // Remove current one first
                RedoStack.Push(UndoStack.Pop());
                // Return second one
                if (UndoStack.Count == 0) retVal = new(string.Empty, -1);
                else retVal = UndoStack.Pop();
                UpdateHistoryProperties();
                HistoryDone?.Invoke(retVal.SelectionIndex == -1, retVal.Text, retVal.SelectionIndex);
            }
        }

        public void Redo()
        {
            if (!CanRedo) return;
            if (IsRichText) Document.Redo();
            else
            {
                var retVal = RedoStack.Pop();
                UpdateHistoryProperties();
                HistoryDone?.Invoke(false, retVal.Text, retVal.SelectionIndex);
            }
        }

        public void ClearHistory()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            UpdateHistoryProperties();
        }

        public void SelectAll()
        {
            Focus(FocusState.Keyboard);
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
            WriteFileSemaphore.Dispose();
            UndoStack?.Clear();
            RedoStack?.Clear();
            UndoStack = null;
            RedoStack = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #endregion
    }
}