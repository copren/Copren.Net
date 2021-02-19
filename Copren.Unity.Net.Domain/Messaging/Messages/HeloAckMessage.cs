using System;
using Copren.Unity.Net.Domain.Attributes;
using ProtoBuf;

namespace Copren.Unity.Net.Domain.Messaging.Messages
{
    [PriorityMessage]
    [ProtoContract]
    public class HeloAckMessage : Message
    {
        [ProtoMember(1)]
        public Guid ClientId { get; }

        public HeloAckMessage() { }

        public HeloAckMessage(Client client)
        {
            this.ClientId = client.ClientId;
        }
    }
}