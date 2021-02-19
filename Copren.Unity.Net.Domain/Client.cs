using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Copren.Unity.Net.Domain;
using Copren.Unity.Net.Domain.Extensions;
using Copren.Unity.Net.Domain.Messaging.Messages;

namespace Copren.Unity.Net.Domain
{
    public class Client
    {
        public Guid ClientId { get; } = Guid.NewGuid();
        public IDictionary<ProtocolType, Uri> Uris { get; } = new Dictionary<ProtocolType, Uri>();
        public DateTimeOffset Connected { get; }
        public DateTimeOffset LastReceived { get; }

        public Client(Uri uri, HeloMessage helo)
        {
            Uris.Add(uri.ProtocolType(), uri);
        }
    }
}