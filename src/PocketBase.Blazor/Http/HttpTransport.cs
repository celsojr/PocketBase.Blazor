using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Exceptions;

namespace PocketBase.Blazor.Http
{
    /// <inheritdoc />
    public class HttpTransport : IHttpTransport
    {
        readonly HttpClient _client;
        readonly string _baseUrl;

        /// <inheritdoc />
        public HttpTransport(string baseUrl, HttpClient? httpClient = null)
        {
            _baseUrl = baseUrl;
            _client = httpClient ?? new HttpClient();
        }

        /// <inheritdoc />
        public async Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null, IDictionary<string, string>? query = null, CancellationToken cancellationToken = default)
        {
            var request = BuildRequest(method, path, body, query);
            var response = await _client.SendAsync(request, cancellationToken);
            return await HandleResponse<T>(response, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SendAsync(HttpMethod method, string path, object? body = null, IDictionary<string, string>? query = null, CancellationToken cancellationToken = default)
        {
            var request = BuildRequest(method, path, body, query);
            var response = await _client.SendAsync(request, cancellationToken);
            await HandleResponse<object>(response, cancellationToken);
        }

        HttpRequestMessage BuildRequest(HttpMethod method, string path, object? body, IDictionary<string, string>? query)
        {
            var url = _baseUrl + path;
            if (query != null)
                url += "?" + string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

            var req = new HttpRequestMessage(method, url);
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            return req;
        }

        static async Task<T> HandleResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var text = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new PocketBaseException(response.StatusCode, text);

            if (typeof(T) == typeof(object))
                return default!;

            // Use JsonSerializerOptions for case-insensitive matching
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new PocketBaseDateTimeConverter());

            return JsonSerializer.Deserialize<T>(text, options)!;
        }
    }
}
