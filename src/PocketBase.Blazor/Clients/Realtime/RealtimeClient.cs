using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
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
    /// <inheritdoc />
    public sealed class RealtimeClient : IRealtimeClient
    {
        private readonly IHttpTransport _http;
        private readonly ILogger<RealtimeClient>? _logger;
        private readonly CancellationTokenSource _cts = new();
        private readonly Channel<RealtimeEvent> _eventChannel = Channel.CreateUnbounded<RealtimeEvent>();
        
        // Using ImmutableList for thread-safe operations without locks
        private readonly ConcurrentDictionary<string, ImmutableList<Action<RealtimeRecordEvent>>> _subscriptions = new();
        
        private Task? _connectionTask;
        private Task? _dispatcherTask;
        private volatile bool _isConnected;
        private string? _clientId;

        /// <inheritdoc />
        public event Action<IReadOnlyList<string>>? OnDisconnect;

        /// <inheritdoc />
        public bool IsConnected => _isConnected;

        /// <inheritdoc />
        internal RealtimeClient(IHttpTransport http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger ??= LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RealtimeClient>();
        }

        /// <inheritdoc />
        public async Task<IDisposable> SubscribeAsync(string collection, string recordId, Action<RealtimeRecordEvent> onEvent, 
            CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(onEvent);

            var topic = $"{collection}/{recordId}";

            await EnsureConnectedAsync(cancellationToken);
            await SubscribeInternalAsync(topic, options, cancellationToken);

            // Atomic update - no locks needed
            _subscriptions.AddOrUpdate(
                topic,
                _ => ImmutableList.Create(onEvent),
                (_, existing) => existing.Add(onEvent)
            );

            // Ensure dispatcher is running
            _dispatcherTask ??= StartDispatcher(cancellationToken);

            return new Subscription(() =>
            {
                _subscriptions.AddOrUpdate(
                    topic,
                    _ => ImmutableList<Action<RealtimeRecordEvent>>.Empty,
                    (_, existing) => existing.Remove(onEvent)
                );
                
                // Clean up empty lists
                if (_subscriptions.TryGetValue(topic, out var handlers) && handlers.IsEmpty)
                {
                    _subscriptions.TryRemove(topic, out _);
                }
            });
        }

        /// <inheritdoc />
        public async Task UnsubscribeAsync(string collection, string? recordId = null, 
            CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);

            var topicsToRemove = _subscriptions.Keys
                .Where(t =>
                {
                    if (!t.StartsWith(collection + "/", StringComparison.Ordinal))
                        return false;

                    if (recordId == null)
                        return true;

                    var suffix = t[(collection.Length + 1)..];
                    return recordId == "*" ? suffix == "*" : suffix == recordId;
                })
                .ToList();

            if (topicsToRemove.Count == 0)
                return;

            foreach (var topic in topicsToRemove)
                _subscriptions.TryRemove(topic, out _);

            await UnsubscribeInternalAsync(topicsToRemove, cancellationToken);
        }

        private Task StartDispatcher(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                await foreach (var evt in _eventChannel.Reader.ReadAllAsync(ct))
                {
                    if (!evt.Data.Contains("\"record\":"))
                        continue;

                    var recordEvt = ParseRecordEvent(evt);
                    if (recordEvt == null)
                        continue;

                    // Try exact match first (e.g., "categories/abc123")
                    var exactTopic = $"{recordEvt.Collection}/{recordEvt.RecordId}";
                    
                    // Try wildcard match (e.g., "categories/*")
                    var wildcardTopic = $"{recordEvt.Collection}/*";

                    // Get handlers for both topics
                    var exactHandlers = GetHandlersForTopic(exactTopic);
                    var wildcardHandlers = GetHandlersForTopic(wildcardTopic);

                    // Combine and deduplicate handlers (in case someone subscribed to both)
                    var allHandlers = exactHandlers.Concat(wildcardHandlers).Distinct();

                    // Execute all handlers
                    foreach (var handler in allHandlers)
                    {
                        try
                        {
                            handler(recordEvt);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error executing realtime event handler");
                        }
                    }
                }
            }, ct);
        }

        private ImmutableList<Action<RealtimeRecordEvent>> GetHandlersForTopic(string topic)
        {
            return _subscriptions.TryGetValue(topic, out var handlers) ? handlers : [];
        }

        private RealtimeRecordEvent? ParseRecordEvent(RealtimeEvent evt)
        {
            try
            {
                var json = JsonDocument.Parse(evt.Data);
                var root = json.RootElement;

                return new RealtimeRecordEvent
                {
                    Action = root.GetProperty("action").GetString()!,
                    Collection = root.GetProperty("record").GetProperty("collectionName").GetString()!,
                    RecordId = root.GetProperty("record").GetProperty("id").GetString()!,
                    Record = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                        root.GetProperty("record").GetRawText())!
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to parse RealtimeEvent data: {Data}", evt.Data);
                return null;
            }
        }

        private async Task SubscribeInternalAsync(string topic, CommonOptions? options, CancellationToken ct)
        {
            if (_clientId == null)
                throw new InvalidOperationException("Realtime not connected.");

            var body = new
            {
                clientId = _clientId,
                subscriptions = (object[])[topic]
            };

            await _http.SendAsync(HttpMethod.Post, "api/realtime", body: body, query: options?.ToDictionary(), cancellationToken: ct);
        }

        private async IAsyncEnumerable<RealtimeEvent> StreamRawEventsAsync([EnumeratorCancellation] CancellationToken ct)
        {
            string? id = null;
            string? evt = null;
            var data = new StringBuilder();

            await foreach (var line in _http.SendForSseAsync(HttpMethod.Get, "api/realtime", cancellationToken: ct))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (evt != null)
                    {
                        yield return new RealtimeEvent
                        {
                            Id = id,
                            Event = evt,
                            Data = data.ToString()
                        };
                    }

                    id = null;
                    evt = null;
                    data.Clear();
                    continue;
                }

                if (line.StartsWith("id:")) id = line[3..].Trim();
                else if (line.StartsWith("event:")) evt = line[6..].Trim();
                else if (line.StartsWith("data:")) data.AppendLine(line[5..].Trim());
            }
        }

        private async Task EnsureConnectedAsync(CancellationToken ct)
        {
            if (_connectionTask != null)
                return;

            _connectionTask = Task.Run(async () =>
            {
                await foreach (var evt in StreamRawEventsAsync(ct))
                {
                    if (evt.Event == "PB_CONNECT")
                    {
                        var json = JsonDocument.Parse(evt.Data);
                        _clientId = json.RootElement.GetProperty("clientId").GetString();
                        continue;
                    }

                    await _eventChannel.Writer.WriteAsync(evt, ct);
                }
            }, ct);

            // Wait for initial connection
            while (_clientId == null)
                await Task.Delay(10, ct);

            _isConnected = true;
        }

        private async Task UnsubscribeInternalAsync(IEnumerable<string> topics, CancellationToken ct)
        {
            if (_clientId == null)
                throw new InvalidOperationException("Realtime not connected.");

            var body = new
            {
                clientId = _clientId,
                unsubscribe = (string[])[.. topics]
            };

            await _http.SendAsync(HttpMethod.Post, "api/realtime", body: body, cancellationToken: ct);

            _isConnected = false;
            OnDisconnect?.Invoke([.. topics]);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();

            if (_connectionTask is not null)
            {
                try
                {
                    await _connectionTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelled
                }
            }

            _eventChannel.Writer.TryComplete();
            _subscriptions.Clear();
            _isConnected = false;
            OnDisconnect?.Invoke(ImmutableList<string>.Empty);
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _unsubscribe;
            private bool _disposed;

            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _unsubscribe();
            }
        }
    }
}
