using System;
using ProtoBuf;

namespace Copren.Unity.Net.Domain.Messaging.Messages
{
    [ProtoContract]
    public class ClientReadyMessage : Message
    {
        [ProtoMember(1)]
        public Guid ClientId { get; set; }
    }
}