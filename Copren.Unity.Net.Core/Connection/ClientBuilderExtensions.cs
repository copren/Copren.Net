using System;
using Copren.Unity.Net.Core.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Copren.Logging.Shared;

namespace Copren.Unity.Net.Core.Connection
{
    public static class ClientBuilderExtensions
    {
        public static ClientBuilder ConnectTo(this ClientBuilder self, Action<ClientOptions> options)
        {
            var hostOptions = new ClientOptions();
            options.Invoke(hostOptions);
            self.Configure(s => s.AddSingleton(hostOptions));
            return self;
        }

        public static ClientBuilder AddMiddleware<T>(this ClientBuilder self)
            where T : IMiddleware
        {
            self.Configure(s => s.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IMiddleware), typeof(T), ServiceLifetime.Singleton)));
            return self;
        }

        public static ClientBuilder AddDebugging(this ClientBuilder self, LogEventLevel minimumLogLevel = LogEventLevel.Debug)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.Console(
                    formatProvider: new GuidFormatter()
                // outputTemplate: "[{Timestamp:mm:ss} {Level:u3} {ClientId:s}] {Message:lj}{NewLine}{Exception}"
                );
            loggerConfiguration.MinimumLevel.ControlledBy(new LoggingLevelSwitch(minimumLogLevel));

            self.Configure(s => s.Replace(
                new ServiceDescriptor(
                    typeof(ILogger),
                    loggerConfiguration.CreateLogger())));
            return self;
        }

        public static ClientBuilder AddLogging<T>(this ClientBuilder self, T logger)
            where T : ILogger
        {
            self.Configure(s => s.Replace(new ServiceDescriptor(typeof(ILogger), logger)));
            return self;
        }

        public static ClientBuilder AddLogging<T>(this ClientBuilder self)
            where T : ILogger
        {
            self.Configure(s => s.Replace(new ServiceDescriptor(typeof(ILogger), typeof(T))));
            return self;
        }
    }
}