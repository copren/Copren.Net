using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Copren.Unity.Net.Domain.Messaging.Transport;

namespace Copren.Unity.Net.Hosting.Messaging.Transport
{


    public interface IHostTransport
    {
        event TransportHandler OnConnected;
        event TransportMessageHandler OnReceived;
        event TransportHandler OnClosed;

        ProtocolType ProtocolType { get; }

        Task StartAsync(CancellationToken ct = default);
        Task StopAsync();
        Task SendMessage(EndPoint endPoint, byte[] data);
    }
}