using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Realtime
{
    /// <summary>
    /// Represents a callback client for connecting to and interacting with PocketBase's Realtime API.
    /// </summary>
    public interface IRealtimeClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the client is currently connected to the Realtime server.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Raised when the client is disconnected from the Realtime server.
        /// </summary>
        event Action<IReadOnlyList<string>> OnDisconnect;

        /// <summary>
        /// Subscribes to realtime events for a specific collection and optional record ID.
        /// </summary>
        /// <param name="collection">The collection name to subscribe to.</param>
        /// <param name="recordId">The optional record ID to subscribe to.</param>
        /// <param name="onEvent">The event handler to be called when an event is received.</param>
        /// <param name="options">The options for the subscription.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that returns an IDisposable that can be used to unsubscribe.</returns>
        Task<IDisposable> SubscribeAsync(string collection, string recordId, Action<RealtimeRecordEvent> onEvent, CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unsubscribes from realtime events for a specific collection and optional record ID.
        /// </summary>
        /// <param name="collection">The collection name to unsubscribe from.</param>
        /// <param name="recordId">The optional record ID to unsubscribe from.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default);
    }
}
