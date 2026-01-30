using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Realtime
{
    /// <summary>
    /// Realtime subscription management service.
    /// Mirrors the JS SDK RealtimeService methods.
    /// </summary>
    public interface IRealtimeClient : IAsyncDisposable
    {
        /// <summary>
        /// Subscribe to a realtime event topic.
        /// </summary>
        /// <param name="collection">The collection to subscribe to.</param>
        /// <param name="recordId">The record ID to subscribe to.</param>
        /// <param name="onEvent">The callback to invoke when the event is received.</param>
        /// <param name="options">Optional common options for the subscription.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the subscription.</param>
        Task<IDisposable> SubscribeAsync(string collection, string recordId, Action<RealtimeRecordEvent>? onEvent, CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unsubscribe from a realtime event topic. If <paramref name="recordId"/> is null or empty, unsubscribes all.
        /// </summary>
        /// <param name="collection">The collection to unsubscribe from.</param>
        /// <param name="recordId">The record ID to unsubscribe from.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the unsubscription.</param>
        Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Indicates whether the realtime connection is established.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Hook invoked when the realtime connection is disconnected.
        /// </summary>
        event Action<IReadOnlyList<string>> OnDisconnect;
    }
}
