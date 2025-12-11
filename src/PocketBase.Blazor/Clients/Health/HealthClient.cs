using System;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;

namespace PocketBase.Blazor.Clients.Health
{
    public class HealthClient : IHealthClient
    {
        readonly IHttpTransport _http;

        public HealthClient(IHttpTransport http)
        {
            _http = http;
        }

        public Task<bool> CheckAsync()
        {
            throw new NotImplementedException();
        }
    }
}
