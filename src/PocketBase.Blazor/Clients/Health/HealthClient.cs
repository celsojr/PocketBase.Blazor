using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;

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
        public async Task<HealthCheckResponse> CheckAsync(CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            var response = await _http.SendAsync<HealthCheckResponse>(
                HttpMethod.Get,
                "/api/health",
                query: options?.Query,
                cancellationToken: cancellationToken
            );

            return response;
        }
    }
}

