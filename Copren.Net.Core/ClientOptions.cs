using System.Net;

namespace Copren.Net.Core
{
    public class ClientOptions
    {
        public EndPoint LocalEndPoint { get; set; }
        public EndPoint RemoteEndPoint { get; set; }
    }
}