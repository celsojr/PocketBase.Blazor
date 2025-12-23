using System;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Clients.Realtime
{
    public interface IRealtimeClient
    {
        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);

        Task<IDisposable> SubscribeAsync(string topic, Action<RealtimeMessage> handler, CancellationToken cancellationToken = default);
    }
}
