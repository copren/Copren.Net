using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Copren.Unity.Net.Domain.Messaging.Transport
{
    public delegate Task TransportHandler(ProtocolType protocol, EndPoint endpoint, SocketAsyncEventArgs eventArgs);
    public delegate Task TransportMessageHandler(ProtocolType protocol, EndPoint endpoint, TransportMessage message);
}