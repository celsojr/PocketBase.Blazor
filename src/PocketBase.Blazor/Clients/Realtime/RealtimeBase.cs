using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Extensions;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Realtime
{
    /// <summary>
    /// Base class for implementing PocketBase Realtime clients.
    /// </summary>
    public abstract class RealtimeBase : IAsyncDisposable
    {
        /// <summary>
        /// The HTTP transport used to communicate with the PocketBase server.
        /// </summary>
        protected readonly IHttpTransport _http;

        /// <summary>
        /// The logger used for logging messages.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// The channel used to receive real-time events.
        /// </summary>
        protected readonly Channel<RealtimeEvent> _eventChannel;

        /// <summary>
        /// The cancellation token source used to cancel the connection task.
        /// </summary>
        protected readonly CancellationTokenSource _cts;

        /// <summary>
        /// The connection task that is running to maintain the connection to the Realtime server.
        /// </summary>
        protected Task? _connectionTask;

        /// <summary>
        /// The client ID assigned by the PocketBase server.
        /// </summary>
        protected string? _clientId;

        /// <summary>
        /// Whether the client is currently connected to the PocketBase server.
        /// </summary>
        protected volatile bool _isConnected;

        /// <summary>
        /// Event that is raised when the client is disconnected from the PocketBase server.
        /// </summary>
        public event Action<IReadOnlyList<string>>? OnDisconnect;

        /// <summary>
        /// Gets a value indicating whether the client is currently connected to the PocketBase server.
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealtimeBase"/> class.
        /// </summary>
        /// <param name="http">The HTTP transport used to communicate with the PocketBase server.</param>
        /// <param name="logger">The logger used for logging messages.</param>
        protected RealtimeBase(IHttpTransport http, ILogger logger)
        {
            _http = http;
            _logger = logger;
            _eventChannel = Channel.CreateUnbounded<RealtimeEvent>();
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Ensures that the client is connected to the PocketBase server.
        /// </summary>
        /// <param name="ct">The cancellation token to use for the operation.</param>
        protected async Task EnsureConnectedAsync(CancellationToken ct)
        {
            if (_connectionTask != null) return;

            _connectionTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var evt in StreamRawEventsAsync(ct))
                    {
                        if (evt.Event == "PB_CONNECT")
                        {
                            var json = JsonDocument.Parse(evt.Data);
                            _clientId = json.RootElement.GetProperty("clientId").GetString();
                            _isConnected = true;
                        }
                        else if (evt.Event == "PB_DISCONNECT")
                        {
                            _isConnected = false;
                            var reasons = JsonSerializer.Deserialize<string[]>(evt.Data) ?? [];
                            OnDisconnect?.Invoke(reasons);
                        }
                        else
                        {
                            await _eventChannel.Writer.WriteAsync(evt, ct);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelled
                }
                finally
                {
                    _isConnected = false;
                }
            }, ct);

            while (_clientId == null && !ct.IsCancellationRequested)
                await Task.Delay(10, ct);
        }

        /// <summary>
        /// Streams raw events from the PocketBase server.
        /// </summary>
        /// <param name="ct">The cancellation token to use for the operation.</param>
        /// <returns>An async enumerable of <see cref="RealtimeEvent"/> objects.</returns>
        protected async IAsyncEnumerable<RealtimeEvent> StreamRawEventsAsync([EnumeratorCancellation] CancellationToken ct)
        {
            string? id = null, evtType = null;
            var data = new StringBuilder();

            await foreach (var line in _http.SendForSseAsync(HttpMethod.Get, "api/realtime", cancellationToken: ct))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (evtType != null) yield return new RealtimeEvent { Id = id, Event = evtType, Data = data.ToString() };
                    id = null; evtType = null; data.Clear();
                    continue;
                }

                if (line.StartsWith("id:")) id = line[3..].Trim();
                else if (line.StartsWith("event:")) evtType = line[6..].Trim();
                else if (line.StartsWith("data:")) data.AppendLine(line[5..].Trim());
            }
        }

        /// <summary>
        /// Subscribes to a topic in the PocketBase server.
        /// </summary>
        /// <param name="topic">The topic to subscribe to.</param>
        /// <param name="options">The options for the subscription.</param>
        /// <param name="ct">The cancellation token to use for the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async Task SubscribeInternalAsync(string topic, CommonOptions? options, CancellationToken ct)
        {
            if (_clientId == null) throw new InvalidOperationException("Realtime not connected.");
            var body = new { clientId = _clientId, subscriptions = new[] { topic } };
            await _http.SendAsync(HttpMethod.Post, "api/realtime", body: body, query: options?.ToDictionary(), ct);
        }

        /// <summary>
        /// Unsubscribes from topics in the PocketBase server.
        /// </summary>
        /// <param name="topics">The topics to unsubscribe from.</param>
        /// <param name="ct">The cancellation token to use for the operation.</param>
        protected async Task UnsubscribeInternalAsync(IEnumerable<string> topics, CancellationToken ct)
        {
            if (_clientId == null) throw new InvalidOperationException("Realtime not connected.");
            var body = new { clientId = _clientId, unsubscribe = topics.ToArray() };
            await _http.SendAsync(HttpMethod.Post, "api/realtime", body: body, cancellationToken: ct);
        }

        /// <summary>
        /// Disposes the Realtime client asynchronously.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            //if (_connectionTask != null) await _connectionTask;
            _eventChannel.Writer.TryComplete();
            _isConnected = false;
            OnDisconnect?.Invoke([]);
        }
    }
}
