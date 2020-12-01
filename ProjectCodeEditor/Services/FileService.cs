using AutoIt.Common;
using Microsoft.Toolkit.Uwp.Helpers;
using ProjectCodeEditor.Core.Helpers;
using ProjectCodeEditor.Models;
using ProjectCodeEditor.ViewModels;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ProjectCodeEditor.Services
{
    public static class FileService
    {
        public static async Task<TextPlusEncoding> ReadTextFileAsync(StorageFile file)
        {
            var bytes = await file.ReadBytesAsync();
            var encoding = GetTextEncoding(bytes);
            string text = encoding.GetString(bytes);
            if (Singleton<SettingsViewModel>.Instance.TrimWhitespace) text = text.TrimEnd();
            return new TextPlusEncoding(text, encoding);
        }

        public static Encoding GetTextEncoding(byte[] data)
        {
            var detect = Singleton<TextEncodingDetect>.Instance;
            var encoding = detect.DetectEncoding(data, data.Length);
            if (encoding == TextEncodingDetect.Encoding.Ansi) return Encoding.Default;
            else if (encoding == TextEncodingDetect.Encoding.Utf8Bom || encoding == TextEncodingDetect.Encoding.Utf8Nobom) return Encoding.UTF8;
            else if (encoding == TextEncodingDetect.Encoding.Utf16BeBom || encoding == TextEncodingDetect.Encoding.Utf16BeNoBom) return Encoding.BigEndianUnicode;
            else if (encoding == TextEncodingDetect.Encoding.Utf16LeBom || encoding == TextEncodingDetect.Encoding.Utf16LeNoBom) return Encoding.Unicode;
            else if (encoding == TextEncodingDetect.Encoding.Ascii) return Encoding.ASCII;
            else return Encoding.UTF32;
        }
    }
}
