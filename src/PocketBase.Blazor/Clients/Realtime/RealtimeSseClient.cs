using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;

namespace PocketBase.Blazor.Clients.Realtime
{
    /// <summary>
    /// SSE-based Realtime client that receives live events from PocketBase.
    /// </summary>
    public sealed class RealtimeSseClient : IRealtimeSseClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ConcurrentDictionary<string, List<Action<RealtimeEvent>>> _subscriptions = new();
        private CancellationTokenSource? _cts;
        private Task? _listeningTask;

        /// <inheritdoc />
        public bool IsConnected { get; private set; }

        /// <inheritdoc />
        public event Action<IReadOnlyList<string>> OnDisconnect = _ => { };

        /// <inheritdoc />
        public RealtimeSseClient(string baseUrl, HttpClient? httpClient = null)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <inheritdoc />
        public void Subscribe(string topic, Action<RealtimeEvent> callback)
        {
            if (!_subscriptions.TryGetValue(topic, out var value))
            {
                value = new List<Action<RealtimeEvent>>();
                _subscriptions[topic] = value;
            }

            value.Add(callback);
        }

        /// <inheritdoc />
        public void Unsubscribe(string topic)
        {
            _subscriptions.TryRemove(topic, out _);
        }

        /// <inheritdoc />
        public async Task StartListeningAsync(CancellationToken cancellationToken = default)
        {
            if (IsConnected) return;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _listeningTask = Task.Run(async () =>
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/realtime");
                    request.Headers.Add("Accept", "text/event-stream");

                    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                    response.EnsureSuccessStatusCode();

                    using var stream = await response.Content.ReadAsStreamAsync(_cts.Token);
                    using var reader = new StreamReader(stream);

                    IsConnected = true;

                    while (!_cts.Token.IsCancellationRequested && !reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

                        var json = line["data:".Length..].Trim();
                        var evt = JsonSerializer.Deserialize<RealtimeEvent>(json);
                        if (evt != null && !string.IsNullOrEmpty(evt.Topic) && _subscriptions.TryGetValue(evt.Topic, out var callbacks))
                        {
                            foreach (var cb in callbacks)
                            {
                                try { cb(evt); } catch { /* swallow callback errors */ }
                            }
                        }
                    }
                }
                catch
                {
                    // Handle disconnect
                    IsConnected = false;
                    OnDisconnect([.. _subscriptions.Keys]);
                }
            }, _cts.Token);

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (!IsConnected) return;

            _cts?.Cancel();
            _listeningTask = null;
            IsConnected = false;
            OnDisconnect([.. _subscriptions.Keys]);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Stop();
            _httpClient.Dispose();
        }
    }
}

// Usage example:
//  var realtime = new RealtimeSseClient("https://pocketbase-backend.com");
//  realtime.Subscribe("collections:books", evt => Console.WriteLine(evt.Data));
//  await realtime.StartListeningAsync();
//  
//  // Later
//  realtime.Stop();

