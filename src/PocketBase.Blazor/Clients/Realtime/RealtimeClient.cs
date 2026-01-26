using System;
using System.Collections.Concurrent;
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
        private readonly IHttpTransport _http;
        private readonly ILogger<RealtimeClient>? _logger;
        private readonly TaskCompletionSource<bool> _connectSignal = new();
        private readonly ConcurrentDictionary<string, List<Action<RealtimeEvent>>> _subscriptions = new();

        private Task? _connectionTask;
        //private string? _clientId;
        private readonly Channel<RealtimeEvent> _events = Channel.CreateUnbounded<RealtimeEvent>();

        //private Task? _listenTask;
        private string? _clientId;
        private bool _connected;
        //private string _lastEvent;

        /// <inheritdoc />
        public event Action<IReadOnlyList<string>> OnDisconnect = _ => { };

        /// <inheritdoc />
        public RealtimeClient(IHttpTransport http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger ??= LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RealtimeClient>();
        }

        /// <inheritdoc />
        public bool IsConnected => _connected;

        /// <inheritdoc />
        //public async Task<bool> SubscribeAsync(string topic, Action<RealtimeEvent> callback, CommonOptions? options = null, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(topic))
        //        throw new ArgumentException("Topic is required.", nameof(topic));

        //    var query = options?.ToDictionary() ?? new Dictionary<string, object?>();
        //    query["topic"] = topic;

        //    string? currentId = null;
        //    string? currentEvent = null;
        //    var dataBuilder = new StringBuilder();

        //    try
        //    {
        //        await foreach (var line in _http.SendForSseAsync(HttpMethod.Get, "api/realtime",
        //            query: query, cancellationToken: cancellationToken))
        //        {
        //            if (string.IsNullOrWhiteSpace(line))
        //            {
        //                // End of SSE frame
        //                if (currentEvent != null)
        //                {
        //                    callback(new RealtimeEvent
        //                    {
        //                        Id = currentId,
        //                        Event = currentEvent,
        //                        Data = dataBuilder.ToString()
        //                    });
        //                }

        //                currentId = null;
        //                currentEvent = null;
        //                dataBuilder.Clear();
        //                continue;
        //            }

        //            if (line.StartsWith("id:"))
        //            {
        //                currentId = line[3..].Trim();
        //            }
        //            else if (line.StartsWith("event:"))
        //            {
        //                currentEvent = line[6..].Trim();
        //            }
        //            else if (line.StartsWith("data:"))
        //            {
        //                dataBuilder.AppendLine(line[5..].Trim());
        //            }
        //        }

        //        return true;
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        // Normal shutdown
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}


        #region IAsyncEnumerable

        public async IAsyncEnumerable<RealtimeRecordEvent> SubscribeAsync(string collection, string recordId, CommonOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var topic = $"{collection}/{recordId}";

            await EnsureConnectedAsync(cancellationToken);
            await SubscribeInternalAsync(topic, options, cancellationToken);

            await foreach (var evt in _events.Reader.ReadAllAsync(cancellationToken))
            {
                if (evt.Event != "record")
                    continue;

                var json = JsonDocument.Parse(evt.Data);
                yield return new RealtimeRecordEvent
                {
                    Action = json.RootElement.GetProperty("action").GetString()!,
                    Collection = json.RootElement.GetProperty("collection").GetString()!,
                    RecordId = json.RootElement.GetProperty("record").GetProperty("id").GetString(),
                    Record = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                        json.RootElement.GetProperty("record").GetRawText())!
                };
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

                    await _events.Writer.WriteAsync(evt, ct);
                }
            }, ct);
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

        private async Task SubscribeInternalAsync(string topic, CommonOptions? options, CancellationToken ct)
        {
            if (_clientId == null)
                throw new InvalidOperationException("Realtime client not connected.");

            var body = new { clientId = _clientId, subscriptions = new[] { topic } };

            await _http.SendAsync(HttpMethod.Post, "api/realtime", body: body, query: options?.ToDictionary(), cancellationToken: ct);
        }

        #endregion

        #region Callback

        private readonly Channel<RealtimeEvent> _eventChannel = Channel.CreateUnbounded<RealtimeEvent>();

        private readonly Dictionary<string, List<Action<RealtimeRecordEvent>>> _subscriptions2 = new();

        private readonly object _sync = new();

        public async Task<IDisposable> SubscribeAsync(string collection, string recordId, Action<RealtimeRecordEvent> onEvent, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            var topic = $"{collection}/{recordId}";
            var key = topic;

            await EnsureConnectedAsync2(cancellationToken);
            StartDispatcher(cancellationToken);
            await SubscribeInternalAsync2(topic, options, cancellationToken);

            lock (_sync)
            {
                if (!_subscriptions2.TryGetValue(key, out var handlers))
                {
                    handlers = new List<Action<RealtimeRecordEvent>>();
                    _subscriptions2[key] = handlers;
                }

                handlers.Add(onEvent);
            }

            return new Subscription(() =>
            {
                lock (_sync)
                {
                    if (_subscriptions2.TryGetValue(key, out var handlers))
                    {
                        handlers.Remove(onEvent);
                        if (handlers.Count == 0)
                            _subscriptions2.Remove(key);
                    }
                }
            });
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
                        if (!_subscriptions2.TryGetValue(topicKey, out handlers))
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


        private async Task SubscribeInternalAsync2(string topic, CommonOptions? options, CancellationToken ct)
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

        private async IAsyncEnumerable<RealtimeEvent> StreamRawEventsAsync2([EnumeratorCancellation] CancellationToken ct)
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

        private async Task EnsureConnectedAsync2(CancellationToken ct)
        {
            if (_connectionTask != null)
                return;

            _connectionTask = Task.Run(async () =>
            {
                await foreach (var evt in StreamRawEventsAsync2(ct))
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
        }

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



        #endregion














































        /// <inheritdoc />
        public async Task<bool> UnsubscribeAsync(string? topic = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                _subscriptions.Clear();
            }
            else
            {
                _subscriptions.TryRemove(topic, out _);
            }

            var body = new { clientId = topic ?? string.Empty, subscriptions = Array.Empty<string>() };
            await _http.SendAsync(HttpMethod.Post, "api/realtime", body, null, cancellationToken);
            _connected = false;
            OnDisconnect([]);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> UnsubscribeByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            foreach (var key in _subscriptions.Keys)
            {
                if (key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    _subscriptions.TryRemove(key, out _);
                }
            }

            var body = new { clientId = prefix, subscriptions = Array.Empty<string>() };
            await _http.SendAsync(HttpMethod.Post, "api/realtime", body, null, cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> UnsubscribeByTopicAndListenerAsync(string topic, Action<RealtimeEvent> callback, CancellationToken cancellationToken = default)
        {
            if (_subscriptions.TryGetValue(topic, out var list))
            {
                list.Remove(callback);
                if (list.Count == 0) _subscriptions.TryRemove(topic, out _);
            }

            var body = new { clientId = topic, subscriptions = _subscriptions.Keys };
            await _http.SendAsync(HttpMethod.Post, "api/realtime", body, null, cancellationToken);
            return true;
        }
    }

}
