using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace FingerLib
{
    [Serializable]
    class ClientNames
    {
        public List<string> Names { get; private set; }

        public ClientNames()
        {
            Names = new List<string>();
        }

        public string GetUserName(string name)
        {
            foreach (var clientName in Names)
            {
                if (clientName == name)
                {
                    return clientName;
                }
            }
            return null;
        }

        public static byte[] Serialize(ClientNames clientNames)
        {
            byte[] serializedClientNames;

            using (var memoryStream = new MemoryStream())
            {
                var xmlSerializer = new XmlSerializer(typeof(ClientNames));
                xmlSerializer.Serialize(memoryStream, clientNames);

                memoryStream.Position = 0;
                serializedClientNames = new byte[memoryStream.Length];

                const int offset = 0;
                memoryStream.Read(serializedClientNames, offset,
                    serializedClientNames.Length);
            }

            return serializedClientNames;
        }

        public static ClientNames Deserialize(byte[] byteArray)
        {
            using var memoryStream = new MemoryStream();
            const int Offset = 0;

            memoryStream.Write(byteArray, Offset, byteArray.Length);
            memoryStream.Position = 0;

            var xmlSerializer = new XmlSerializer(typeof(ClientNames));
            return (ClientNames)xmlSerializer.Deserialize(memoryStream);
        }
    }
}
