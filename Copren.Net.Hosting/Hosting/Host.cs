using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Copren.Net.Domain;
using Copren.Net.Domain.Messaging.Messages;
using Serilog;
using Copren.Net.Hosting.Messaging;
using Copren.Net.Domain.Extensions;
using Copren.Net.Hosting.Context;
using Copren.Net.Hosting.Middleware;
using System.Reflection;
using Copren.Net.Domain.Attributes;

namespace Copren.Net.Hosting.Hosting
{
    public class Host
    {
        public event ConnectionHandler OnConnected;
        public event ConnectionHandler OnDisconnected;
        public bool IsListening { get; }
        public EndPoint LocalEndPoint { get; }
        public IEnumerable<Client> Clients => _clientCollection.Clients;
        public IServiceProvider ServiceProvider { get; }
        private readonly ClientCollection _clientCollection;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly MessageCenter _messageCenter;
        private readonly IEnumerable<IMiddleware> _middleware;
        private Task _hostTransportTask;
        private readonly ILogger _logger;

        public Host(IServiceProvider serviceProvider,
            MessageCenter messageCenter,
            ClientCollection clientCollection,
            IEnumerable<IMiddleware> middleware,
            HostOptions hostOptions,
            ILogger logger)
        {
            ServiceProvider = serviceProvider;
            _messageCenter = messageCenter;
            _clientCollection = clientCollection;
            _middleware = middleware;
            _messageCenter.OnMessage += OnMessage;
            _messageCenter.OnClosed += OnClosed;
            LocalEndPoint = hostOptions.LocalEndPoint;
            _logger = logger.ForContext<Host>();
        }

        public Task StartAsync()
        {
            if (IsListening) return Task.CompletedTask;

            _logger.Information("Listening on (tcp|udp)://{EndPoint}", LocalEndPoint);

            _cancellationTokenSource = new CancellationTokenSource();
            _hostTransportTask = _messageCenter.StartAsync(_cancellationTokenSource.Token);
            return _hostTransportTask;
        }

        public Task StopAsync()
        {
            if (!IsListening) return Task.CompletedTask;
            _cancellationTokenSource.Cancel(false);
            return _hostTransportTask;
        }

        public Task SendClientMessageAsync<T>(Guid clientId, T message)
            where T : Message
        {
            if (!_clientCollection.TryGet(clientId, out var client))
            {
                _logger.Debug("Unknown client {ClientId}", clientId);
                return Task.CompletedTask;
            }

            var protocolType = message
                .GetType()
                .GetTypeInfo()
                .GetCustomAttribute(typeof(PriorityMessage)) != null
                ? ProtocolType.Tcp
                : ProtocolType.Udp;

            return _messageCenter.SendMessage(client.Uris[protocolType], message);
        }

        public Task SendClientMessageAsync<T>(Client client, T message)
            where T : Message
        {
            var protocolType = message
                .GetType()
                .GetTypeInfo()
                .GetCustomAttribute(typeof(PriorityMessage)) != null
                ? ProtocolType.Tcp
                : ProtocolType.Udp;

            return _messageCenter.SendMessage(client.Uris[protocolType], message);
        }

        public Task OnClosed(Uri uri)
        {
            return ProcessClientDisconnect(uri);
        }

        public Task OnMessage(Uri uri, Message message)
        {
            _logger.Verbose("Received {MessageType} message from {Uri}", message.GetType(), uri);

            switch (message)
            {
                case HeloMessage helo:
                    return ProcessClientConnect(uri, helo);
                case ClientReadyMessage clientReady:
                    return ProcessClientReady(uri, clientReady);
                case OlehMessage _:
                    return ProcessClientDisconnect(uri);
            }

            _clientCollection.TryGet(uri, out var client);
            var hostContext = new HostContext(this, uri, client?.ClientId);

            var middlewareEnumerator = _middleware.GetEnumerator();
            Func<Task> next = null;
            next = () =>
            {
                if (!middlewareEnumerator.MoveNext()) return Task.CompletedTask;
                _logger.Verbose("Executing host {Middleware}.{Method}", middlewareEnumerator.Current.GetType(), nameof(IMiddleware.OnMessage));
                return middlewareEnumerator.Current.OnMessage(hostContext, message, next);
            };

            return next();
        }

        private Task ProcessClientReady(Uri uri, ClientReadyMessage clientReady)
        {
            if (!_clientCollection.TryGet(clientReady.ClientId, out var client))
            {
                // Something weird is going on
                // Ignore the message
                return Task.CompletedTask;
            }

            return FireOnConnected(client, uri);
        }

        private async Task ProcessClientConnect(Uri uri, HeloMessage helo)
        {
            Client client;

            // TODO : Move into IHostProtocol

            // Check if we're registering another protocol
            if (helo.ClientId.HasValue)
            {
                if (!_clientCollection.TryGet(helo.ClientId.Value, out client))
                {
                    // Something weird is going on
                    // Ignore the message
                    return;
                }

                // Register the protocol
                client.Uris.Add(uri.ProtocolType(), uri);

                // Update the client
                _clientCollection.Add(client);
            }
            else if (_clientCollection.TryGet(uri, out client))
            {
                if (client.ClientId.Equals(helo.ClientId))
                {
                    // Ignore
                }
                else
                {
                    // TODO: Renegotiate security   
                }

                return;
            }
            else
            {
                // No matching client
                client = new Client(uri, helo);
                _clientCollection.Add(client);
            }

            // Acknowledge the request
            var ack = new HeloAckMessage(client);
            await _messageCenter.SendMessage(uri, ack);
        }

        private Task ProcessClientDisconnect(Uri uri)
        {
            if (!_clientCollection.TryGet(uri, out var client))
            {
                return Task.CompletedTask;
            }

            _clientCollection.Remove(client);

            return FireOnDisconnected(client, uri);
        }

        private async Task FireOnConnected(Client client, Uri clientUri)
        {
            var hostContext = new HostContext(this, clientUri);

            var middlewareEnumerator = _middleware.GetEnumerator();
            Func<Task> next = null;
            next = () =>
            {
                if (!middlewareEnumerator.MoveNext()) return Task.CompletedTask;
                _logger.Verbose("Executing host {Middleware}.{Method}", middlewareEnumerator.Current.GetType(), nameof(IMiddleware.OnClientConnected));
                return middlewareEnumerator.Current.OnClientConnected(hostContext, client, next);
            };

            await next();

            // Fire the connected event
            await (OnConnected?.Invoke(client) ?? Task.CompletedTask);
        }

        private async Task FireOnDisconnected(Client client, Uri clientUri)
        {
            var hostContext = new HostContext(this, clientUri);

            var middlewareEnumerator = _middleware.GetEnumerator();
            Func<Task> next = null;
            next = () =>
            {
                if (!middlewareEnumerator.MoveNext()) return Task.CompletedTask;
                _logger.Verbose("Executing host {Middleware}.{Method}", middlewareEnumerator.Current.GetType(), nameof(IMiddleware.OnClientDisconnected));
                return middlewareEnumerator.Current.OnClientDisconnected(hostContext, client, next);
            };

            await next();

            // Fire the disconnected event
            await (OnDisconnected?.Invoke(client) ?? Task.CompletedTask);
        }
    }
}