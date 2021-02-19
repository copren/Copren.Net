using System;
using Copren.Unity.Net.Core.Connection;

namespace Copren.Unity.Net.Core.Context
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