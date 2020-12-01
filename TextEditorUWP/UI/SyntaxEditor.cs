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

using System;
using System.ComponentModel;
using TextEditor.Lexer;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
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

        public UISettings UserInterfaceSettings { get; set; }
    
        public event EventHandler TextChanged;

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

        public MenuFlyout ContextMenu
        {
            get => TextView?.ContextFlyout as MenuFlyout;
            set => TextView.ContextFlyout = value;
        }

        public int TabSize
        {
            get
            {
                if (SyntaxLanguage.IndentationProvider != null) return SyntaxLanguage.IndentationProvider.TabWidth;
                else return Convert.ToInt32(TextView.TextDocument.DefaultTabStop) - 4;
            }
            set
            {
                if (SyntaxLanguage?.IndentationProvider != null) SyntaxLanguage.IndentationProvider.TabWidth = value;
                TextView.TextDocument.DefaultTabStop = value + 4;
            }
        }

        #region Interaction

        public void SelectAll()
        {
            TextView.TextDocument.Selection.StartPosition = 0;
            TextView.TextDocument.Selection.EndPosition = Text.Length - 1;
        }

        public void ClearSelection() => TextView.TextDocument.Selection.EndPosition = TextView.TextDocument.Selection.StartPosition;

        public void FindText(string text)
        {

        }

        #endregion

        private void OnTextViewChanged(RichEditBox oldValue, RichEditBox newValue)
        {
            if (oldValue != null)
            {
                oldValue.SelectionChanged -= Editor_SelectionChanged;
                oldValue.TextChanged -= HandleTextViewTextChanged;
                oldValue.KeyUp -= HandleTextViewKeyUp;
                oldValue.Paste -= RequestLineNumberRedraw;
                newValue.KeyDown -= HandleTextViewKeyDown;
            }

            if (newValue != null)
            {
                newValue.TextChanged += HandleTextViewTextChanged;
                newValue.KeyUp += HandleTextViewKeyUp;
                newValue.KeyDown += HandleTextViewKeyDown;

                // Set default settings
                newValue.TextDocument.UndoLimit = 0;
                if (oldValue != null)
                {
                    newValue.FontFamily = new FontFamily(FontFamily);
                    newValue.FontSize = FontSize;
                    newValue.ContextFlyout = ContextFlyout;
                    if (SyntaxLanguage?.IndentationProvider != null) SyntaxLanguage.IndentationProvider.TabWidth = TabSize;
                    TextView.TextDocument.DefaultTabStop = TabSize + 4;
                }

                // BindTextViewStyle();
                // RefreshLineNumbers(1);

                newValue.SelectionChanged += Editor_SelectionChanged;
                newValue.Paste += RequestLineNumberRedraw;
            }
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

        private void RequestLineNumberRedraw(object sender, object e)
        {
            // RefreshLineNumbers(Text.Count<char>(c => c == '\r'));
        }

        /*
        void BindTextViewStyle() => BindTextViewerScrollViewer();

        void BindTextViewerScrollViewer()
        {
            if (ShowLineNumbers)
            {
                if (VisualTreeHelper.GetChild(TextView, 0) is Grid g)
                {
                    int max = VisualTreeHelper.GetChildrenCount(g);
                    for (int i = 0; i < max; i++)
                    {
                        if (VisualTreeHelper.GetChild(g, i) is ScrollViewer ele && scrollViewer != null)
                        {
                            ele.ViewChanged += (s, e) =>
                            {
                                RefreshLineNumbers(Text.Count<char>(c => c == '\r'));
                                scrollViewer.ChangeView(ele.HorizontalOffset, ele.VerticalOffset, null, true);
                            };
                        }
                    }
                }
            }
        }
        
        #endregion
        
        #region Line Number

        public static readonly DependencyProperty LineNumberBlockProperty =
            DependencyProperty.Register("LineNumberBlock", typeof(TextBlock), typeof(SyntaxEditor), new PropertyMetadata(null));

        public TextBlock LineNumberBlock
        {
            get { return (TextBlock)GetValue(LineNumberBlockProperty); }
            set { SetValue(LineNumberBlockProperty, value); }
        }

        void RefreshLineNumbers(int stop)
        {
            var builder = new StringBuilder();
            for (int i = 1; i <= stop; i++)
                builder.AppendLine(i.ToString());

            LineNumberBlock.Text = builder.ToString();
        }

        */

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

            if (!SyntaxLanguage.IsPlainText)
            {
                if (newValue.HighlightColors == null)
                    throw new ArgumentException("Grammer HightlightColrs must not be null");

                tokenizer = new Tokenizer(newValue.Grammer);
            }
        }

        #endregion

        #region Highlighting

        Tokenizer tokenizer = null;

        int textLength = 0;
        public void HandleTextViewTextChanged(object sender, RoutedEventArgs e)
        {
            TextChanged?.Invoke(this, new EventArgs());
            if (tokenizer == null) return;

            var editor = (RichEditBox)sender;
            string text = Text;

            if (text.Length == textLength) return;
            textLength = text.Length;

            editor.Document.GetRange(0, int.MaxValue).CharacterFormat.ForegroundColor = UserInterfaceSettings.GetColorValue(UIColorType.Foreground);
            var t = tokenizer.Tokenize(text);
            var highlightColors = SyntaxLanguage.HighlightColors;

            while (t.MoveNext())
            {
                if (highlightColors.TryGetValue(t.Current.Type, out Color foregroundColor))
                {
                    editor.Document.GetRange(t.Current.StartIndex, t.Current.StartIndex + t.Current.Length).CharacterFormat.ForegroundColor = foregroundColor;
                }
            }
        }

        #endregion

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
                if (TextView != null)
                {
                    // RefreshLineNumbers(value.Count<char>(c => c == '\r'));
                    TextView.Document.SetText(TextSetOptions.None, value);
                }
            }
        }

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
            if (SyntaxLanguage.IndentationProvider == null ||
                TextView.Document.Selection.Length != 0)
                return 0;

            try
            {
                return SyntaxLanguage.IndentationProvider.GuessIndentLevel(text, TextView.Document.Selection.EndPosition);
            }
            catch (Exception)
            { }

            return 0;
        }

        #endregion

        // ScrollViewer scrollViewer;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            /* if (TextView != null)            
                BindTextViewStyle(); 

            scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

            Debug.Assert(scrollViewer != null); */
        }

        public void ScrollToLine(int line)
        {
            ITextRange rangeToSelect = null;
            int size = 0;
            while (Text.Length > size)
            {
                var range = TextView.TextDocument.GetRange(size, size + 1);
                size += range.Expand(TextRangeUnit.Line);
                var lineIndex = range.GetIndex(TextRangeUnit.Line);
                if (line == lineIndex)
                {
                    rangeToSelect = range;
                    break;
                }
                size += 1;
            }

            TextView.TextChanged -= HandleTextViewTextChanged;
            TextView.KeyUp -= HandleTextViewKeyUp;
            TextView.KeyDown -= HandleTextViewKeyDown;
            TextView.Focus(FocusState.Keyboard);
            TextView.TextDocument.Selection.SetRange(rangeToSelect.StartPosition, rangeToSelect.EndPosition - 1);
            TextView.TextChanged += HandleTextViewTextChanged;
            TextView.KeyUp += HandleTextViewKeyUp;
            TextView.KeyDown += HandleTextViewKeyDown;
        }

        public void Dispose()
        {
            TextView.SelectionChanged -= Editor_SelectionChanged;
            TextView.TextChanged -= HandleTextViewTextChanged;
            TextView.KeyUp -= HandleTextViewKeyUp;
            TextView.Paste -= RequestLineNumberRedraw;
            TextView.KeyDown -= HandleTextViewKeyDown;
            SyntaxLanguage = null;
        }
    }
}