using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UtfUnknown;
using Windows.Storage;
using Windows.System;

namespace ProjectCodeEditor.Services
{
    public static class FileService
    {
        public static async Task<(string, object)> ReadTextFileAsync(StorageFile file)
        {
            var bytes = await file.ReadBytesAsync();
            var encoding = CharsetDetector.DetectFromBytes(bytes);
            if (encoding.Detected == null) return (Encoding.UTF8.GetString(bytes).TrimEnd().TrimEnd('\r').TrimEnd('\n'), Encoding.UTF8);
            string text = encoding.Detected.Encoding.GetString(bytes).TrimEnd().TrimEnd('\r').TrimEnd('\n');
            return (text, encoding);
        }

        public static async Task OpenFileLocation(StorageFile file) => await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(file.Path));
    }
}
