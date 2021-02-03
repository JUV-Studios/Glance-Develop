using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ProjectCodeEditor.Core.Helpers
{
    public static class General
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items) collection.Add(item);
        }

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

        public static bool WriteObject(object obj, Stream stream)
        {
            bool result = true;
            try
            {
                Singleton<BinaryFormatter>.Instance.Serialize(stream, obj);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is SerializationException) result = false;
                else throw ex;
            }

            return result;
        }

        public static object ReadObject(Stream stream) => Singleton<BinaryFormatter>.Instance.Deserialize(stream);
    }
}

