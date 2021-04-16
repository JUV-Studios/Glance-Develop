using Windows.UI;

namespace TextEditor.Lexer
{
    public enum TokenType
	{
        None, Keyword, Identifier, Operator, String, Type, Delimiter, Attribute, Other
    }

    public struct HighlightToken
    {
        public int StartIndex;
        public uint Length;
        public TokenType TokenType;
        public Color Colour;
    }
}
