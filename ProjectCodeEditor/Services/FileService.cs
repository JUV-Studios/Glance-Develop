using AutoIt.Common;
using Microsoft.Toolkit.Uwp.Helpers;
using ProjectCodeEditor.Core.Helpers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace ProjectCodeEditor.Services
{
    public static class FileService
    {
        public static async Task<bool> DeleteFileAsync(StorageFile file)
        {
            bool result = true;
            try
            {
                await file.DeleteAsync();
            }
            catch (ArgumentException)
            {
                result = false;
            }

            return result;
        }

        public static async Task<(string, Encoding)> ReadTextFileAsync(StorageFile file)
        {
            var bytes = await file.ReadBytesAsync();
            var encoding = GetTextEncoding(bytes);
            return (encoding?.GetString(bytes), encoding);
        }

        public static Encoding GetTextEncoding(byte[] data)
        {
            var encoding = Singleton<TextEncodingDetect>.Instance.DetectEncoding(data, data.Length);
            if (encoding == TextEncodingDetect.Encoding.Ansi) return Encoding.Default;
            else if (encoding == TextEncodingDetect.Encoding.Utf8Bom || encoding == TextEncodingDetect.Encoding.Utf8Nobom) return Encoding.UTF8;
            else if (encoding == TextEncodingDetect.Encoding.Utf16BeBom || encoding == TextEncodingDetect.Encoding.Utf16BeNoBom) return Encoding.BigEndianUnicode;
            else if (encoding == TextEncodingDetect.Encoding.Utf16LeBom || encoding == TextEncodingDetect.Encoding.Utf16LeNoBom) return Encoding.Unicode;
            else if (encoding == TextEncodingDetect.Encoding.Ascii) return Encoding.ASCII;
            else return Encoding.UTF32;
        }

        public static string GetFolderPath(IStorageItem item) => item.Path.Remove(item.Path.IndexOf(item.Name) - 1);

        public static async Task OpenFileLocationAsync(StorageFile file) => await Launcher.LaunchFolderPathAsync(Path.GetDirectoryName(file.Path));
    }
}
