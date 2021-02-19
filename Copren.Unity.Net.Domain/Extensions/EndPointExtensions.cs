using System;
using System.Net;
using System.Net.Sockets;

namespace Copren.Unity.Net.Domain.Extensions
{
    public static class EndPointExtensions
    {
        public static Uri ToUri(this EndPoint self, ProtocolType protocol)
        {
            if (self is IPEndPoint ipEndPoint) return ipEndPoint.ToUri(protocol);
            if (self is DnsEndPoint dnsEndPoint) return dnsEndPoint.ToUri(protocol);
            throw new NotImplementedException("Unknown EndPoint type");
        }
        public static Uri ToUri(this IPEndPoint self, ProtocolType protocol)
        {
            return new Uri($"{protocol}://{self.Address}:{self.Port}");
        }

        public static Uri ToUri(this DnsEndPoint self, ProtocolType protocol)
        {
            return new Uri($"{protocol}://{self.Host}:{self.Port}");
        }
    }
}