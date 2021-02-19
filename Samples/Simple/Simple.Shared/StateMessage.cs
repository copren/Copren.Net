using System;
using ProtoBuf;

namespace Simple.Shared
{
    [ProtoContract]
    public class StateMessage
    {
        [ProtoMember(1)]
        public double X { get; set; }
        [ProtoMember(2)]
        public double Y { get; set; }
    }
}
