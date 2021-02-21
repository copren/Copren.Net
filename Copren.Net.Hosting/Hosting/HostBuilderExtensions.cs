using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Configuration;
using Serilog.Events;
using Copren.Net.Hosting.Middleware;
using Copren.Logging.Shared;

namespace Copren.Net.Hosting.Hosting
{
    public static class HostBuilderExtensions
    {
        public static HostBuilder ListenTo(this HostBuilder self, Action<HostOptions> options)
        {
            var hostOptions = new HostOptions();
            options.Invoke(hostOptions);
            self.Configure(s => s.AddSingleton(hostOptions));
            return self;
        }

        public static HostBuilder AddMiddleware<T>(this HostBuilder self)
            where T : IMiddleware
        {
            self.Configure(s => s.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IMiddleware), typeof(T), ServiceLifetime.Singleton)));
            return self;
        }

        public static HostBuilder AddDebugging(this HostBuilder self, LogEventLevel minimumLogLevel = LogEventLevel.Debug)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.Console(formatProvider: new GuidFormatter());
            loggerConfiguration.MinimumLevel.ControlledBy(new LoggingLevelSwitch(minimumLogLevel));

            self.Configure(s => s.Replace(
                new ServiceDescriptor(
                    typeof(ILogger),
                    loggerConfiguration.CreateLogger())));
            return self;
        }

        public static HostBuilder AddLogging<T>(this HostBuilder self, T logger)
            where T : ILogger
        {
            self.Configure(s => s.Replace(new ServiceDescriptor(typeof(ILogger), logger)));
            return self;
        }

        public static HostBuilder AddLogging<T>(this HostBuilder self)
            where T : ILogger
        {
            self.Configure(s => s.Replace(new ServiceDescriptor(typeof(ILogger), typeof(T))));
            return self;
        }
    }
}