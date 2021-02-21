using System;
using Copren.Net.Hosting.Hosting;

namespace Copren.Net.Hosting.Context
{
    public class HostContext
    {
        public Host Host { get; }
        public Uri ClientUri { get; }
        public Guid? ClientId { get; }

        public HostContext(Host host, Uri clientUri, Guid? clientId = null)
        {
            Host = host;
            ClientUri = clientUri;
            ClientId = clientId;
        }
    }
}