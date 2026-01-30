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
    /// <inheritdoc />
    public sealed class RealtimeClient : IRealtimeClient
    {
        private readonly object _sync = new();
        private readonly IHttpTransport _http;
        private readonly ILogger<RealtimeClient>? _logger;
        private readonly CancellationTokenSource _cts = new();
        private readonly Dictionary<string, List<Action<RealtimeRecordEvent>>> _subscriptions = new();
        private readonly Channel<RealtimeEvent> _eventChannel = Channel.CreateUnbounded<RealtimeEvent>();

        private Task? _connectionTask;
        private volatile bool _isConnected;
        private string? _clientId;

        /// <inheritdoc />
        public event Action<IReadOnlyList<string>>? OnDisconnect = _ => { };

        /// <inheritdoc />
        public bool IsConnected => _isConnected;

        /// <inheritdoc />
        internal RealtimeClient(IHttpTransport http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger ??= LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RealtimeClient>();
        }

        /// <inheritdoc />
        public async Task<IDisposable> SubscribeAsync(string collection, string recordId, Action<RealtimeRecordEvent>? onEvent, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            var topic = $"{collection}/{recordId}";
            var key = topic;

            await EnsureConnectedAsync(cancellationToken);
            StartDispatcher(key, cancellationToken);
            await SubscribeInternalAsync(topic, options, cancellationToken);

            lock (_sync)
            {
                if (!_subscriptions.TryGetValue(key, out var handlers))
                {
                    handlers = new List<Action<RealtimeRecordEvent>>();
                    _subscriptions[key] = handlers;
                }

                handlers.Add(onEvent!);
            }

            return new Subscription(() =>
            {
                lock (_sync)
                {
                    if (_subscriptions.TryGetValue(key, out var handlers))
                    {
                        handlers.Remove(onEvent!);
                        if (handlers.Count == 0)
                            _subscriptions.Remove(key);
                    }
                }
            });
        }

        /// <inheritdoc />
        public async Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);

            List<string> topicsToRemove;

            lock (_sync)
            {
                topicsToRemove = _subscriptions.Keys
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

                foreach (var topic in topicsToRemove)
                    _subscriptions.Remove(topic);
            }

            if (topicsToRemove.Count == 0)
                return;

            await UnsubscribeInternalAsync(topicsToRemove, cancellationToken);
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _dispose;
            private bool _disposed;

            public Subscription(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _dispose();
            }
        }

        private void StartDispatcher(string topic, CancellationToken ct)
        {
            Task.Run(async () =>
            {
                await foreach (var evt in _eventChannel.Reader.ReadAllAsync(ct))
                {
                    if (!evt.Event.Contains("\"record\":"))
                        continue;

                    var recordEvt = ParseRecordEvent(evt);

                    List<string> topics =
                    [
                        topic,
                        $"{recordEvt?.Collection}/{recordEvt?.RecordId ?? "*"}",
                    ];

                    List<Action<RealtimeRecordEvent>>? handlers = [];

                    lock (_sync)
                    {
                        foreach (var key in topics)
                        {
                            if (_subscriptions.TryGetValue(key, out handlers))
                            {
                                handlers = [.. handlers]; // snapshot
                            }

                        }

                    }

                    if (handlers != null)
                        foreach (var h in handlers)
                            h(recordEvt!);
                }
            }, ct);
        }

        private RealtimeRecordEvent? ParseRecordEvent(RealtimeEvent evt)
        {
            var json = JsonDocument.Parse(evt.Data);
            var root = json.RootElement;
            var @event = default(RealtimeRecordEvent?);

            try
            {
                @event = new RealtimeRecordEvent
                {
                    // ValueKind = Object : "{"record":{"collectionId":"pbc_3292755704","collectionName":"categories","created":"2026-01-30 18:58:15.571Z","id":"xmngfsfxo6rw8sa","name":"Test Category c16d9307c57642a","slug":"test-category-c16d9307c57642a","updated":"2026-01-30 18:58:15.571Z"},"action":"create"}"
                    Action = root.GetProperty("action").GetString()!,
                    Collection = root.GetProperty("record").GetProperty("collectionName").GetString()!,
                    RecordId = root.GetProperty("record").GetProperty("id").GetString(),
                    Record = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                        root.GetProperty("record").GetRawText())!
                };
            }
            catch (System.Exception ex)
            {
                // Log or handle parsing error
                _logger?.LogError(ex, "Failed to parse RealtimeEvent data: {Data}", evt.Data);
            }

            return @event;
        }

        private async Task SubscribeInternalAsync(string topic, CommonOptions? options, CancellationToken ct)
        {
            if (_clientId == null)
                throw new InvalidOperationException("Realtime not connected.");

            var body = new
            {
                clientId = _clientId,
                subscriptions = new[] { topic }
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

            // Wait until PB_CONNECT is received
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
                unsubscribe = topics.ToArray()
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
                    // expected
                }
            }

            _eventChannel.Writer.TryComplete();

            lock (_sync)
            {
                _subscriptions.Clear();
            }

            _isConnected = false;
            OnDisconnect?.Invoke([]);
        }

        //var col = pb.Collection("example");

        //// subscribe
        //await col.SubscribeAsync("*", e => Console.WriteLine(e.Action));
        //await col.SubscribeAsync("RECORD_ID", e => Console.WriteLine(e.Record));

        //// unsubscribe
        //await col.UnsubscribeAsync("RECORD_ID");
        //await col.UnsubscribeAsync("*");
        //await col.UnsubscribeAsync(); // whole collection

        // Usage example:
        //using var sub = await pb
        //    .Collection("example")
        //    .SubscribeAsync(
        //        "*",
        //        e =>
        //        {
        //            Console.WriteLine(e.Action);
        //            Console.WriteLine(e.Record["title"]);
        //        },
        //        cancellationToken: cts.Token);
    }

}
