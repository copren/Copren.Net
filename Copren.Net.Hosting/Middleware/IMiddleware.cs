using System;
using System.Threading.Tasks;
using Copren.Net.Domain;
using Copren.Net.Domain.Messaging.Messages;
using Copren.Net.Hosting.Context;

namespace Copren.Net.Hosting.Middleware
{
    public interface IMiddleware
    {
        Task OnClientConnected(HostContext context, Client client, Func<Task> next);
        Task OnMessage(HostContext context, Message message, Func<Task> next);
        Task OnClientDisconnected(HostContext context, Client client, Func<Task> next);
    }
}