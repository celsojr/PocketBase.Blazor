using System;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Http;

namespace PocketBase.Blazor.Clients.Realtime
{
    public class RealtimeClient : IRealtimeClient
    {
        private IHttpTransport _http;

        public RealtimeClient(IHttpTransport http)
        {
            _http = http;
        }

        public Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IDisposable> SubscribeAsync(string topic, Action<RecordSubscriptionEvent> handler, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
