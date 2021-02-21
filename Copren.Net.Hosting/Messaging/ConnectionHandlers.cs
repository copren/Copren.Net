using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Copren.Net.Hosting.Messaging
{
    public delegate Task ConnectionHandler(Uri uri);
}