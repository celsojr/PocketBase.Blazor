using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Extensions;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Realtime
{
    /// <summary>
    /// SSE-based Realtime client that receives live events from PocketBase.
    /// </summary>
    public sealed class RealtimeSseClient // : IRealtimeSseClient
    {
        //private readonly HttpClient _httpClient;
        //private readonly string _baseUrl;
        //private readonly ConcurrentDictionary<string, List<Action<RealtimeEvent>>> _subscriptions = new();
        //private CancellationTokenSource? _cts;
        //private Task? _listeningTask;

        private readonly IHttpTransport _http;
        private readonly ILogger<RealtimeClient>? _logger;
        private readonly TaskCompletionSource<bool> _connectSignal = new();
        private readonly ConcurrentDictionary<string, List<Action<RealtimeEvent>>> _subscriptions = new();
        private readonly Channel<RealtimeEvent> _events = Channel.CreateUnbounded<RealtimeEvent>();

        private Task? _connectionTask;
        private string? _clientId;
        //private bool _connected;
        private volatile bool _isConnected;

        /// <inheritdoc />
        public event Action<IReadOnlyList<string>>? OnDisconnect = _ => { };

        /// <inheritdoc />
        public bool IsConnected => _isConnected;

        /// <inheritdoc />
        public RealtimeSseClient(string baseUrl, IHttpTransport? httpClient = null)
        {
            //_baseUrl = baseUrl.TrimEnd('/');
            //_httpClient = httpClient ?? new HttpClient();
            _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger ??= LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RealtimeClient>();
        }

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

        //private TaskCompletionSource _connectedTcs =
        //    new(TaskCreationOptions.RunContinuationsAsynchronously);

        //private async Task EnsureConnectedAsync(CancellationToken ct)
        //{
        //    if (_isConnected)
        //        return;

        //    if (_sseTask == null)
        //    {
        //        _sseTask = Task.Run(() => RunSseLoopAsync(ct), ct);
        //    }

        //    using (ct.Register(() => _connectedTcs.TrySetCanceled()))
        //    {
        //        await _connectedTcs.Task;
        //    }
        //}

        private async IAsyncEnumerable<RealtimeEvent> StreamRawEventsAsync([EnumeratorCancellation] CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        //private async IAsyncEnumerable<RealtimeEvent> StreamRawEventsAsync([EnumeratorCancellation] CancellationToken ct)
        //{
        //    try
        //    {
        //        await foreach (var line in _http.SendForSseAsync(HttpMethod.Get, "api/realtime", cancellationToken: ct))
        //        {
        //            if (string.IsNullOrWhiteSpace(line))
        //                continue;

        //            if (line.StartsWith("event:", StringComparison.Ordinal))
        //            {
        //                _currentEvent = line[6..].Trim();
        //            }
        //            else if (line.StartsWith("data:", StringComparison.Ordinal))
        //            {
        //                HandleEvent(_currentEvent, line[5..].Trim());
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        _isConnected = false;
        //    }
        //    //string? id = null;
        //    //string? evt = null;
        //    //var data = new StringBuilder();

        //    //await foreach (var line in _http.SendForSseAsync(HttpMethod.Get, "api/realtime", cancellationToken: ct))
        //    //{
        //    //    if (string.IsNullOrWhiteSpace(line))
        //    //    {
        //    //        if (evt != null)
        //    //        {
        //    //            yield return new RealtimeEvent
        //    //            {
        //    //                Id = id,
        //    //                Event = evt,
        //    //                Data = data.ToString()
        //    //            };
        //    //        }

        //    //        id = null;
        //    //        evt = null;
        //    //        data.Clear();
        //    //        continue;
        //    //    }

        //    //    if (line.StartsWith("id:")) id = line[3..].Trim();
        //    //    else if (line.StartsWith("event:")) evt = line[6..].Trim();
        //    //    else if (line.StartsWith("data:")) data.AppendLine(line[5..].Trim());
        //    //}
        //}

        private void HandleEvent(string? evt, string data)
        {
            switch (evt)
            {
                case "PB_CONNECT":
                {
                    var json = JsonDocument.Parse(data);
                    _clientId = json.RootElement.GetProperty("clientId").GetString();
                    _isConnected = true;
                    break;
                }

                case "PB_DISCONNECT":
                {
                    _isConnected = false;
                    IReadOnlyList<string> reasons = JsonSerializer.Deserialize<string[]>(data) ?? [];
                    OnDisconnect?.Invoke(reasons);
                    break;
                }

                default:
                {
                    //DispatchRealtimeEvent(evt, data);
                    break;
                }
            }
        }

        private async Task SubscribeInternalAsync(string topic, CommonOptions? options, CancellationToken ct)
        {
            if (_clientId == null)
                throw new InvalidOperationException("Realtime client not connected.");

            var body = new { clientId = _clientId, subscriptions = new[] { topic } };

            await _http.SendAsync(HttpMethod.Post, "api/realtime", body: body, query: options?.ToDictionary(), cancellationToken: ct);
        }
























        ///// <inheritdoc />
        //public void Subscribe(string topic, Action<RealtimeEvent> callback)
        //{
        //    if (!_subscriptions.TryGetValue(topic, out var value))
        //    {
        //        value = new List<Action<RealtimeEvent>>();
        //        _subscriptions[topic] = value;
        //    }

        //    value.Add(callback);
        //}

        ///// <inheritdoc />
        //public void Unsubscribe(string topic)
        //{
        //    _subscriptions.TryRemove(topic, out _);
        //}

        ///// <inheritdoc />
        //public async Task<Result> StartListeningAsync(CancellationToken cancellationToken = default)
        //{
        //    if (IsConnected) return Result.Fail("Already connected");

        //    _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        //    _listeningTask = Task.Run(async () =>
        //    {
        //        try
        //        {
        //            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/realtime");
        //            request.Headers.Add("Accept", "text/event-stream");

        //            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
        //            response.EnsureSuccessStatusCode();

        //            using var stream = await response.Content.ReadAsStreamAsync(_cts.Token);
        //            using var reader = new StreamReader(stream);

        //            IsConnected = true;

        //            while (!_cts.Token.IsCancellationRequested && !reader.EndOfStream)
        //            {
        //                var line = await reader.ReadLineAsync();
        //                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

        //                var json = line["data:".Length..].Trim();
        //                var evt = JsonSerializer.Deserialize<RealtimeEvent>(json);
        //                if (evt != null && !string.IsNullOrEmpty(evt.Topic) && _subscriptions.TryGetValue(evt.Topic, out var callbacks))
        //                {
        //                    foreach (var cb in callbacks)
        //                    {
        //                        try { cb(evt); } catch { /* swallow callback errors */ }
        //                    }
        //                }
        //            }
        //        }
        //        catch
        //        {
        //            // Handle disconnect
        //            IsConnected = false;
        //            OnDisconnect([.. _subscriptions.Keys]);
        //        }
        //    }, _cts.Token);

        //    await Task.CompletedTask;

        //    return Result.Ok();
        //}

        ///// <inheritdoc />
        //public void Stop()
        //{
        //    if (!IsConnected) return;

        //    _cts?.Cancel();
        //    _listeningTask = null;
        //    IsConnected = false;
        //    OnDisconnect([.. _subscriptions.Keys]);
        //}

        ///// <inheritdoc />
        //public void Dispose()
        //{
        //    Stop();
        //    _httpClient.Dispose();
        //}
    }
}

// Usage example:
//  var realtime = new RealtimeSseClient("https://pocketbase-backend.com");
//  realtime.Subscribe("collections:books", evt => Console.WriteLine(evt.Data));
//  await realtime.StartListeningAsync();
//  
//  // Later
//  realtime.Stop();
