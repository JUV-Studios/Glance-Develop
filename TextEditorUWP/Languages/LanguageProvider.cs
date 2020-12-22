using ColorCode.Common;
using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace TextEditor.Languages
{
    public static class LanguageProvider
    {
        public static readonly Color CommentColor = Color.FromArgb(255, 0, 128, 0);
        public static readonly Color StringColor = Color.FromArgb(255, 163, 21, 21);
        public static readonly Color BuiltinsColor = Color.FromArgb(255, 208, 144, 31);
        public static readonly Color KeywordsColor = Color.FromArgb(255, 52, 141, 255);

        public static Dictionary<string, Lazy<SyntaxLanguage>> CodeLanguages = new()
        {
            { ".py", new(new PythonSyntaxLanguage()) },
            { ".txt", new(new PlainTextLanguage()) },
        };

        public static readonly IDictionary<string, Color> HighlightColors = new Dictionary<string, Color>
        {
            { ScopeName.Comment, CommentColor },
            { ScopeName.String, StringColor },
            { ScopeName.Predefined, BuiltinsColor },
            { ScopeName.Keyword, KeywordsColor },
            { ScopeName.PlainText, new UISettings().GetColorValue(UIColorType.Foreground) }
        };
    }
}
