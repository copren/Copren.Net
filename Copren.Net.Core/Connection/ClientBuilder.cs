using System;
using Copren.Net.Core.Messaging;
using Copren.Net.Core.Messaging.Protocol;
using Copren.Net.Core.Messaging.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Copren.Net.Core.Connection
{
    public class ClientBuilder
    {

        private readonly IServiceCollection _serviceCollection = new ServiceCollection();

        public ClientBuilder()
        {
            AddDefaultServices();
        }

        public Client Build()
        {
            var serviceProvider = _serviceCollection.BuildServiceProvider();
            return serviceProvider.GetRequiredService<Client>();
        }

        public ClientBuilder Configure(Action<IServiceCollection> services)
        {
            services.Invoke(_serviceCollection);
            return this;
        }

        private void AddDefaultServices()
        {
            _serviceCollection.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IClientTransport), typeof(TcpClientTransport), ServiceLifetime.Singleton));
            _serviceCollection.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IClientTransport), typeof(UdpClientTransport), ServiceLifetime.Singleton));
            _serviceCollection.AddSingleton<TransportManager>();
            _serviceCollection.AddSingleton<ITransportProtocol, TransportProtocolV1>();
            _serviceCollection.AddSingleton<MessageCenter>();
            _serviceCollection.AddSingleton<Client>();
        }
    }
}