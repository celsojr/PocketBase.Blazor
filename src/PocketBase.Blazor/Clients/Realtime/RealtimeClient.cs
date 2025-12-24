using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly ConcurrentDictionary<string, List<Action<RealtimeEvent>>> _subscriptions = new();

        private bool _connected;

        /// <inheritdoc />
        public event Action<IReadOnlyList<string>> OnDisconnect = _ => { };

        /// <inheritdoc />
        public RealtimeClient(IHttpTransport http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        /// <inheritdoc />
        public bool IsConnected => _connected;

        /// <inheritdoc />
        public async Task<bool> SubscribeAsync(string topic, Action<RealtimeEvent> callback, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic must be provided.", nameof(topic));
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            _subscriptions.AddOrUpdate(topic,
                _ => new List<Action<RealtimeEvent>> { callback },
                (_, list) =>
                {
                    list.Add(callback);
                    return list;
                });

            var body = new { clientId = topic, subscriptions = new[] { topic } };

            await _http.SendAsync(HttpMethod.Post, "/api/realtime", body, options?.ToDictionary(), cancellationToken);
            _connected = true;
            return true;
        }

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
            await _http.SendAsync(HttpMethod.Post, "/api/realtime", body, null, cancellationToken);
            _connected = false;
            OnDisconnect(Array.Empty<string>());
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
            await _http.SendAsync(HttpMethod.Post, "/api/realtime", body, null, cancellationToken);
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
            await _http.SendAsync(HttpMethod.Post, "/api/realtime", body, null, cancellationToken);
            return true;
        }
    }
}
