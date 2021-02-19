using System;
using System.Net.Sockets;
using ProtoBuf;

namespace Copren.Unity.Net.Domain.Messaging.Messages
{
    [ProtoContract]
    public class HeloMessage : Message
    {
        [ProtoMember(1)]
        public Guid? ClientId { get; set; }
    }
}