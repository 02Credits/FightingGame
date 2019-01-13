using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Networking
{
    public static class SerializationUtils
    {
        public static void SerializeToFile<T>(string fileName, T obj)
        {
            Console.WriteLine("Saving to " + fileName);
            var path = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(path) && !string.IsNullOrWhiteSpace(path))
                Directory.CreateDirectory(path);
            using (var stream = File.OpenWrite(fileName))
            using (var streamWriter = new StreamWriter(stream))
            {
                streamWriter.Write(JsonConvert.SerializeObject(obj));
                streamWriter.Flush();
                Console.WriteLine("    Finished");
            }
        }

        public static T DeserializeFileOrValue<T>(string fileName, T otherwise)
        {
            Console.WriteLine("Attempting deserialization of " + fileName);
            if (File.Exists(fileName))
            {
                using (var stream = File.OpenRead(fileName))
                using (var streamReader = new StreamReader(stream))
                {
                    var obj = JsonConvert.DeserializeObject<T>(streamReader.ReadToEnd());
                    Console.WriteLine("Success!");
                    return obj;
                }
            }
            else
            {
                Console.WriteLine("File doesnt exist. Returning default.");
                return otherwise;
            }
        }

        public static byte[] Serialize<T>(T itemToSerialize)
        {
            byte[] returnArray;
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream))
            {
                streamWriter.Write(JsonConvert.SerializeObject(itemToSerialize));
                streamWriter.Flush();
                returnArray = stream.ToArray();
            }
            return returnArray;
        }

        public static T Deserialize<T>(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var streamReader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<T>(streamReader.ReadToEnd());
            }
        }
    }
}
