using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PocketBase.Blazor.Converters;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Realtime
{
    using static RecordHelper;

    /// <inheritdoc />
    public sealed class RealtimeClient : RealtimeBase, IRealtimeClient
    {
        private readonly ConcurrentDictionary<string, ImmutableList<Action<RealtimeRecordEvent>>> _subscriptions;

        /// <inheritdoc />
        public RealtimeClient(IHttpTransport http, ILogger<RealtimeClient>? logger = null)
            : base(http, logger ?? CreateDefaultLogger<RealtimeClient>())
        {
            _subscriptions = new ConcurrentDictionary<string, ImmutableList<Action<RealtimeRecordEvent>>>();
            _ = StartDispatcher(_cts.Token);
        }

        /// <inheritdoc />
        public async Task<IDisposable> SubscribeAsync(string collection, string recordId, Action<RealtimeRecordEvent> onEvent, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(onEvent);
            var topic = recordId == "*" ? $"{collection}/*" : $"{collection}/{recordId}";

            await EnsureConnectedAsync(cancellationToken);
            await SubscribeInternalAsync(topic, options, cancellationToken);

            _subscriptions.AddOrUpdate(topic,
                _ => ImmutableList.Create(onEvent),
                (_, existing) => existing.Add(onEvent));

            return new Subscription(() => UnsubscribeHandler(topic, onEvent));
        }

        /// <inheritdoc />
        public async Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);
            var predicate = CreateTopicPredicate(collection, recordId);
            var topicsToRemove = _subscriptions.Keys.Where(predicate).ToList();

            foreach (var topic in topicsToRemove) _subscriptions.TryRemove(topic, out _);
            if (topicsToRemove.Count > 0) await UnsubscribeInternalAsync(topicsToRemove, cancellationToken);
        }

        private Task StartDispatcher(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                await foreach (var evt in _eventChannel.Reader.ReadAllAsync(ct))
                {
                    if (!evt.Data.Contains("\"record\":")) continue;

                    var recordEvt = ParseRecordEvent(evt);
                    if (recordEvt == null) continue;

                    var exactTopic = $"{recordEvt.Collection}/{recordEvt.RecordId}";
                    var wildcardTopic = $"{recordEvt.Collection}/*";

                    var exactHandlers = GetHandlers(exactTopic);
                    var wildcardHandlers = GetHandlers(wildcardTopic);

                    foreach (var handler in exactHandlers.Concat(wildcardHandlers).Distinct())
                    {
                        try { handler(recordEvt); }
                        catch (Exception ex) { _logger.LogError(ex, "Error in event handler"); }
                    }
                }
            }, ct);
        }

        private void UnsubscribeHandler(string topic, Action<RealtimeRecordEvent> handler)
        {
            _subscriptions.AddOrUpdate(topic,
                _ => ImmutableList<Action<RealtimeRecordEvent>>.Empty,
                (_, existing) => existing.Remove(handler));

            if (_subscriptions.TryGetValue(topic, out var handlers) && handlers.IsEmpty)
                _subscriptions.TryRemove(topic, out _);
        }

        private ImmutableList<Action<RealtimeRecordEvent>> GetHandlers(string topic) =>
            _subscriptions.TryGetValue(topic, out var handlers) ? handlers : [];

        private static Func<string, bool> CreateTopicPredicate(string collection, string? recordId) => t =>
        {
            if (!t.StartsWith(collection + "/")) return false;
            if (recordId == null) return true;
            var suffix = t[(collection.Length + 1)..];
            return recordId == "*" ? suffix == "*" : suffix == recordId;
        };

        private static ILogger CreateDefaultLogger<T>() =>
            LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<T>();
    }
}
