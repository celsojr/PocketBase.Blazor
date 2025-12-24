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
    public interface IRealtimeClient
    {
        /// <summary>
        /// Subscribe to a realtime event topic.
        /// </summary>
        /// <param name="topic">The topic to subscribe to.</param>
        /// <param name="callback">The callback to invoke when the event is received.</param>
        /// <param name="options">Optional common options for the subscription.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the subscription.</param>
        Task<bool> SubscribeAsync(string topic, Action<RealtimeEvent> callback, CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unsubscribe from a realtime event topic. If <paramref name="topic"/> is null or empty, unsubscribes all.
        /// </summary>
        /// <param name="topic">The topic to unsubscribe from. If null or empty, unsubscribes all topics.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the unsubscription.</param>
        Task<bool> UnsubscribeAsync(string? topic = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unsubscribe from all subscription listeners whose topic begins with the specified prefix.
        /// </summary>
        /// <param name="prefix">The topic prefix to unsubscribe from.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the unsubscription.</param>
        Task<bool> UnsubscribeByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unsubscribe the specified subscription listener for a given topic and callback identity.
        /// </summary>
        /// <param name="topic">The topic to unsubscribe from.</param>
        /// <param name="callback">The callback to unsubscribe.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the unsubscription.</param>
        Task<bool> UnsubscribeByTopicAndListenerAsync(string topic, Action<RealtimeEvent> callback, CancellationToken cancellationToken = default);

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
