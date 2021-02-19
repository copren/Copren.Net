using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Copren.Unity.Net.Domain.Messaging.Messages
{
    public class MessageSerializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            try
            {
                if (typeName.Contains("Uri")) throw new Exception($"{assemblyName}::{typeName}");
                return Type.GetType(typeName);
            }
            catch (Exception e)
            {
                if (typeName.Contains("Uri")) throw new Exception($"{assemblyName}::{typeName}, {e.Message}");
            }

            return null;
        }
    }
}