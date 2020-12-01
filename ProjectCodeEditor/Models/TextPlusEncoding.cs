using System.Text;

namespace ProjectCodeEditor.Models
{
    public readonly struct TextPlusEncoding
    {
        public TextPlusEncoding(string text, Encoding encoding)
        {
            Text = text;
            Encoding = encoding;
        }

        public string Text { get; }
        public Encoding Encoding { get; }
    }
}
