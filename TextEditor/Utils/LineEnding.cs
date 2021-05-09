using Windows.UI.Text;

namespace TextEditor.Utils
{
	public enum LineEnding
	{
		CR, LF, CRLF
	}

	internal static class LineEndingHelper
	{
		public static string GetNewLineString(LineEnding lineEnding)
		{
			return lineEnding switch
			{
				LineEnding.LF => "\n",
				LineEnding.CRLF => "\r\n",
				_ => "\r"
			};
		}

		public static void NormalizeLineEnding(ref string text, LineEnding lineEnding)
		{
			if (lineEnding == LineEnding.CR)
			{
				text = text.Replace("\r\n", "\r").Replace("\n\r", "\r");
				StringHelper.ReplaceCharacter(text, '\n', '\r');
			}
			else if (lineEnding == LineEnding.LF)
			{
				text = text.Replace("\r\n", "\n").Replace("\n\r", "\n");
				StringHelper.ReplaceCharacter(text, '\r', '\n');
			}
			else
			{
				NormalizeLineEnding(ref text, LineEnding.CR);
				text = text.Replace("\r", "\r\n");
			}
		}

		public static TextGetOptions GetRichRetriveOptions(LineEnding lineEnding)
		{
			return lineEnding switch
			{
				LineEnding.LF => TextGetOptions.UseLf,
				LineEnding.CRLF => TextGetOptions.UseCrlf,
				_ => TextGetOptions.None
			};
		}
	}
}
