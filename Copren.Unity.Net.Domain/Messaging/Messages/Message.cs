using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ProtoBuf;

namespace Copren.Unity.Net.Domain.Messaging.Messages
{
    public class Message
    {
        public static byte[] Serialize<T>(T message)
           where T : Message
        {
            using var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, message);
            var serializedPayload = memoryStream.GetBuffer();
            var typeData = Encoding.UTF8.GetBytes(typeof(T).AssemblyQualifiedName);
            var buffer = new byte[8 + 4 + typeData.Length + memoryStream.Length];
            buffer[0] = (byte)((long)memoryStream.Length >> 54);
            buffer[1] = (byte)((long)memoryStream.Length >> 48);
            buffer[2] = (byte)((long)memoryStream.Length >> 40);
            buffer[3] = (byte)((long)memoryStream.Length >> 32);
            buffer[4] = (byte)((long)memoryStream.Length >> 24);
            buffer[5] = (byte)((long)memoryStream.Length >> 16);
            buffer[6] = (byte)((long)memoryStream.Length >> 8);
            buffer[7] = (byte)((long)memoryStream.Length >> 0);

            buffer[8] = (byte)((int)typeData.Length >> 24);
            buffer[9] = (byte)((int)typeData.Length >> 16);
            buffer[10] = (byte)((int)typeData.Length >> 8);
            buffer[11] = (byte)((int)typeData.Length >> 0);

            Array.Copy(typeData, 0, buffer, 12, typeData.Length);
            Array.Copy(serializedPayload, 0, buffer, 12 + typeData.Length, memoryStream.Length);
            return buffer;
        }

        public static T Deserialize<T>(byte[] data, int offset, int length, Type type)
            where T : Message
        {
            using var memoryStream = new MemoryStream(data, offset, length);
            return (T)Serializer.Deserialize(type, memoryStream);
        }
    }
}