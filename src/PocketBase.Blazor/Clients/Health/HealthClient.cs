using System.Net.Http;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Health
{
    /// <inheritdoc />
    public class HealthClient : IHealthClient
    {
        readonly IHttpTransport _http;

        /// <inheritdoc />
        public HealthClient(IHttpTransport http)
        {
            _http = http;
        }

        /// <inheritdoc />
        public async Task<HealthCheckResponse> CheckAsync(CommonOptions? options = null)
        {
            var response = await _http.SendAsync<HealthCheckResponse>(
                HttpMethod.Get,
                "/api/health",
                query: options?.Query
            );

            return response;
        }
    }
}

