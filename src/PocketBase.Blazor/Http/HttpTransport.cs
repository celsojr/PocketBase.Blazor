using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using PocketBase.Blazor.Exceptions;

namespace PocketBase.Blazor.Http
{
    public class HttpTransport : IHttpTransport
    {
        readonly HttpClient _client;
        readonly string _baseUrl;

        public HttpTransport(string baseUrl, HttpClient? httpClient = null)
        {
            _baseUrl = baseUrl;
            _client = httpClient ?? new HttpClient();
        }

        public async Task<T> SendAsync<T>(HttpMethod method, string path, object? body = null, IDictionary<string, string>? query = null)
        {
            var request = BuildRequest(method, path, body, query);
            var response = await _client.SendAsync(request);
            return await HandleResponse<T>(response);
        }

        public async Task SendAsync(HttpMethod method, string path, object? body = null, IDictionary<string, string>? query = null)
        {
            var request = BuildRequest(method, path, body, query);
            var response = await _client.SendAsync(request);
            await HandleResponse<object>(response);
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

        async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new PocketBaseException(response.StatusCode, text);

            if (typeof(T) == typeof(object))
                return default!;

            return JsonSerializer.Deserialize<T>(text)!;
        }
    }
}
