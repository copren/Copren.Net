using System.Net;

namespace Copren.Unity.Net.Core
{
    public class ClientOptions
    {
        public EndPoint LocalEndPoint { get; set; }
        public EndPoint RemoteEndPoint { get; set; }
    }
}