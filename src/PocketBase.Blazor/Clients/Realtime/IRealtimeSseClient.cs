using System;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;

namespace PocketBase.Blazor.Clients.Realtime
{
    /// <summary>
    /// Interface for a standalone SSE-based Realtime client.
    /// </summary>
    public interface IRealtimeSseClient : IDisposable
    {
        /// <summary>
        /// Indicates whether the SSE connection is active.
        /// </summary>
        bool IsConnected { get; }

        ///// <summary>
        ///// Event fired when the SSE connection is disconnected.
        ///// </summary>
        //event Action OnDisconnect;

        /// <summary>
        /// Subscribe to a topic.
        /// </summary>
        /// <param name="topic">The topic to subscribe to.</param>
        /// <param name="callback">The callback to invoke when an event is received.</param>
        void Subscribe(string topic, Action<RealtimeEvent> callback);

        /// <summary>
        /// Unsubscribe from a topic.
        /// </summary>
        /// <param name="topic">The topic to unsubscribe from.</param>
        void Unsubscribe(string topic);

        /// <summary>
        /// Starts listening for SSE events.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        Task StartListeningAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops listening for SSE events.
        /// </summary>
        void Stop();
    }
}
