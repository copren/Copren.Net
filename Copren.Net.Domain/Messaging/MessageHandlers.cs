using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Copren.Net.Domain.Messaging.Messages;

namespace Copren.Net.Domain.Messaging
{
    public delegate Task MessageHandler(Uri uri, Message message);
}