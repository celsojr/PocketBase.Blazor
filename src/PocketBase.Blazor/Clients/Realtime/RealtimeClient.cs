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
    public class RealtimeClient : IRealtimeClient
    {
        private readonly object _sync = new();
        private readonly IHttpTransport _http;
        private readonly ILogger<RealtimeClient>? _logger;
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
        public RealtimeClient(IHttpTransport http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger ??= LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RealtimeClient>();
        }

        /// <inheritdoc />
        public async Task<IDisposable> SubscribeAsync(string collection, string recordId, Action<RealtimeRecordEvent> onEvent, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            var topic = $"{collection}/{recordId}";
            var key = topic;

            await EnsureConnectedAsync(cancellationToken);
            StartDispatcher(cancellationToken);
            await SubscribeInternalAsync(topic, options, cancellationToken);

            lock (_sync)
            {
                if (!_subscriptions.TryGetValue(key, out var handlers))
                {
                    handlers = new List<Action<RealtimeRecordEvent>>();
                    _subscriptions[key] = handlers;
                }

                handlers.Add(onEvent);
            }

            return new Subscription(() =>
            {
                lock (_sync)
                {
                    if (_subscriptions.TryGetValue(key, out var handlers))
                    {
                        handlers.Remove(onEvent);
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

        private void StartDispatcher(CancellationToken ct)
        {
            Task.Run(async () =>
            {
                await foreach (var evt in _eventChannel.Reader.ReadAllAsync(ct))
                {
                    if (evt.Event != "record")
                        continue;

                    var recordEvt = ParseRecordEvent(evt);

                    var topicKey = $"{recordEvt.Collection}/{recordEvt.RecordId ?? "*"}";

                    List<Action<RealtimeRecordEvent>> handlers;

                    lock (_sync)
                    {
                        if (!_subscriptions.TryGetValue(topicKey, out handlers))
                            continue;

                        handlers = [.. handlers]; // snapshot
                    }

                    foreach (var h in handlers)
                        h(recordEvt);
                }
            }, ct);
        }

        private static RealtimeRecordEvent ParseRecordEvent(RealtimeEvent evt)
        {
            var json = JsonDocument.Parse(evt.Data);
            var root = json.RootElement;

            return new RealtimeRecordEvent
            {
                Action = root.GetProperty("action").GetString()!,
                Collection = root.GetProperty("collection").GetString()!,
                RecordId = root.GetProperty("record").GetProperty("id").GetString(),
                Record = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    root.GetProperty("record").GetRawText())!
            };
        }

        private async Task SubscribeInternalAsync(string topic, CommonOptions? options, CancellationToken ct)
        {
            if (_clientId == null)
                throw new InvalidOperationException("Realtime not connected.");

            await _http.SendAsync(
                HttpMethod.Post,
                "api/realtime",
                body: new
                {
                    clientId = _clientId,
                    topic
                },
                query: options?.ToDictionary(),
                cancellationToken: ct);
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
