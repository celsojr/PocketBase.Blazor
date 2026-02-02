using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Realtime
{
    /// <summary>
    /// Represents a SSE client for handling real-time streaming events.
    /// </summary>
    public interface IRealtimeStreamClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IRealtimeStreamClient"/> is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Occurs when the client is disconnected.
        /// </summary>
        event Action<IReadOnlyList<string>> OnDisconnect;

        /// <summary>
        /// Subscribes to real-time events for a specific collection and record.
        /// </summary>
        /// <param name="collection">The collection name to subscribe to.</param>
        /// <param name="recordId">The record ID to subscribe to.</param>
        /// <param name="options">The options for the subscription.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async enumerable of real-time events.</returns>
        IAsyncEnumerable<RealtimeRecordEvent> SubscribeAsync(string collection, string recordId, CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unsubscribes from real-time events for a specific collection and optional record.
        /// </summary>
        /// <param name="collection">The collection name to unsubscribe from.</param>
        /// <param name="recordId">The record ID to unsubscribe from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default);
    }
}
