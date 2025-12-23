using System.Threading.Tasks;
using PocketBase.Blazor.Exceptions;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Health
{
    /// <summary>
    /// Client interface for performing health checks on the PocketBase server.
    /// </summary>
    public interface IHealthClient
    {
        /// <summary>
        /// Checks the health status of the api.
        /// </summary>
        /// <param name="options"></param>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<HealthCheckResponse> CheckAsync(CommonOptions? options = null);
    }
}

