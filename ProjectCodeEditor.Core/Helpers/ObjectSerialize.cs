using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ProjectCodeEditor.Core.Helpers
{
    public sealed class ObjectSerialize
    {
        public static bool Write(object obj, Stream stream)
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

        public static object Read(Stream stream) => Singleton<BinaryFormatter>.Instance.Deserialize(stream);
    }
}
