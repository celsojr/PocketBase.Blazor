using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Converters;

namespace PocketBase.Blazor.Clients.Realtime
{
    using static RecordHelper;

    /// <inheritdoc />
    public sealed class RealtimeSseClient : RealtimeBase, IRealtimeStreamClient
    {
        /// <inheritdoc />
        public RealtimeSseClient(IHttpTransport http, ILogger<RealtimeSseClient>? logger = null)
            : base(http, logger ?? CreateDefaultLogger<RealtimeSseClient>()) { }

        /// <inheritdoc />
        public async IAsyncEnumerable<RealtimeRecordEvent> SubscribeAsync(string collection, string recordId, CommonOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var topic = recordId == "*" ? $"{collection}/*" : $"{collection}/{recordId}";
            await EnsureConnectedAsync(cancellationToken);
            await SubscribeInternalAsync(topic, options, cancellationToken);

            try
            {
                await foreach (var evt in _eventChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    if (!evt.Data.Contains("\"record\":")) continue;
                    var recordEvt = ParseRecordEvent(evt);
                    if (recordEvt != null) yield return recordEvt;
                }
            }
            finally
            {
                await UnsubscribeInternalAsync([topic], cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);
            var topic = recordId == null ? collection : $"{collection}/{recordId}";
            await UnsubscribeInternalAsync([topic], cancellationToken);
        }

        private static ILogger CreateDefaultLogger<T>()
        {
            if (OperatingSystem.IsBrowser())
            {
                return NullLogger<T>.Instance;
            }

            return LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<T>();
        }
    }
}
