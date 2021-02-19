using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Copren.Unity.Net.Domain.Messaging.Transport;

namespace Copren.Unity.Net.Core.Messaging.Transport
{
    public interface IClientTransport
    {
        event TransportHandler OnConnected;
        event TransportMessageHandler OnReceived;
        event TransportHandler OnClosed;

        ProtocolType ProtocolType { get; }

        Task StartAsync(CancellationToken ct = default);
        Task StopAsync();
        Task SendServerMessageAsync(byte[] data);
        Task SendMessageAsync(EndPoint endPoint, byte[] data);
    }
}