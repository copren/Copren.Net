using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Copren.Unity.Net.Domain.Messaging.Transport;

namespace Copren.Unity.Net.Hosting.Messaging.Transport
{
    public class TransportManager
    {
        public event TransportHandler OnConnected;
        public event TransportMessageHandler OnReceived;
        public event TransportHandler OnClosed;

        public EndPoint LocalEndPoint { get; }
        private readonly IEnumerable<IHostTransport> _hostTransports;
        private readonly IDictionary<ProtocolType, IHostTransport> _protocolToHostMapping = new Dictionary<ProtocolType, IHostTransport>();
        private CancellationTokenSource _cancellationTokenSource;
        private IList<Task> _hostTransportTasks;

        public TransportManager(HostOptions hostOptions, IEnumerable<IHostTransport> hostTransports)
        {
            LocalEndPoint = hostOptions.LocalEndPoint;
            _hostTransports = hostTransports;

            foreach (var transport in _hostTransports)
            {
                _protocolToHostMapping.Add(transport.ProtocolType, transport);
            }
        }

        public Task StartAsync(CancellationToken ct = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);

            _hostTransportTasks = new List<Task>();
            foreach (var hostTransport in _hostTransports)
            {
                hostTransport.OnConnected += (p, e, s) => OnConnected?.Invoke(p, e, s);
                hostTransport.OnReceived += (p, e, m) => OnReceived?.Invoke(p, e, m);
                hostTransport.OnClosed += (p, e, s) => OnClosed?.Invoke(p, e, s);
                _hostTransportTasks.Add(hostTransport.StartAsync(ct));
            }

            return Task.WhenAll(_hostTransportTasks);
        }

        public Task StopAsync()
        {
            _cancellationTokenSource.Cancel();
            return Task.WhenAll(_hostTransportTasks);
        }

        public Task SendMessage(ProtocolType protocolType, EndPoint endPoint, byte[] data)
        {
            var transport = _protocolToHostMapping[protocolType];
            return transport.SendMessage(endPoint, data);
        }
    }
}