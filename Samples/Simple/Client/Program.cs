using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Copren.Unity.Net.Contrib.Peers;
using Copren.Unity.Net.Contrib.Peers.State;
using Copren.Unity.Net.Contrib.State;
using Copren.Unity.Net.Core.Connection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using Serilog;
using Serilog.Events;
using Simple.Shared;

namespace Client
{
    class Program
    {
        private static readonly AsyncAutoResetEvent _closedEvent = new AsyncAutoResetEvent();

        static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            BuildApplication().Execute(args);
        }

        private class PeerHandler : IPeerHandler
        {
            private readonly ILogger _logger;

            public PeerHandler(ILogger logger)
            {
                _logger = logger.ForContext<PeerHandler>();
            }

            public Task OnPeerConnected(Guid peerId)
            {
                _logger.Information("Peer {PeerId:s} connected", peerId);
                return Task.CompletedTask;
            }

            public Task OnPeerDisconnected(Guid peerId)
            {
                _logger.Information("Peer {PeerId:s} disconnected", peerId);
                return Task.CompletedTask;
            }
        }

        private class StateHandler : IClientStateHandler, IPeerStateHandler
        {
            private readonly ILogger _logger;

            public StateHandler(ILogger logger)
            {
                _logger = logger.ForContext<StateHandler>();
            }

            public Task OnPeerStateChanged(Copren.Unity.Net.Core.Connection.Client client, Guid peerId, object state)
            {
                _logger.Information("Peer {PeerId:s} reported state = {Position}", peerId, state);
                return Task.CompletedTask;
            }

            public Task OnStateChanged(Copren.Unity.Net.Core.Connection.Client client, bool fromServer, object state)
            {
                // Ignore peer updates
                if (!fromServer) return Task.CompletedTask;

                return Task.FromResult(true);
            }
        }

        private static async Task<int> Connect(
            string addressString, string portString,
            string localAddressString, string localPortString)
        {
            var remoteEndPoint = ParseEndPoint(addressString, portString);
            var localEndPoint = ParseEndPoint(localAddressString, localPortString);

            var builder = new ClientBuilder();
            builder.AddDebugging(LogEventLevel.Verbose);
            builder.AddPeers()
                .AddPeerListener<PeerHandler>();
            builder.AddState()
                .AddStateListener<StateHandler>()
                .UpdatePeers()
                .AddStateListener<StateHandler>();
            builder.ConnectTo(options =>
            {
                options.RemoteEndPoint = remoteEndPoint;
                options.LocalEndPoint = null;//localEndPoint;
            });
            var client = builder.Build();

            client.OnConnected += clientId =>
            {
                client.ServiceProvider.GetRequiredService<ILogger>().Information("Client connected; assigned {ClientId:s}", clientId);
                return Task.CompletedTask;
            };
            client.OnDisconnected += clientId =>
            {
                client.ServiceProvider.GetRequiredService<ILogger>().Information("Client disconnected as {ClientId:s}", clientId);
                _closedEvent.Set();
                return Task.CompletedTask;
            };

            await client.StartAsync();
            var _ = Task.Run(async () =>
            {
                var position = new StateMessage
                {
                    X = 0,
                    Y = 0
                };
                var random = new Random();
                while (true)
                {
                    await Task.Delay(1000);

                    position = client.State().GetState() == null ? position : (StateMessage)client.State().GetState();

                    position.X += random.NextDouble() - .5;
                    position.Y += random.NextDouble() - .5;

                    var logger = client.ServiceProvider.GetRequiredService<ILogger>();
                    logger.ForContext<Program>().Information("Client {ClientId:s} sending state = {Position}", client.Id, position);
                    await client.UpdateStateAsync(position);
                }
            });
            await _closedEvent.WaitAsync();

            return 0;
        }

        private static EndPoint ParseEndPoint(string addressString, string portString)
        {
            if (!int.TryParse(portString, out var port))
            {
                throw new ArgumentException(nameof(port));
            }

            EndPoint endPoint;
            if (IPAddress.TryParse(addressString, out var ipAddress))
            {
                endPoint = new IPEndPoint(ipAddress, port);
            }
            else
            {
                endPoint = new DnsEndPoint(addressString, port);
            }

            return endPoint;
        }

        private static CommandLineApplication BuildApplication()
        {
            return new CommandLineApplication()
                .Command("serve", command =>
                {
                    command.HelpOption("--help");
                    var host = command.Option("-h|--host <address>", "", CommandOptionType.SingleValue);
                    var port = command.Option("-p|--port <port>", "", CommandOptionType.SingleValue);
                    var localHost = command.Option("-H|--local-host <address>", "", CommandOptionType.SingleValue);
                    var localPort = command.Option("-P|--local-port <port>", "", CommandOptionType.SingleValue);
                    command.OnExecute(() =>
                    {
                        return Connect(
                            host.Value() ?? "127.0.0.1", port.Value() ?? "3000",
                            localHost.Value() ?? "127.0.0.1", localPort.Value() ?? "3000");
                    });
                });
        }
    }
}
