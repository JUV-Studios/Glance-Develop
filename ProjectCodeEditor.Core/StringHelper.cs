using System.Collections.Generic;

namespace ProjectCodeEditor.Core
{
    public static class StringHelper
    {
        public static bool StartsWithMultiple(string text, IEnumerable<string> strings)
        {
            bool startsWith = false;
            foreach (var line in strings)
            {
                if (text.StartsWith(line))
                {
                    startsWith = true;
                    break;
                }
            }

            return startsWith;
        }
    }
}
