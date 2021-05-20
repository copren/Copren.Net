using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Copren.Net.Hosting.Messaging;
using Copren.Net.Hosting.Messaging.Transport;
using Microsoft.Extensions.Hosting;

namespace Copren.Net.Hosting.Hosting
{
    public class CoprenNetHostBuilder
    {
        private readonly IHostBuilder _hostBuilder;

        public CoprenNetHostBuilder(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger());
                services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IHostTransport), typeof(UdpHostTransport), ServiceLifetime.Singleton));
                services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IHostTransport), typeof(TcpHostTransport), ServiceLifetime.Singleton));
                services.AddSingleton<TransportManager>();
                services.AddSingleton<MessageCenter>();
                services.AddSingleton<ClientCollection>();
                services.AddSingleton<CoprenNetHost>();
            });
        }

        public CoprenNetHostBuilder Configure(Action<IServiceCollection> services)
        {
            _hostBuilder.ConfigureServices(s => services.Invoke(s));
            return this;
        }
    }
}