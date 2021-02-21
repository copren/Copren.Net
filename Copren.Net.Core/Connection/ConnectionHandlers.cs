using System;
using System.Threading.Tasks;

namespace Copren.Net.Core.Connection
{
    public class ConnectionHandlers
    {
        public delegate Task ConnectionHandler(Guid clientId);
    }
}