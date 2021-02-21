using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Copren.Net.Core.Messaging.Transport;
using Copren.Net.Domain;
using Copren.Net.Domain.Messaging.Messages;
using Copren.Net.Domain.Messaging.Transport;
using Nito.AsyncEx;
using Serilog;

namespace Copren.Net.Core.Messaging.Protocol
{
    public class TransportProtocolV1 : ITransportProtocol
    {
        public event ConnectionHandler OnConnected;
        public event ConnectionHandler OnDisconnected;
        private readonly AsyncAutoResetEvent _asyncEvent = new AsyncAutoResetEvent(false);
        private TaskCompletionSource<HeloAckMessage> _receiveCts;
        private EndPoint _serverEndPoint;
        private Guid _clientId;
        private readonly ILogger _logger;

        public TransportProtocolV1(ILogger logger)
        {
            _logger = logger.ForContext<ITransportProtocol>();
        }

        public async Task Connect(TransportManager transportManager, CancellationToken ct = default)
        {
            _logger.Information("Handling connection handshake");

            transportManager.OnConnected += OnTransportConnected;
            transportManager.OnReceived += OnTransportReceived;

            var tcpTransport = transportManager.Get(ProtocolType.Tcp);
            var udpTransport = transportManager.Get(ProtocolType.Udp);

            await tcpTransport.StartAsync(ct);
            await _asyncEvent.WaitAsync(ct);

            _logger.Verbose("TCP Transport connected; sending Helo");

            _receiveCts = new TaskCompletionSource<HeloAckMessage>();
            await tcpTransport.SendServerMessageAsync(Message.Serialize(new HeloMessage()));
            var ack = await _receiveCts.Task;

            _logger.Verbose("Received TCP HeloAck");

            await udpTransport.StartAsync(ct);
            await _asyncEvent.WaitAsync(ct);

            _logger.Verbose("UDP Transport connected; sending Helo");

            _receiveCts = new TaskCompletionSource<HeloAckMessage>();
            await udpTransport.SendServerMessageAsync(Message.Serialize(new HeloMessage { ClientId = ack.ClientId }));
            ack = await _receiveCts.Task;

            _logger.Verbose("Received UDP HeloAck");

            transportManager.OnConnected -= OnTransportConnected;
            transportManager.OnReceived -= OnTransportReceived;

            _clientId = ack.ClientId;
            await tcpTransport.SendServerMessageAsync(Message.Serialize(new ClientReadyMessage { ClientId = _clientId }));

            _logger.Information("Connection established with {RemoteEndPoint}; assigned {ClientId:s}", _serverEndPoint, _clientId);

            await (OnConnected?.Invoke(_clientId) ?? Task.CompletedTask);
        }

        private Task OnTransportConnected(ProtocolType protocol, EndPoint endPoint, SocketAsyncEventArgs eventArgs)
        {
            _logger.Verbose($"{protocol}://{endPoint} transport connected");
            _serverEndPoint = endPoint;
            _asyncEvent.Set();
            return Task.CompletedTask;
        }

        private Task OnTransportReceived(ProtocolType protocol, EndPoint endPoint, TransportMessage rawMessage)
        {
            if (_receiveCts.Task.IsCompleted) return Task.CompletedTask;

            try
            {
                var message = rawMessage.DeserializeMessage<HeloAckMessage>();
                _receiveCts.SetResult(message);
            }
            catch
            {
                /* Ignore */
            }

            return Task.CompletedTask;
        }

        public async Task Disconnect(TransportManager transportManager, CancellationToken ct = default)
        {
            _logger.Information("Handling disconnection");

            await (OnDisconnected?.Invoke(_clientId) ?? Task.CompletedTask);
            var olehMessage = Message.Serialize(new OlehMessage());
            await Task.WhenAny(
                transportManager.Get(ProtocolType.Tcp).SendServerMessageAsync(olehMessage),
                transportManager.Get(ProtocolType.Udp).SendServerMessageAsync(olehMessage));
        }
    }
}