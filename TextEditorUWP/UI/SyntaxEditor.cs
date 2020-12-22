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

// Edited by Jaiganesh Kumaran for the Universal Windows Platform with additional features for Develop

using ColorCode.Common;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TextEditor.Languages;
using TextEditor.Lexer;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace TextEditor.UI
{
    public sealed class SyntaxEditor : Control, INotifyPropertyChanged, IDisposable
    {
        // TODO: Implement line numbers properly
        public bool ShowLineNumbers { get; set; } = false;

        public SyntaxEditor()
        {
            DefaultStyleKey = typeof(SyntaxEditor);

            /// LineNumberBlock = new TextBlock { Foreground = new SolidColorBrush(ColorProvider.GetColorValue(UIColorType.AccentLight3)) /* Color.FromArgb(255, 43, 145, 175)) */ };
            TextView = new RichEditBox();

            // this.Loaded += (s, e) => { BindTextViewerScrollViewer(); };
        }
    
        public event PropertyChangedEventHandler PropertyChanged;

        #region Text View

        public static readonly DependencyProperty TextViewProperty =
            DependencyProperty.Register("TextView", typeof(RichEditBox), typeof(SyntaxEditor),
                                        new PropertyMetadata(null, OnTextViewPropertyChanged));

        private static void OnTextViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SyntaxEditor)d).OnTextViewChanged((RichEditBox)e.OldValue, (RichEditBox)e.NewValue);
        }

        public Tuple<int, int> TextSelection = new Tuple<int, int>(0, 0);

        public RichEditBox TextView
        {
            get { return (RichEditBox)GetValue(TextViewProperty); }
            set { SetValue(TextViewProperty, value); }
        }

        public new double FontSize
        {
            get => TextView.FontSize;
            set => TextView.FontSize = value;
        }

        public new string FontFamily
        {
            get => TextView.FontFamily.Source;
            set => TextView.FontFamily = new FontFamily(value);
        }

        public int TabSize
        {
            get
            {
                if (SyntaxLanguage?.IndentationProvider != null) return SyntaxLanguage.IndentationProvider.TabWidth;
                else return Convert.ToInt32(TextView.TextDocument.DefaultTabStop) - 8;
            }
            set
            {
                if (SyntaxLanguage?.IndentationProvider != null) SyntaxLanguage.IndentationProvider.TabWidth = value;
                TextView.TextDocument.DefaultTabStop = value + 8;
            }
        }

        private void OnTextViewChanged(RichEditBox oldValue, RichEditBox newValue)
        {
            var isOldNull = DetachEvents(oldValue);
            if (AttachEvents(newValue))
            {
                // Set default settings
                newValue.TextDocument.UndoLimit = 0;
                if (!isOldNull)
                {
                    newValue.FontFamily = new FontFamily(FontFamily);
                    newValue.FontSize = FontSize;
                    if (SyntaxLanguage?.IndentationProvider != null) SyntaxLanguage.IndentationProvider.TabWidth = TabSize;
                    TextView.TextDocument.DefaultTabStop = TabSize + 4;
                }
            }
        }

        private async void TextView_Pasting(object sender, TextControlPasteEventArgs e)
        {
            e.Handled = true;
            var clipboardContent = Clipboard.GetContent();
            if (clipboardContent.AvailableFormats.Contains("Text")) TextView.TextDocument.Selection.TypeText(await clipboardContent.GetTextAsync());
        }

        private void HandleTextViewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Tab)
            {
                // Tab support
                e.Handled = true;
                int size;
                var indentationProvider = SyntaxLanguage?.IndentationProvider;
                if (indentationProvider != null) size = indentationProvider.TabWidth;
                else size = TabSize;
                TextView.TextDocument.Selection.TypeText(new string(' ', size));
            }
        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextSelection = new Tuple<int, int>(TextView.TextDocument.Selection.StartPosition, TextView.TextDocument.Selection.EndPosition);
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsSelectionValid)));
        }

        public bool IsSelectionValid => TextSelection.Item1 != TextSelection.Item2 && !string.IsNullOrWhiteSpace(TextView.TextDocument.GetRange(TextSelection.Item1, TextSelection.Item2).Text);

        public string Text
        {
            get
            {
                string text = string.Empty;

                if (TextView != null)
                    TextView.Document.GetText(TextGetOptions.None, out text);

                return text;
            }
            set
            {
                if (TextView != null && Text != value)
                {
                    TextView.TextDocument.SetText(TextSetOptions.None, value);
                }
            }
        }

        public bool AttachEvents(RichEditBox editBox)
        {
            if (editBox != null)
            {
                editBox.TextChanged += HandleTextViewTextChanged;
                editBox.KeyUp += HandleTextViewKeyUp;
                editBox.KeyDown += HandleTextViewKeyDown;
                editBox.SelectionChanged += Editor_SelectionChanged;
                editBox.Paste += TextView_Pasting;
                return true;
            }

            return false;
        }

        public bool DetachEvents(RichEditBox editBox)
        {
            if (editBox != null)
            {
                editBox.SelectionChanged -= Editor_SelectionChanged;
                editBox.TextChanged -= HandleTextViewTextChanged;
                editBox.KeyUp -= HandleTextViewKeyUp;
                editBox.Paste -= TextView_Pasting;
                editBox.KeyDown -= HandleTextViewKeyDown;
                return true;
            }

            return false;
        }

        #endregion

        #region Syntax Language

        public static readonly DependencyProperty SyntaxLanguageProperty =
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
            if (newValue == null)
            {
                tokenizer = null;
                return;
            }

            TextView.IsSpellCheckEnabled = SyntaxLanguage.IsPlainText;
            if (!SyntaxLanguage.IsPlainText) tokenizer = new Tokenizer(newValue.Grammer);
        }

        #endregion

        #region Highlighting

        Tokenizer tokenizer = null;

        int textLength = 0;

        public async void HandleTextViewTextChanged(object sender, RoutedEventArgs e)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Text)));
            if (tokenizer != null)
            {
                // Syntax highliting
                var editor = sender as RichEditBox;
                if (Text.Length == textLength) return;
                textLength = Text.Length;
                // Store a local copy to use from another thread
                var text = Text;
                await Task.Run(() =>
                {
                    if (tokenizer != null)
                    {
                        var t = tokenizer.Tokenize(text);
                        while (t.MoveNext())
                        {
                            if (t.Current != null)
                            {
                                if (LanguageProvider.HighlightColors.TryGetValue(t.Current.Type, out Color foregroundColor)) ColourRegion(t.Current, foregroundColor);
                                else ColourRegion(t.Current, LanguageProvider.HighlightColors[ScopeName.PlainText]);
                            }
                        }
                    }
                });
            }
        }

        private void ColourRegion(Token token, Color color)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
            {
                var range = TextView.Document.GetRange(token.StartIndex, token.StartIndex + token.Length);
                if (range.CharacterFormat.ForegroundColor != color) range.CharacterFormat.ForegroundColor = color;
            })).AsTask().ConfigureAwait(false);
        }

        #endregion

        #region Indentation

        private void HandleTextViewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                string text = Text;
                // RefreshLineNumbers(text.Count<char>(c => c == '\r'));

                var indentLevel = GetIndentLevel(ref text);
                e.Handled = true;
                if (indentLevel == 0) return;

                TextView.Document.Selection.SetText(TextSetOptions.None, new String(' ', indentLevel));
                var x = TextView.Document.Selection.StartPosition + indentLevel;
                TextView.Document.Selection.SetRange(x, x);
            }
            else e.Handled = false;
            // else if (TextView.Document.Selection.Length > 0 ||
            // e.Key == Windows.System.VirtualKey.Back)
            // RefreshLineNumbers(Text.Count<char>(c => c == '\r'));
        }

        int GetIndentLevel(ref string text)
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
        }

        #endregion

        #region Interaction

        public void SelectAll()
        {
            TextView.TextDocument.Selection.Expand(TextRangeUnit.Story);
            TextView.TextDocument.Selection.MoveEnd(TextRangeUnit.Character, -1);
        }

        public void ClearSelection() => TextView.TextDocument.Selection.EndPosition = TextView.TextDocument.Selection.StartPosition;

        public void FindText(string text)
        {
            
        }

        public void ScrollToLine(int line, bool extend)
        {
            TextView.Focus(FocusState.Keyboard);
            TextView.TextDocument.Selection.HomeKey(TextRangeUnit.Story, false);
            if (line > 1) TextView.TextDocument.Selection.MoveStart(TextRangeUnit.Line, line - 1);
            if (extend)
            {
                TextView.TextDocument.Selection.Expand(TextRangeUnit.Line);
                TextView.TextDocument.Selection.EndPosition = TextView.TextDocument.Selection.EndPosition - 1;
            }
        }

        public void Dispose()
        {
            if (DetachEvents(TextView)) SyntaxLanguage = null;
        }

        #endregion
    }
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
