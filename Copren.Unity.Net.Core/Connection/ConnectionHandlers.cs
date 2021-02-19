using System;
using System.Threading.Tasks;

namespace Copren.Unity.Net.Core.Connection
{
    public class ConnectionHandlers
    {
        public delegate Task ConnectionHandler(Guid clientId);
    }
}