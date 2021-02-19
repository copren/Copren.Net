using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Copren.Unity.Net.Core.Messaging.Protocol;
using Copren.Unity.Net.Core.Messaging.Transport;
using Copren.Unity.Net.Domain.Messaging.Transport;
using Nito.AsyncEx;

namespace Copren.Unity.Net.Core.Messaging.Transport
{
    public class TransportManager
    {
        public event TransportHandler OnConnected;
        public event TransportMessageHandler OnReceived;
        public event TransportHandler OnClosed;

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Dictionary<ProtocolType, IClientTransport> _transports = new Dictionary<ProtocolType, IClientTransport>();

        public TransportManager(IEnumerable<IClientTransport> transports)
        {
            foreach (var transport in transports)
            {
                _transports[transport.ProtocolType] = transport;
                transport.OnConnected += (p, e, saea) => (OnConnected?.Invoke(p, e, saea) ?? Task.CompletedTask);
                transport.OnReceived += (p, e, m) => (OnReceived?.Invoke(p, e, m) ?? Task.CompletedTask);
                transport.OnClosed += (p, e, saea) => (OnClosed?.Invoke(p, e, saea) ?? Task.CompletedTask);
            }
        }

        public Task StartAsync(CancellationToken ct = default)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _cts.Cancel();
            return Task.CompletedTask;
        }

        public IClientTransport Get(ProtocolType protocol)
        {
            return _transports[protocol];
        }

        public Task SendServerMessageAsync(ProtocolType protocolType, byte[] data)
        {
            var transport = _transports[protocolType];
            return transport.SendServerMessageAsync(data);
        }

        public Task SendMessageAsync(ProtocolType protocolType, EndPoint endPoint, byte[] data)
        {
            var transport = _transports[protocolType];
            return transport.SendMessageAsync(endPoint, data);
        }
    }
}