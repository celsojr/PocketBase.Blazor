using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;

namespace PocketBase.Blazor.Http
{
    /// <summary>
    /// Abstraction for HTTP transport layer.
    /// </summary>
    public interface IHttpTransport : IDisposable
    {
        /// <summary>
        /// Gets the base URL used for API requests.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Constructs a fully qualified URL by combining the base service address with the specified endpoint path.
        /// </summary>
        /// <param name="endpoint">The relative endpoint path to append to the base service address. Must not be <see langword="null"/> or empty.</param>
        /// <returns>A string containing the complete URL for the specified endpoint.</returns>
        string BuildUrl(string endpoint);

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
        /// Sends an HTTP request with the specified method, path, and multipart file content asynchronously.
        /// </summary>
        /// <param name="method">The HTTP method to use for the request (for example, <see cref="HttpMethod.Post"/> or <see cref="HttpMethod.Put"/>).</param>
        /// <param name="path">The relative URI path to which the request is sent. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="file">The multipart file to include in the request body. Cannot be <see langword="null"/>.</param>
        /// <param name="query">An optional dictionary of query string parameters to include in the request. May be <see langword="null"/> if no query parameters are needed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result"/> object representing the outcome of the request.</returns>
        Task<Result> SendAsync(HttpMethod method, string path, MultipartFile file, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default);

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

