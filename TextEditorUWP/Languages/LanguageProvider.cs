using System;
using System.Collections.Generic;
using TextEditor;

namespace TextEditorUWP.Languages
{
    public static class LanguageProvider
    {
        public static Dictionary<string, Lazy<SyntaxLanguage>> CodeLanguages = new()
        {
            { ".py", new(new TextEditor.Languages.PythonSyntaxLanguage()) },
            { ".txt", new(new PlainTextLanguage()) },
        };
    }
}
