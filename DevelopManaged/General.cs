using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace DevelopManaged
{
    public static class General
    {
        private static readonly ResourceLoader ResourceLocator = ResourceLoader.GetForViewIndependentUse();

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

        public static string TrimEndings(this string text) => text.TrimEnd().TrimEnd('\r').TrimEnd('\n');

        public static string StorablePathName(string path) => path.Replace("\\", "-").Replace(":", "=");

        public static string GetLocalized(string key) => ResourceLocator.GetString(key);
    }
}