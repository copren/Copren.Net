using System;
using System.Threading.Tasks;
using Copren.Unity.Net.Domain;
using Copren.Unity.Net.Domain.Messaging.Messages;
using Copren.Unity.Net.Hosting.Context;

namespace Copren.Unity.Net.Hosting.Middleware
{
    public interface IMiddleware
    {
        Task OnClientConnected(HostContext context, Client client, Func<Task> next);
        Task OnMessage(HostContext context, Message message, Func<Task> next);
        Task OnClientDisconnected(HostContext context, Client client, Func<Task> next);
    }
}