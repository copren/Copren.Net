using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Copren.Net.Hosting.Messaging;
using Copren.Net.Hosting.Messaging.Transport;

namespace Copren.Net.Hosting.Hosting
{
    public class HostBuilder
    {
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();

        public HostBuilder()
        {
            AddDefaultServices();
        }

        public Host Build()
        {
            var serviceProvider = _serviceCollection.BuildServiceProvider();
            return serviceProvider.GetRequiredService<Host>();
        }

        public HostBuilder Configure(Action<IServiceCollection> services)
        {
            services.Invoke(_serviceCollection);
            return this;
        }

        private void AddDefaultServices()
        {
            _serviceCollection.AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger());
            _serviceCollection.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IHostTransport), typeof(UdpHostTransport), ServiceLifetime.Singleton));
            _serviceCollection.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IHostTransport), typeof(TcpHostTransport), ServiceLifetime.Singleton));
            _serviceCollection.AddSingleton<TransportManager>();
            _serviceCollection.AddSingleton<MessageCenter>();
            _serviceCollection.AddSingleton<ClientCollection>();
            _serviceCollection.AddSingleton<Host>();
        }
    }
}