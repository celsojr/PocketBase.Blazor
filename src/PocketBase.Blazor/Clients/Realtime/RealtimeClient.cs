using System;
using System.Threading.Tasks;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Clients.Realtime
{
    public class RealtimeClient : IRealtimeClient
    {
        public Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IDisposable> SubscribeAsync(string topic, Action<RealtimeMessage> handler)
        {
            throw new NotImplementedException();
        }
    }
}
