using Windows.UI;

namespace TextEditor.Lexer
{
    public struct HighlightToken
    {
        public int StartIndex;
        public uint Length;
        public Color Colour;
    }
}
