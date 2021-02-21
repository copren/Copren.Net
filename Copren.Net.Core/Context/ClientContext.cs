using System;
using Copren.Net.Core.Connection;

namespace Copren.Net.Core.Context
{
    public class ClientContext
    {
        public Client Client { get; }
        public Uri SenderUri { get; }

        internal ClientContext(Client client, Uri uri)
        {
            Client = client;
            SenderUri = uri;
        }
    }
}