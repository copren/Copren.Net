using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Copren.Net.Core.Messaging.Protocol;
using Copren.Net.Core.Messaging.Transport;
using Copren.Net.Domain.Messaging;
using Copren.Net.Domain.Messaging.Messages;
using Copren.Net.Domain.Messaging.Transport;
using Copren.Net.Domain.Extensions;
using Serilog;

namespace Copren.Net.Core.Messaging
{
    public class MessageCenter
    {
        public event MessageHandler OnMessage;
        private readonly TransportManager _transportManager;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;

        public MessageCenter(TransportManager transportManager, ILogger logger)
        {
            _transportManager = transportManager;
            _transportManager.OnReceived += OnReceive;
            _logger = logger.ForContext<MessageCenter>();
        }

        public async Task StartAsync(CancellationToken ct = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
            await _transportManager.StartAsync(_cancellationTokenSource.Token);
        }

        public Task StopAsync()
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public Task OnReceive(ProtocolType protocol, EndPoint endPoint, TransportMessage message)
        {
            var payload = message.DeserializeMessage();
            _logger.Verbose("OnReceive({Protocol}://{EndPoint} -> {Message})", protocol, endPoint, message.GetType());
            return OnMessage?.Invoke(endPoint.ToUri(protocol), payload);
        }

        public Task SendServerMessageAsync<T>(ProtocolType protocolType, T message)
            where T : Message
        {
            var data = Message.Serialize(message);
            return _transportManager.SendServerMessageAsync(protocolType, data);
        }

        public Task SendMessageAsync<T>(Uri uri, T message)
            where T : Message
        {
            var data = Message.Serialize(message);
            return _transportManager.SendMessageAsync(uri.ProtocolType(), uri.ToEndPoint(), data);
        }
    }
}