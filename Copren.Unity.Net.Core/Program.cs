// using System;
// using System.IO;
// using System.Net;
// using System.Net.Sockets;
// using System.Runtime.Serialization.Formatters.Binary;
// using System.Text;
// using System.Threading;
// using System.Threading.Tasks;
// using Copren.Unity.Net.Core.Connection;
// using Copren.Unity.Net.Core.Messaging;
// using Copren.Unity.Net.Core.Messaging.Protocol;
// using Copren.Unity.Net.Core.Messaging.Transport;
// using Copren.Unity.Net.Domain.Messaging.Messages;
// using Microsoft.Extensions.CommandLineUtils;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Configuration.Yaml;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.DependencyInjection.Extensions;
// using Serilog;

// namespace Copren.Unity.Net.Core
// {
//     class Program
//     {
//         static void Main(string[] args)
//         {
//             var app = new CommandLineApplication();
//             app.Command("connect", config => {
//                 var hostOption = config.Option("-h|--host", "", CommandOptionType.SingleValue);
//                 var portOption = config.Option("-p|--port", "", CommandOptionType.SingleValue);
//                 var clientHostOption = config.Option("-H|--client-host", "", CommandOptionType.SingleValue);
//                 var clientPortOption = config.Option("-P|--client-port", "", CommandOptionType.SingleValue);
//                 var protocolOption = config.Option("-r|--protocol", "", CommandOptionType.SingleValue);
//                 var messageArg = config.Argument("message", "");
//                 config.OnExecute(async () => {
//                     var localEndPoint = new IPEndPoint(IPAddress.Parse(clientHostOption.Value() ?? "127.0.0.1"), int.Parse(clientPortOption.Value() ?? "3000"));
//                     var remoteEndPoint = new IPEndPoint(IPAddress.Parse(hostOption.Value() ?? "127.0.0.1"), int.Parse(portOption.Value() ?? "3000"));

//                     if (!Enum.TryParse<ProtocolType>(protocolOption.Value(), out var precastedProtocolType)) {
//                         throw new Exception("Invalid protocol type");
//                     }

//                     var services = new ServiceCollection();
//                     services.AddSingleton<IConfiguration>(
//                         new ConfigurationBuilder()
//                             .AddYamlFile("appsettings.yaml")
//                             .Build());
//                     services.AddSingleton<ILogger>(s =>
//                         new LoggerConfiguration()
//                             .ReadFrom
//                             .Configuration(s.GetRequiredService<IConfiguration>())
//                             .CreateLogger());
//                     services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IClientTransport), typeof(TcpClientTransport), ServiceLifetime.Singleton));
//                     services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IClientTransport), typeof(UdpClientTransport), ServiceLifetime.Singleton));
//                     services.AddSingleton<TransportManager>();
//                     services.AddSingleton<ITransportProtocol, TransportProtocolV1>();
//                     services.AddSingleton<MessageCenter>();
//                     services.AddSingleton<ClientOptions>(new ClientOptions {
//                         LocalEndPoint = localEndPoint,
//                         RemoteEndPoint = remoteEndPoint
//                     });
//                     services.AddSingleton<Client>();
//                     var provider = services.BuildServiceProvider();

//                     var client = provider.GetRequiredService<Client>();
//                     await client.StartAsync();

//                     while(true) { await Task.Delay(100); }
//                 });
//             });
//             app.HelpOption("-h|--help");
//             app.Execute(args);
//         }
//     }
// }
