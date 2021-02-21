using System;
using System.Runtime.Serialization;

namespace Copren.Net.Core.Connection
{
    [Serializable]
    internal class InvalidStateException : Exception
    {
        public ClientState ClientState;

        public InvalidStateException(ClientState clientState)
        {
            ClientState = clientState;
        }

        public InvalidStateException(ClientState clientState, string message) : base(message)
        {
            ClientState = clientState;
        }

        public InvalidStateException(ClientState clientState, string message, Exception innerException) : base(message, innerException)
        {
            ClientState = clientState;
        }

        protected InvalidStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}