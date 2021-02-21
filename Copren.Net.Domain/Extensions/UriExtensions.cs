using System;
using System.Net;
using System.Net.Sockets;

namespace Copren.Net.Domain.Extensions
{
    public static class UriExtensions
    {
        public static EndPoint ToEndPoint(this Uri self)
        {
            if (IPAddress.TryParse(self.Host, out var ipAddress)) return new IPEndPoint(ipAddress, self.Port);
            return new DnsEndPoint(self.Host, self.Port);
        }

        public static ProtocolType ProtocolType(this Uri self)
        {
            return (ProtocolType)Enum.Parse(
                typeof(ProtocolType),
                self.Scheme[0].ToString().ToUpper() + self.Scheme.Substring(1));
        }
    }
}