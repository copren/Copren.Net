using System;
using ProtoBuf;

namespace Asteroid.Shared
{
    [ProtoContract]
    public class StateMessage
    {
        [ProtoMember(1)]
        public double PositionX { get; set; }
        [ProtoMember(2)]
        public double PositionY { get; set; }
        [ProtoMember(3)]
        public double PositionZ { get; set; }
        [ProtoMember(4)]
        public double RotationX { get; set; }
        [ProtoMember(5)]
        public double RotationY { get; set; }
        [ProtoMember(6)]
        public double RotationZ { get; set; }
    }
}
