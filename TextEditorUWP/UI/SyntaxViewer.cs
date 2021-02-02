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

/*
using System;
using System.Text;
using TextEditor.Lexer;
using TextEditor.Languages;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;

namespace TextEditor.UI
{
    public class SyntaxViewer : Control
    {
        readonly Storyboard highlightLineStoryboard;

        public SyntaxViewer()
        {
            DefaultStyleKey = typeof(SyntaxViewer);

            TextView = new RichTextBlock
            {
                IsTextSelectionEnabled = false
            };

            TextView.LayoutUpdated += OnTextViewLayoutUpdated;

            LineNumberBlock = new TextBlock { Foreground = new SolidColorBrush(Color.FromArgb(255, 43, 145, 175)) };

            highlightLineStoryboard = new Storyboard();

            var timeline = new DoubleAnimationUsingKeyFrames
            {
                EnableDependentAnimation = true,
                FillBehavior = FillBehavior.HoldEnd
            };

            Storyboard.SetTargetProperty(highlightLineStoryboard, "HighlightTop");
            Storyboard.SetTarget(highlightLineStoryboard, this);

            timeline.KeyFrames.Add(new DiscreteDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)) });
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(700)), EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut } });

            highlightLineStoryboard.Children.Add(timeline);
        }


        public Visibility HighlightSelectedLine
        {
            get { return (Visibility)GetValue(HighlightSelectedLineProperty); }
            set { SetValue(HighlightSelectedLineProperty, value); }
        }

        public static readonly DependencyProperty HighlightSelectedLineProperty =
            DependencyProperty.Register("HighlightSelectedLine", typeof(Visibility), typeof(SyntaxViewer), new PropertyMetadata(Visibility.Collapsed));



        #region HighlightTop

        public static DependencyProperty HighlightTopProperty =
            DependencyProperty.Register("HighlightTop", typeof(double), typeof(SyntaxViewer), new PropertyMetadata(0.0));

        public double HighlightTop
        {
            get { return (double)GetValue(HighlightTopProperty); }
            set { SetValue(HighlightTopProperty, value); }
        }

        #endregion

        #region Line Height

        public static DependencyProperty LineHeightProperty =
            DependencyProperty.Register("LineHeight", typeof(double), typeof(SyntaxViewer), new PropertyMetadata(0.0));

        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        #endregion

        #region TextView

        public static DependencyProperty TextViewProperty =
            DependencyProperty.Register("TextView", typeof(RichTextBlock), typeof(SyntaxViewer),
                                        new PropertyMetadata(null));

        public RichTextBlock TextView
        {
            get { return (RichTextBlock)GetValue(TextViewProperty); }
            set { SetValue(TextViewProperty, value); }
        }

        #endregion

        #region Line No Margin

        public static DependencyProperty LineNumberBlockProperty =
            DependencyProperty.Register("LineNumberBlock", typeof(TextBlock), typeof(SyntaxViewer),
                                        new PropertyMetadata(null));

        public TextBlock LineNumberBlock
        {
            get { return (TextBlock)GetValue(LineNumberBlockProperty); }
            set { SetValue(LineNumberBlockProperty, value); }
        }

        #endregion

        #region Syntax Language

        public static readonly DependencyProperty SyntaxLanguageProperty =
            DependencyProperty.Register("SyntaxLanguage", typeof(SyntaxLanguage), typeof(SyntaxViewer),
                                        new PropertyMetadata(null, OnSyntaxLanguagePropertyChanged));

        private static void OnSyntaxLanguagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SyntaxViewer)d).OnSyntaxLanguageChanged((SyntaxLanguage)e.NewValue);
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
        }

        #endregion

        #region Text

        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SyntaxViewer), new PropertyMetadata("", OnTextProperyChanged));

        private static void OnTextProperyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SyntaxViewer)d).OnTextChanged((string)e.NewValue);
        }

        public string Text
        {
            get { return GetValue(TextProperty).ToString(); }
            set { SetValue(TextProperty, value); }
        }

        int lines;
        protected void OnTextChanged(string value)
        {
            if (TextView == null || value == null || tokenizer == null) return;

            lines = value.Split('\n').Length;
            StringBuilder builder = new StringBuilder();
            for (int i = 1; i <= lines; i++)
            {
                builder.Append(i);
                builder.Append("\n");
            }

            LineNumberBlock.Text = builder.ToString();
            builder.Clear();

            TextView.Blocks.Clear();
            var p = new Paragraph();
            TextView.Blocks.Add(p);

            if (tokenizer == null) return;

            var t = tokenizer.Tokenize(value);

            while (t.MoveNext())
            {
                if (LanguageProvider.HighlightColors.TryGetValue(t.Current.Type, out Color color))
                {
                    if (builder.Length > 0)
                    {
                        p.Inlines.Add(new Run { Text = builder.ToString() });
                        builder.Clear();
                    }

                    p.Inlines.Add(new Run
                    {
                        Text = value.Substring(t.Current.StartIndex, t.Current.Length),
                        Foreground = new SolidColorBrush(color)
                    });
                }
                else
                    builder.Append(value.Substring(t.Current.StartIndex, t.Current.Length));
            }

            if (builder.Length > 0)
            {
                p.Inlines.Add(new Run { Text = builder.ToString() });
                builder.Clear();
            }
        }

        private void OnTextViewLayoutUpdated(object sender, object e)
        {
            LineHeight = (lines > 0) ? TextView.ActualHeight / lines : 0;

            if (pendingMove)
            {
                pendingMove = false;
                OnSelectedLineChanged(0, SelectedLine);
            }
        }

        #endregion

        ScrollViewer scrollViewer;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
        }

        #region SelectedLine

        public static DependencyProperty SelectedLineProperty =
            DependencyProperty.Register("SelectedLine", typeof(int), typeof(SyntaxViewer), new PropertyMetadata(0, OnSelectedLinePropertyChanged));

        private static void OnSelectedLinePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SyntaxViewer)d).OnSelectedLineChanged((int)e.OldValue, (int)e.NewValue);
        }

        bool pendingMove = false;
        protected void OnSelectedLineChanged(int oldValue, int newValue)
        {
            if (highlightLineStoryboard == null) return;

            if (LineHeight == 0)
            {
                pendingMove = true;
                return;
            }

            double offset = LineHeight * (newValue - 1);
            double vOffset = scrollViewer != null ? scrollViewer.VerticalOffset : 0.0;

            ((DoubleAnimationUsingKeyFrames)highlightLineStoryboard.Children[0]).KeyFrames[0].Value = HighlightTop;
            ((DoubleAnimationUsingKeyFrames)highlightLineStoryboard.Children[0]).KeyFrames[1].Value = offset;

            highlightLineStoryboard.Stop();
            highlightLineStoryboard.Begin();

            if (scrollViewer == null) return;

            if (!(offset > vOffset) ||
                !(offset < vOffset + scrollViewer.ActualHeight - LineHeight * 2))

                scrollViewer.ChangeView(null, offset - this.ActualHeight / 2, null);
        }

        public int SelectedLine
        {
            get { return (int)GetValue(SelectedLineProperty); }
            set { SetValue(SelectedLineProperty, value); }
        }

        #endregion
    }
}

*/