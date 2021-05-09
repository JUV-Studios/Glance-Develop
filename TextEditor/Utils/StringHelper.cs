namespace TextEditor.Utils
{
	internal static unsafe class StringHelper
	{
		public static void ReplaceCharacter(string text, char replaced, char replacedWith)
		{
			fixed (char* buffer = text)
			{
				for (int i = 0; i < text.Length; i++)
				{
					if (buffer[i] == replaced)
					{
						buffer[i] = replacedWith;
					}
				}
			}
		}
	}
}
