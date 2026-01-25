using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using FluentResults;

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
        Task<Result<T>> SendAsync<T>(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP request.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="path">The request path.</param>
        /// <param name="body">The request body.</param>
        /// <param name="query">The query parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<Result> SendAsync(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP request and returns the response as a stream (for file downloads, etc).
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="path">The request path.</param>
        /// <param name="body">The request body.</param>
        /// <param name="query">The query parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<Result<System.IO.Stream>> SendForStreamAsync(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP request and returns the response as a byte array (for file downloads, etc).
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="path">The request path.</param>
        /// <param name="body">The request body.</param>
        /// <param name="query">The query parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<Result<byte[]>> SendForBytesAsync(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP request and returns an async stream of SSE events (for realtime updates).
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="path">The request path.</param>
        /// <param name="body">The request body.</param>
        /// <param name="query">The query parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of SSE events.</returns>
        IAsyncEnumerable<string> SendForSseAsync(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, System.Threading.CancellationToken cancellationToken = default);
    }
}

