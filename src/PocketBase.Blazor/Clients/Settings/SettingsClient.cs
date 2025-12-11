using System;
using System.Text.Json;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;

namespace PocketBase.Blazor.Clients.Settings
{
    public class SettingsClient : ISettingsClient
    {
        readonly IHttpTransport _http;

        public SettingsClient(IHttpTransport http)
        {
            _http = http;
        }

        public Task<JsonElement> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(object settings)
        {
            throw new NotImplementedException();
        }
    }
}
