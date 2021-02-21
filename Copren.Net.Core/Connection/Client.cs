using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Copren.Shared.Async;
using Copren.Net.Core.Context;
using Copren.Net.Core.Messaging;
using Copren.Net.Core.Messaging.Protocol;
using Copren.Net.Core.Messaging.Transport;
using Copren.Net.Core.Middleware;
using Copren.Net.Domain;
using Copren.Net.Domain.Attributes;
using Copren.Net.Domain.Extensions;
using Copren.Net.Domain.Messaging.Messages;
using Serilog;

namespace Copren.Net.Core.Connection
{
    public partial class Client : AsyncTask
    {
        public event ConnectionHandler OnConnected;
        public event ConnectionHandler OnDisconnected;
        public IServiceProvider ServiceProvider { get; }
        public EndPoint RemoteEndPoint { get; }
        public Guid Id { get; private set; }
        public bool IsConnected { get; private set; }
        private ClientState _clientState = ClientState.Disconnected;
        private readonly TransportManager _transportManager;
        private readonly ITransportProtocol _transportProtocol;
        private readonly MessageCenter _messageCenter;
        private readonly IEnumerable<IMiddleware> _middleware;
        private readonly ILogger _logger;

        public Client(
            IServiceProvider serviceProvider,
            MessageCenter messageCenter,
            ITransportProtocol transportProtocol,
            TransportManager transportManager,
            ClientOptions clientOptions,
            IEnumerable<IMiddleware> middleware,
            ILogger logger)
        {
            ServiceProvider = serviceProvider;
            _messageCenter = messageCenter;
            _messageCenter.OnMessage += OnMessage;
            _transportProtocol = transportProtocol;
            _transportProtocol.OnConnected += OnConnect;
            _transportProtocol.OnDisconnected += OnDisconnect;
            _transportManager = transportManager;
            RemoteEndPoint = clientOptions.RemoteEndPoint;
            _middleware = middleware;
            _logger = logger.ForContext<Client>();
        }

        public new async Task StopAsync()
        {
            lock (this)
            {
                if (_clientState == ClientState.Disconnecting) throw new InvalidStateException(_clientState, "Already disconnecting");
                _clientState = ClientState.Disconnecting;
            }

            _logger.Information("Client stopping");

            await _messageCenter.StopAsync();

            lock (this)
            {
                _clientState = ClientState.Disconnected;
            }

            await base.StopAsync();
        }

        protected override void Run()
        {
            Task.Run(async () =>
            {
                lock (this)
                {
                    if (_clientState == ClientState.Connected) throw new InvalidStateException(_clientState, "Already connecting");
                    _clientState = ClientState.Connecting;
                }

                _logger.Information("Client connecting to {RemoteEndPoint}", RemoteEndPoint);

                await _messageCenter.StartAsync(CancellationToken);
                await _transportProtocol.Connect(_transportManager, CancellationToken);

                lock (this)
                {
                    _clientState = ClientState.Connected;
                }

                while (true) { await Task.Delay(100); }
            });
        }

        private Task OnMessage(Uri uri, Message message)
        {
            var clientContext = new ClientContext(this, uri);

            var middlewareEnumerator = _middleware.GetEnumerator();
            Func<Task> next = null;
            next = () =>
            {
                if (!middlewareEnumerator.MoveNext()) return Task.CompletedTask;
                _logger.Verbose("Executing client {Middleware}.{Method}", middlewareEnumerator.Current.GetType(), nameof(IMiddleware.OnMessage));
                return middlewareEnumerator.Current.OnMessage(clientContext, message, next);
            };

            return next();
        }

        private async Task OnConnect(Guid clientId)
        {
            Id = clientId;
            IsConnected = true;

            var clientContext = new ClientContext(this, null);

            var middlewareEnumerator = _middleware.GetEnumerator();
            Func<Task> next = null;
            next = () =>
            {
                if (!middlewareEnumerator.MoveNext()) return Task.CompletedTask;
                _logger.Verbose("Executing client {Middleware}.{Method}", middlewareEnumerator.Current.GetType(), nameof(IMiddleware.OnConnected));
                return middlewareEnumerator.Current.OnConnected(clientContext, next);
            };

            await next();

            await OnConnected?.Invoke(clientId);
        }

        private async Task OnDisconnect(Guid clientId)
        {
            IsConnected = false;

            var clientContext = new ClientContext(this, null);

            var middlewareEnumerator = _middleware.GetEnumerator();
            Func<Task> next = null;
            next = () =>
            {
                if (!middlewareEnumerator.MoveNext()) return Task.CompletedTask;
                _logger.Verbose("Executing client {Middleware}.{Method}", middlewareEnumerator.Current.GetType(), nameof(IMiddleware.OnDisconnected));
                return middlewareEnumerator.Current.OnDisconnected(clientContext, next);
            };

            await next();

            await OnDisconnected?.Invoke(clientId);
        }

        public Task SendServerMessageAsync<T>(T message)
            where T : Message
        {
            var protocolType = message
                .GetType()
                .GetTypeInfo()
                .GetCustomAttribute(typeof(PriorityMessage)) != null
                ? ProtocolType.Tcp
                : ProtocolType.Udp;
            return _messageCenter.SendServerMessageAsync(protocolType, message);
        }

        public Task SendMessageAsync<T>(Uri uri, T message)
            where T : Message
        {
            return _messageCenter.SendMessageAsync(uri, message);
        }
    }
}