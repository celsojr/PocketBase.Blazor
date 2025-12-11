using System;
using System.Threading.Tasks;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Clients.Realtime
{
    public interface IRealtimeClient
    {
        Task ConnectAsync();
        Task DisconnectAsync();

        Task<IDisposable> SubscribeAsync(string topic, Action<RealtimeMessage> handler);
    }
}
