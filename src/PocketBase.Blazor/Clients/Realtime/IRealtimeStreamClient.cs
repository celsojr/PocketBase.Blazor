using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Realtime
{
    public interface IRealtimeStreamClient : IAsyncDisposable
    {
        IAsyncEnumerable<RealtimeRecordEvent> SubscribeAsync(string collection, string recordId, CommonOptions? options = null, CancellationToken cancellationToken = default);
        Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default);
        event Action<IReadOnlyList<string>> OnDisconnect;
        bool IsConnected { get; }
    }
}
