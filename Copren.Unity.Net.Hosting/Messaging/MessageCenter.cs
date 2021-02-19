using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Copren.Unity.Net.Domain;
using Copren.Unity.Net.Domain.Messaging;
using Copren.Unity.Net.Domain.Messaging.Messages;
using Copren.Unity.Net.Domain.Messaging.Transport;
using Copren.Unity.Net.Hosting.Messaging.Transport;
using Copren.Unity.Net.Domain.Extensions;
using System;

namespace Copren.Unity.Net.Hosting.Messaging
{
    public class MessageCenter
    {
        public event ConnectionHandler OnConnected;
        public event MessageHandler OnMessage;
        public event ConnectionHandler OnClosed;

        private readonly TransportManager _transportManager;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        public MessageCenter(TransportManager transportManager)
        {
            _transportManager = transportManager;
            _transportManager.OnConnected += (p, e, _) => OnConnected?.Invoke(e.ToUri(p));
            _transportManager.OnReceived += OnReceive;
            _transportManager.OnClosed += (p, e, _) => OnClosed?.Invoke(e.ToUri(p));
        }

        public Task StartAsync(CancellationToken ct = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _task = _transportManager.StartAsync(_cancellationTokenSource.Token);
            return _task;
        }

        public Task StopAsync()
        {
            _cancellationTokenSource.Cancel();
            return _task;
        }

        private Task OnReceive(ProtocolType protocol, EndPoint endPoint, TransportMessage message)
        {
            var payload = message.DeserializeMessage();
            return OnMessage?.Invoke(endPoint.ToUri(protocol), payload);
        }

        public Task SendMessage<T>(Uri uri, T message)
            where T : Message
        {
            var data = Message.Serialize(message);
            return _transportManager.SendMessage(uri.ProtocolType(), uri.ToEndPoint(), data);
        }
    }
}