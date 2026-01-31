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
    public abstract class RealtimeBase : IAsyncDisposable
    {
        protected readonly IHttpTransport _http;
        protected readonly ILogger _logger;
        protected readonly Channel<RealtimeEvent> _eventChannel;
        protected readonly CancellationTokenSource _cts;
        protected Task? _connectionTask;
        protected string? _clientId;
        protected volatile bool _isConnected;

        public event Action<IReadOnlyList<string>>? OnDisconnect;
        public bool IsConnected => _isConnected;

        protected RealtimeBase(IHttpTransport http, ILogger logger)
        {
            _http = http;
            _logger = logger;
            _eventChannel = Channel.CreateUnbounded<RealtimeEvent>();
            _cts = new CancellationTokenSource();
        }

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

        protected async IAsyncEnumerable<RealtimeEvent> StreamRawEventsAsync(
            [EnumeratorCancellation] CancellationToken ct)
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

        protected async Task SubscribeInternalAsync(string topic, CommonOptions? options, CancellationToken ct)
        {
            if (_clientId == null) throw new InvalidOperationException("Realtime not connected.");
            var body = new { clientId = _clientId, subscriptions = new[] { topic } };
            await _http.SendAsync(HttpMethod.Post, "api/realtime", body: body, query: options?.ToDictionary(), ct);
        }

        protected async Task UnsubscribeInternalAsync(IEnumerable<string> topics, CancellationToken ct)
        {
            if (_clientId == null) throw new InvalidOperationException("Realtime not connected.");
            var body = new { clientId = _clientId, unsubscribe = topics.ToArray() };
            await _http.SendAsync(HttpMethod.Post, "api/realtime", body: body, cancellationToken: ct);
        }

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
