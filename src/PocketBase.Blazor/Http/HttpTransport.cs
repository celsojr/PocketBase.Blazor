using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Exceptions;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Http
{
    /// <inheritdoc />
    public class HttpTransport : IHttpTransport
    {
        readonly HttpClient _client;
        readonly PocketBaseOptions _pocketBaseOptions;

        /// <inheritdoc />
        public HttpTransport(string baseUrl, HttpClient? httpClient = null, PocketBaseOptions? options = null)
        {
            _client = httpClient ?? new HttpClient() { BaseAddress = new Uri(baseUrl) };

            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("User-Agent", "PocketBase.Blazor");

            if (options?.ApiKey != null)
                _client.DefaultRequestHeaders.Add("Authorization", options.ApiKey);

            _pocketBaseOptions = options ?? new PocketBaseOptions();
        }

        /// <inheritdoc />
        public async Task<Result<T>> SendAsync<T>(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            var request = BuildRequest(method, path, body, query);
            var response = await _client.SendAsync(request, cancellationToken);
            return await HandleResponse<T>(response, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result<object>> SendAsync(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            var request = BuildRequest(method, path, body, query);
            var response = await _client.SendAsync(request, cancellationToken);
            return await HandleResponse<object>(response, cancellationToken);
        }

        HttpRequestMessage BuildRequest(HttpMethod method, string path, object? body, IDictionary<string, object?>? query)
        {
            var url = _client.BaseAddress + path;
            if (query != null)
                url += "?" + string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value?.ToString() ?? "")}"));

            var req = new HttpRequestMessage(method, url);
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, _pocketBaseOptions.JsonSerializerOptions);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            return req;
        }

        async Task<Result<T>> HandleResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string content = string.Empty;

            try
            {
                content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var ex = new PocketBaseException(response.StatusCode, content);
                    return Result.Fail<T>(ex.Message).WithError(ex.Message);
                }

                if (typeof(T) == typeof(object))
                    return Result.Ok<T>(default!);

                T? data;
                try
                {
                    data = JsonSerializer.Deserialize<T>(content, _pocketBaseOptions.JsonSerializerOptions);
                }
                catch (JsonException ex)
                {
                    return Result.Fail<T>($"Failed to deserialize response: {ex.Message}").WithError(content);
                }

                return data != null ? Result.Ok(data) : Result.Fail<T>("Deserialized value is null");
            }
            catch (HttpRequestException ex)
            {
                return Result.Fail<T>($"Request failed: {ex.Message}").WithError(content);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Result.Fail<T>("Request timed out").WithError(content);
            }
            catch (Exception ex)
            {
                return Result.Fail<T>($"Unexpected error: {ex.Message}").WithError(content);
            }
        }
    }
}
