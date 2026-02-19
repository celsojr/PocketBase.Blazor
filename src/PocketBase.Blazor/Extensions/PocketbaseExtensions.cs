using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Clients.Record;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Extensions
{
    public static class PocketbaseExtensions
    {
        public static async Task<IDisposable> SubscribeAsync(this IRecordClient client, string recordId,
            Action<RealtimeRecordEvent>? onEvent, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return await client.Realtime.SubscribeAsync(client.CollectionName, recordId, onEvent, options, cancellationToken);
        }
    
        public static IAsyncEnumerable<RealtimeRecordEvent> SubscribeAsync(this IRecordClient client,
            string recordId, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(client);
            return client.RealtimeSse.SubscribeAsync(client.CollectionName, recordId, options, cancellationToken);
        }
    }
}
