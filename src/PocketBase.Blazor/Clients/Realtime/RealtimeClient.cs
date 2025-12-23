using System;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;

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

        public Task<IDisposable> SubscribeAsync(string topic, Action<RealtimeMessage> handler, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
