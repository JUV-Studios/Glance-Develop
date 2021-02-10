using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;

namespace DevelopManaged
{
    public static class General
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

        public static string TrimEndings(this string text) => text.TrimEnd().TrimEnd('\r').TrimEnd('\n');

        public static string StorablePathName(string path) => path.Replace("\\", "-").Replace(":", "=");

        public static string FolderPath(string path) => Path.GetDirectoryName(path);
    }

    /// <summary>
    /// Represents an object that can be requested for close asynchronously
    /// </summary>
    public interface IAsyncClosable
    {
        IAsyncOperation<bool> CloseAsync();
    }

    /// <summary>
    /// Represents an object that can be suspended and resumes
    /// </summary>
    public interface ISuspendable : IAsyncClosable
    {
        IAsyncAction SuspendAsync();

        IAsyncAction ResumeAsync();
    }
}
