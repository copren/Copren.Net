using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Copren.Net.Domain;
using Copren.Net.Domain.Extensions;
using Copren.Net.Domain.Messaging.Messages;

namespace Copren.Net.Domain
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