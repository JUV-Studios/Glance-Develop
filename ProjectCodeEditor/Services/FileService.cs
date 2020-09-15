using AutoIt.Common;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ProjectCodeEditor.Services
{
    public sealed class TextPlusEncoding
    {
        public string Text { get; set; }
        public Encoding Encoding { get; set; }
    }

    public static class FileService
    {
        public static async Task<TextPlusEncoding> ReadCodeFile(this StorageFile file)
        {
            var bytes = await file.ReadBytesAsync();
            var encoding = GetTextEncoding(bytes);
            return new TextPlusEncoding()
            {
                Encoding = encoding,
                Text = encoding.GetString(bytes)
            };
        }

        public static Encoding GetTextEncoding(byte[] data)
        {
            var detect = new TextEncodingDetect();
            var encoding = detect.DetectEncoding(data, data.Length);
            if (encoding == TextEncodingDetect.Encoding.Ansi)
            {
                return Encoding.Default;
            }
            else if (encoding == TextEncodingDetect.Encoding.Utf8Bom || encoding == TextEncodingDetect.Encoding.Utf8Nobom)
            {
                return Encoding.UTF8;
            }
            else if (encoding == TextEncodingDetect.Encoding.Utf16BeBom || encoding == TextEncodingDetect.Encoding.Utf16BeNoBom)
            {
                return Encoding.BigEndianUnicode;
            }
            else if (encoding == TextEncodingDetect.Encoding.Utf16LeBom || encoding == TextEncodingDetect.Encoding.Utf16LeNoBom)
            {
                return Encoding.Unicode;
            }
            else if (encoding == TextEncodingDetect.Encoding.Ascii)
            {
                return Encoding.ASCII;
            }
            else
            {
                throw new Exception("Couldn't get the encoding of the file. Might not be plain text.");
            }
        }
    }
}
