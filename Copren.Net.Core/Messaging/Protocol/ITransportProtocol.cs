using System;
using System.Threading;
using System.Threading.Tasks;
using Copren.Net.Core.Messaging.Transport;

namespace Copren.Net.Core.Messaging.Protocol
{
    public delegate Task ConnectionHandler(Guid clientId);

    public interface ITransportProtocol
    {
        event ConnectionHandler OnConnected;
        event ConnectionHandler OnDisconnected;
        Task Connect(TransportManager transportManager, CancellationToken ct);
        Task Disconnect(TransportManager transportManager, CancellationToken ct);
    }
}