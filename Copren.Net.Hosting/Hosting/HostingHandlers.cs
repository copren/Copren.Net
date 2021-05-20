using System;
using System.Threading.Tasks;
using Copren.Net.Domain;
using Copren.Net.Domain.Messaging.Messages;

namespace Copren.Net.Hosting.Hosting
{
    public delegate Task ConnectionHandler(Client client);
    public delegate Task MessageHandler(Client client, Message message);
    public delegate Task ExceptionHandler(Client client, Exception exception);
}