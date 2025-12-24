using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace PocketBase.Blazor.Http
{
    /// <summary>
    /// Abstraction for HTTP transport layer.
    /// </summary>
    public interface IHttpTransport
    {
        /// <summary>
        /// Sends an HTTP request.
        /// </summary>
        /// <typeparam name="T">The type of the response.</typeparam>
        /// <param name="method">The HTTP method.</param>
        /// <param name="path">The request path.</param>
        /// <param name="body">The request body.</param>
        /// <param name="query">The query parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null, IDictionary<string, string>? query = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP request.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="path">The request path.</param>
        /// <param name="body">The request body.</param>
        /// <param name="query">The query parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SendAsync(HttpMethod method, string path, object? body = null, IDictionary<string, string>? query = null, CancellationToken cancellationToken = default);
    }
}

