using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Copren.Unity.Net.Contrib.Peers;
using Copren.Unity.Net.Contrib.Peers.State;
using Copren.Unity.Net.Contrib.State;
using Copren.Unity.Net.Contrib.State.Extensions;
using Copren.Unity.Net.Contrib.State.Messages;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Copren.Unity.Net.Hosting.Hosting;
using Simple.Shared;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            BuildApplication().Execute(args);
        }

        private class StateHandler : IHostStateHandler
        {
            private readonly ILogger _logger;

            public StateHandler(ILogger logger)
            {
                _logger = logger.ForContext<StateHandler>();
            }

            public Task OnStateChanged(Host host, bool fromServer, Guid clientId, object state)
            {
                try
                {
                    _logger.Verbose("OnStateChanged({Host}, {fromServer}, {clientId:s}, {state})", host, fromServer, clientId, state);

                    // Ignore server updates
                    if (fromServer) return Task.CompletedTask;

                    var position = (StateMessage)state;
                    _logger.Verbose("Client {ClientId} reported state = {Position}", clientId, position);

                    var valid = true;
                    if ((double)position.X > 1)
                    {
                        valid = false;
                        position.X = 1d;
                    }
                    if ((double)position.X < -1)
                    {
                        valid = false;
                        position.X = -1d;
                    }
                    if ((double)position.X > 1)
                    {
                        valid = false;
                        position.X = 1d;
                    }
                    if ((double)position.X < -1)
                    {
                        valid = false;
                        position.X = -1d;
                    }

                    if (!valid)
                    {
                        _logger.Information("Force-fixing client {ClientId:s} state = {Position}", clientId, position);
                        return host.State().UpdateAsync(clientId, position);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "{Exception}", e.Message);
                }

                return Task.CompletedTask;
            }
        }

        private static async Task<int> Serve(string addressString, string portString)
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

            var builder = new HostBuilder();
            builder.AddDebugging(LogEventLevel.Verbose);
            builder.AddPeers();
            // .AddPeerListener<PeerHandler>();
            builder.AddState()
                .AddStateListener<StateHandler>()
                .UpdateClients();
            builder.ListenTo(options =>
            {
                options.LocalEndPoint = endPoint;
            });
            var host = builder.Build();
            host.OnConnected += (client) =>
            {
                host.ServiceProvider.GetRequiredService<ILogger>().Information("Client connected");
                return Task.CompletedTask;
            };

            await host.StartAsync();

            return 0;
        }

        private static CommandLineApplication BuildApplication()
        {
            return new CommandLineApplication()
                .Command("serve", command =>
                {
                    command.HelpOption("--help");
                    var host = command.Option("-h|--host <address>", "", CommandOptionType.SingleValue);
                    var port = command.Option("-p|--port <port>", "", CommandOptionType.SingleValue);
                    command.OnExecute(() =>
                    {
                        return Serve(host.Value() ?? "127.0.0.1", port.Value() ?? "3000");
                    });
                });
        }
    }
}
