using System;
using System.Threading.Tasks;
using Copren.Net.Core.Context;
using Copren.Net.Domain.Messaging.Messages;

namespace Copren.Net.Core.Middleware
{
    public interface IMiddleware
    {
        Task OnConnected(ClientContext context, Func<Task> next);
        Task OnMessage(ClientContext context, Message message, Func<Task> next);
        Task OnDisconnected(ClientContext context, Func<Task> next);
    }
}