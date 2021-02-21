using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Copren.Net.Domain;

namespace Copren.Net.Hosting
{
    public class ClientCollection : IEnumerable<Client>
    {
        public IEnumerable<Client> Clients => _guidToClientMapping.Values.ToImmutableArray();
        private readonly IDictionary<Uri, Client> _uriToClientMapping = new Dictionary<Uri, Client>();
        private readonly IDictionary<Guid, Client> _guidToClientMapping = new Dictionary<Guid, Client>();

        public void Add(Client client)
        {
            foreach (var uri in client.Uris.Values)
            {
                _uriToClientMapping[uri] = client;
            }
            _guidToClientMapping[client.ClientId] = client;
        }

        public void Remove(Client client)
        {
            foreach (var uri in client.Uris.Values)
            {
                _uriToClientMapping.Remove(uri);
            }
            _guidToClientMapping.Remove(client.ClientId);
        }

        public Client Get(Guid clientId)
        {
            return _guidToClientMapping[clientId];
        }

        public bool TryGet(Guid clientId, out Client client)
        {
            client = null;
            if (!Contains(clientId)) return false;
            client = Get(clientId);
            return true;
        }

        public Client Get(Uri uri)
        {
            return _uriToClientMapping[uri];
        }

        public bool TryGet(Uri uri, out Client client)
        {
            client = null;
            if (!Contains(uri)) return false;
            client = Get(uri);
            return true;
        }

        public bool Contains(Guid clientId)
        {
            return _guidToClientMapping.ContainsKey(clientId);
        }

        public bool Contains(Uri uri)
        {
            return _uriToClientMapping.ContainsKey(uri);
        }

        public IEnumerator<Client> GetEnumerator()
        {
            return _guidToClientMapping.Values.ToImmutableList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _guidToClientMapping.Values.ToImmutableList().GetEnumerator();
        }
    }
}