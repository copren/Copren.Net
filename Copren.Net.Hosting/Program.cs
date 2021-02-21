// using System;
// using System.Linq;
// using System.Net;
// using System.Net.Sockets;
// using System.Text;
// using System.Threading.Tasks;
// using Copren.Net.Domain.Messaging.Messages;
// using Microsoft.Extensions.CommandLineUtils;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Configuration.Yaml;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.DependencyInjection.Extensions;
// using Serilog;
// using Copren.Net.Hosting.Hosting;
// using Copren.Net.Hosting.Messaging;
// using Copren.Net.Hosting.Messaging.Transport;
// using Copren.Logging.Shared;

// namespace Copren.Net.Hosting
// {
//     class Program
//     {
//         static void Main(string[] args)
//         {
//             var app = new CommandLineApplication();
//             app.Command("start", config =>
//             {
//                 config.HelpOption("--help");
//                 var hostOption = config.Option("-h|--host", "The host address to listen to", CommandOptionType.SingleValue);
//                 var portOption = config.Option("-p|--port", "The port to listen to", CommandOptionType.SingleValue);
//                 config.OnExecute(() =>
//                 {
//                     // Serilog.Debugging.SelfLog.Enable(Console.Error);
//                     // var services = new ServiceCollection();
//                     // services.AddSingleton<IConfiguration>(
//                     //     new ConfigurationBuilder()
//                     //         .AddYamlFile("appsettings.yaml")
//                     //         .Build());
//                     // services.AddSingleton<ILogger>(s =>
//                     //     new LoggerConfiguration()
//                     //         // .WriteTo.Console(formatProvider: new GuidFormatter())
//                     //         .ReadFrom.Configuration(s.GetRequiredService<IConfiguration>())
//                     //         .CreateLogger());
//                     // services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IHostTransport), typeof(UdpHostTransport), ServiceLifetime.Singleton));
//                     // services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IHostTransport), typeof(TcpHostTransport), ServiceLifetime.Singleton));
//                     // services.AddSingleton<TransportManager>();
//                     // services.AddSingleton<MessageCenter>();
//                     // services.AddSingleton<ClientCollection>();
//                     // services.AddSingleton<HostOptions>(new HostOptions { LocalEndPoint = new IPEndPoint(IPAddress.Parse(hostOption.Value()), int.Parse(portOption.Value())) });
//                     // services.AddSingleton<Host>();
//                     // var provider = services.BuildServiceProvider();

//                     // var host = provider.GetRequiredService<Host>();
//                     // var messageCenter = provider.GetRequiredService<MessageCenter>();
//                     // var logger = provider.GetRequiredService<ILogger>().ForContext<Program>();
//                     // logger.Information("{Guid:s}", Guid.NewGuid());
//                     // host.OnConnected += async (client) => {
//                     //     logger.Information("Client \"{ClientId}\" connected", client.ClientId);
//                     //     foreach (var c in host.Clients) {
//                     //         if (c.ClientId == client.ClientId) continue;
//                     //         await messageCenter.SendMessage(
//                     //             client.Uris[ProtocolType.Udp],
//                     //             new PeerIdentifyMessage(c));
//                     //     }
//                     // };
//                     // host.OnDisconnected += (client) => {
//                     //     logger.Information("Client \"{ClientId:s}\" disconnected", client.ClientId);
//                     //     return Task.CompletedTask;
//                     // };
//                     // await host.StartAsync();

//                     return 0;
//                 });
//             });
//             app.HelpOption("-h|--help");
//             app.Execute(args);
//         }
//     }
// }
