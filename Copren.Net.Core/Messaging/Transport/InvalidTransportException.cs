using System;
using System.Runtime.Serialization;

namespace Copren.Net.Core.Messaging.Transport
{
    [Serializable]
    internal class InvalidTransportException : Exception
    {
        public InvalidTransportException()
        {
        }

        public InvalidTransportException(string message) : base(message)
        {
        }

        public InvalidTransportException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidTransportException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}