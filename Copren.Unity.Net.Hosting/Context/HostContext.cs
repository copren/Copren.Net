using System;
using Copren.Unity.Net.Hosting.Hosting;

namespace Copren.Unity.Net.Hosting.Context
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