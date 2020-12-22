using TextEditor.IntelliSense;

namespace TextEditor.Languages
{
    public sealed class PlainTextLanguage : SyntaxLanguage
    {
        public PlainTextLanguage() : base("Plain text")
        {
            IsPlainText = true;
        }

        public override IFileIntelliSense CreateIntelliSenseEngine() => null;
    }
}
