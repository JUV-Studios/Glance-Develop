using TextEditor;

namespace TextEditorUWP.Languages
{
    public sealed class PlainTextLanguage : SyntaxLanguage
    {
        public PlainTextLanguage() : base("Plain text")
        {
            IsPlainText = true;
        }
    }
}
