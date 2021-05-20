using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Configuration;
using Serilog.Events;
using Copren.Net.Hosting.Middleware;
using Copren.Logging.Shared;
using Microsoft.Extensions.Hosting;

namespace Copren.Net.Hosting.Hosting
{
    public static class HostBuilderExtensions
    {
        public static CoprenNetHostBuilder UseCoprenNet(this HostBuilder self, Action<HostOptions> options)
        {
            var coprenHostBuilder = new CoprenNetHostBuilder(self);
            var hostOptions = new HostOptions();
            options.Invoke(hostOptions);
            coprenHostBuilder.Configure(s => s.AddSingleton(hostOptions));
            return coprenHostBuilder;
        }

        public static CoprenNetHostBuilder AddMiddleware<T>(this CoprenNetHostBuilder self)
            where T : IMiddleware
        {
            self.Configure(s => s.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IMiddleware), typeof(T), ServiceLifetime.Singleton)));
            return self;
        }

        public static CoprenNetHostBuilder AddDebugging(this CoprenNetHostBuilder self, LogEventLevel minimumLogLevel = LogEventLevel.Debug)
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

        public static CoprenNetHostBuilder AddLogging<T>(this CoprenNetHostBuilder self, T logger)
            where T : ILogger
        {
            self.Configure(s => s.Replace(new ServiceDescriptor(typeof(ILogger), logger)));
            return self;
        }

        public static CoprenNetHostBuilder AddLogging<T>(this CoprenNetHostBuilder self)
            where T : ILogger
        {
            self.Configure(s => s.Replace(new ServiceDescriptor(typeof(ILogger), typeof(T))));
            return self;
        }
    }
}