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
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Input.Preview.Injection;
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

    public sealed class SyntaxEditor : INotifyPropertyChanged, IDisposable
    {
        public SyntaxEditor(RichEditBox editor, bool isRichText)
        {
            TextView = editor;
            IsRichText = isRichText;
            // Set default settings
            AttachEvents(TextView);
            if (!isRichText)
            {
                TextView.DisabledFormattingAccelerators = DisabledFormattingAccelerators.All;
                TextView.TextDocument.UndoLimit = 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private StyleDictionary TextStyles => ApplicationTheme.Light == ApplicationTheme.Dark ? StyleDictionary.DefaultDark : StyleDictionary.DefaultLight;

        #region Text View

        private const byte TabSizeIncrement = 8;

        public byte TabSize
        {
            get => (byte)(Convert.ToByte(TextView.TextDocument.DefaultTabStop) - TabSizeIncrement);
            set => TextView.TextDocument.DefaultTabStop = value + TabSizeIncrement;
        }

        private SelectionInfo _TextSelection = new();

        public SelectionInfo TextSelection => _TextSelection;

        private readonly RichEditBox TextView;

        private readonly bool IsRichText;

        private async void TextView_Pasting(object sender, TextControlPasteEventArgs e)
        {
            e.Handled = true;
            var clipboardContent = Clipboard.GetContent();
            if (clipboardContent.AvailableFormats.Contains("Text")) TextView.TextDocument.Selection.TypeText(await clipboardContent.GetTextAsync());
        }

        private async void HandleTextViewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Tab)
            {
                e.Handled = true;
                TextView.TextDocument.Selection.TypeText("\t");
            }
            else if (e.Key == VirtualKey.Escape)
            {
                e.Handled = true;
                await FocusManager.TryFocusAsync(FocusManager.FindFirstFocusableElement(null), FocusState.Keyboard);
            }
        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            _TextSelection.SelectionStart = Convert.ToUInt32(TextView.TextDocument.Selection.StartPosition);
            _TextSelection.SelectionEnd = Convert.ToUInt32(TextView.TextDocument.Selection.EndPosition);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectionValid)));
        }

        public bool IsSelectionValid => IsSelectionValidImpl(TextSelection.SelectionStart, TextSelection.SelectionEnd);

        private bool IsSelectionValidImpl(uint start, uint end) => start != end && !string.IsNullOrWhiteSpace(TextView.TextDocument.GetRange(Convert.ToInt32(start), Convert.ToInt32(end)).Text);

        public string Text
        {
            get
            {
                TextView.Document.GetText(TextGetOptions.None, out string text);
                return text;
            }
            set
            {
                if (Text != value) TextView.TextDocument.SetText(TextSetOptions.None, value);
            }
        }

        public bool AttachEvents(RichEditBox editBox)
        {
            if (editBox != null)
            {
                editBox.TextChanged += Editor_TextChanged;
                editBox.KeyUp += HandleTextViewKeyUp;
                editBox.KeyDown += HandleTextViewKeyDown;
                editBox.SelectionChanged += Editor_SelectionChanged;
                editBox.Paste += TextView_Pasting;
                return true;
            }

            return false;
        }

        private void Editor_TextChanged(object sender, RoutedEventArgs e)
        {
        }

        public bool DetachEvents(RichEditBox editBox)
        {
            if (editBox != null)
            {
                editBox.SelectionChanged -= Editor_SelectionChanged;
                editBox.TextChanged -= Editor_TextChanged;
                editBox.KeyUp -= HandleTextViewKeyUp;
                editBox.Paste -= TextView_Pasting;
                editBox.KeyDown -= HandleTextViewKeyDown;
                return true;
            }

            return false;
        }

        #endregion

        #region Syntax Language

        /* private static readonly DependencyProperty SyntaxLanguageProperty =
            DependencyProperty.Register("SyntaxLanguage", typeof(SyntaxLanguage), typeof(SyntaxEditor),
                                        new PropertyMetadata(null, OnSyntaxLanguagePropertyChanged));

        private static void OnSyntaxLanguagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SyntaxEditor)d).OnSyntaxLanguageChanged((SyntaxLanguage)e.NewValue);
        }

        public SyntaxLanguage SyntaxLanguage
        {
            get { return (SyntaxLanguage)GetValue(SyntaxLanguageProperty); }
            set { SetValue(SyntaxLanguageProperty, value); }
        }

        private void OnSyntaxLanguageChanged(SyntaxLanguage newValue)
        {
            if (newValue != null) TextView.IsSpellCheckEnabled = newValue.IsPlainText;
        }
        */
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
                        var range = TextView.Document.GetRange(token.StartIndex, Convert.ToInt32(token.StartIndex + token.Length));
                        if (range.CharacterFormat.ForegroundColor != token.Colour) range.CharacterFormat.ForegroundColor = token.Colour;
                    }
                }
            }

            HighlightLock = false;
            return Task.FromResult(true);
        }

        #endregion

        #region Indentation

        private void HandleTextViewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                // RefreshLineNumbers(text.Count<char>(c => c == '\r'));

                /* var indentLevel = GetIndentLevel(ref text);
                e.Handled = true;
                if (indentLevel == 0) return;

                TextView.Document.Selection.SetText(TextSetOptions.None, new String(' ', indentLevel));
                var x = TextView.Document.Selection.StartPosition + indentLevel;
                TextView.Document.Selection.SetRange(x, x);*/
            }
            else e.Handled = false;
            // else if (TextView.Document.Selection.Length > 0 ||
            // e.Key == Windows.System.VirtualKey.Back)
            // RefreshLineNumbers(Text.Count<char>(c => c == '\r'));
        }

        /* int GetIndentLevel(ref string text)
        {
            if (!SyntaxLanguage.IsPlainText)
            {
                if (SyntaxLanguage.IndentationProvider == null || TextView.Document.Selection.Length != 0) return 0;
                try
                {
                    return SyntaxLanguage.IndentationProvider.GuessIndentLevel(text, TextView.Document.Selection.EndPosition);
                }
                catch (Exception)
                {

                }
            }

            return 0;
        } */

        #endregion

        #region Interaction

        static readonly InputInjector injector = InputInjector.TryCreate();

        public void SelectAll()
        {
            TextView.Focus(FocusState.Keyboard);
            TextView.TextDocument.Selection.StartPosition = 0;
            TextView.TextDocument.Selection.MoveEnd(TextRangeUnit.Story, 1);
        }

        public void ClearSelection() => TextView.TextDocument.Selection.EndPosition = TextView.TextDocument.Selection.StartPosition;

        public void FindText(string text)
        {
        }

        public void ScrollToLine(int line, bool extend)
        {
            TextView.Focus(FocusState.Keyboard);
            TextView.TextDocument.Selection.HomeKey(TextRangeUnit.Story, false);
            TextView.TextDocument.Selection.MoveStart(TextRangeUnit.Line, line - 1);
            if (extend)
            {
                TextView.TextDocument.Selection.Expand(TextRangeUnit.Line);
                TextView.TextDocument.Selection.EndPosition = TextView.TextDocument.Selection.EndPosition - 1;
            }
        }

        public void Dispose()
        {
            if (DetachEvents(TextView))
            {
                HighlightCancellation.Cancel();
                // SyntaxLanguage = null;
            }
        }

        #endregion
    }
}