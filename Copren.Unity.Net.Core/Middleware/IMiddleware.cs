using System;
using System.Threading.Tasks;
using Copren.Unity.Net.Core.Context;
using Copren.Unity.Net.Domain.Messaging.Messages;

namespace Copren.Unity.Net.Core.Middleware
{
    public interface IMiddleware
    {
        Task OnConnected(ClientContext context, Func<Task> next);
        Task OnMessage(ClientContext context, Message message, Func<Task> next);
        Task OnDisconnected(ClientContext context, Func<Task> next);
    }
}