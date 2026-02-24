using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Exceptions;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Store;

namespace PocketBase.Blazor.Http
{
    /// <inheritdoc />
    public sealed class HttpTransport : IHttpTransport
    {
        readonly HttpClient _client;
        readonly PocketBaseOptions _pocketBaseOptions;
        private PocketBaseStore? _store;
        private readonly bool _ownsClient;
        private bool _disposed;

        /// <inheritdoc />
        public string BaseUrl => _client.BaseAddress?.ToString() ?? string.Empty;

        /// <inheritdoc />
        public HttpTransport(string baseUrl, HttpClient? httpClient = null, PocketBaseOptions? options = null)
        {
            if (httpClient is null)
            {
                _client = new HttpClient
                {
                    BaseAddress = new Uri(baseUrl)
                };
                _ownsClient = true;
            }
            else
            {
                _client = httpClient;
                _ownsClient = false;
            }

            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            TryAddHeader("User-Agent", "PocketBase.Blazor");

            _pocketBaseOptions = options ?? new PocketBaseOptions();
        }

        private void TryAddHeader(string name, string value)
        {
            try
            {
                _client.DefaultRequestHeaders.Add(name, value);
            }
            catch (Exception) when (OperatingSystem.IsBrowser())
            {
                // Browser networking forbids some headers (eg. User-Agent).
                // Ignore so WASM can continue using Fetch defaults.
            }
        }

        /// <inheritdoc />
        public void SetStore(PocketBaseStore store)
        {
            _store = store;
        }

        private void UpdateAuthorizationHeader()
        {
            if (!string.IsNullOrEmpty(_store?.Token))
            {
                // Remove any previous token, but leave other headers intact
                _client.DefaultRequestHeaders.Remove("Authorization");
                _client.DefaultRequestHeaders.Add("Authorization", _store.Token);
            }
        }

        /// <inheritdoc />
        public async Task<Result<T>> SendAsync<T>(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            UpdateAuthorizationHeader();
            HttpRequestMessage request = BuildRequest(method, path, body, query);
            HttpResponseMessage response = await _client.SendAsync(request, cancellationToken);
            return await HandleResponse<T>(response, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result> SendAsync(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            UpdateAuthorizationHeader();
            HttpRequestMessage request = BuildRequest(method, path, body, query);
            HttpResponseMessage response = await _client.SendAsync(request, cancellationToken);
            return await HandleResponse(response, cancellationToken);
        }

        HttpRequestMessage BuildRequest(HttpMethod method, string path, object? body, IDictionary<string, object?>? query)
        {
            string url = BuildUrl(path);
            if (query != null)
                url += "?" + string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value?.ToString() ?? "")}"));

            HttpRequestMessage req = new HttpRequestMessage(method, url);

            if (body is HttpContent httpContent)
            {
                req.Content = httpContent;
            }
            else if (body != null)
            {
                string json = JsonSerializer.Serialize(body, _pocketBaseOptions.JsonSerializerOptions);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            return req;
        }

        /// <inheritdoc />
        public async Task<Result> SendAsync(HttpMethod method, string path, MultipartFile file, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            UpdateAuthorizationHeader();
            HttpRequestMessage request = BuildRequest(method, path, file, query);
            HttpResponseMessage response = await _client.SendAsync(request, cancellationToken);
            return await HandleResponse(response, cancellationToken);
        }

        private HttpRequestMessage BuildRequest(HttpMethod method, string path, MultipartFile file, IDictionary<string, object?>? query)
        {
            string url = BuildUrl(path);
            if (query != null)
                url += "?" + string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value?.ToString() ?? "")}"));

            HttpRequestMessage request = new HttpRequestMessage(method, url);

            MultipartFormDataContent formData = new MultipartFormDataContent();
            StreamContent streamContent = new StreamContent(file.Content);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            formData.Add(streamContent, file.Name, file.FileName);
    
            request.Content = formData;
            return request;
        }

        static async Task<Result> HandleResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string content = string.Empty;
            try
            {
                content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    PocketBaseException ex = new PocketBaseException(response.StatusCode, content);
                    return Result.Fail(ex.Message).WithError(ex.Message);
                }
                return Result.Ok();
            }
            catch (HttpRequestException ex)
            {
                return Result.Fail($"Request failed: {ex.Message}").WithError(content);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Result.Fail("Request timed out").WithError(content);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Unexpected error: {ex.Message}").WithError(content);
            }
        }

        async Task<Result<T>> HandleResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string content = string.Empty;

            try
            {
                content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    PocketBaseException ex = new PocketBaseException(response.StatusCode, content);

                    return Result.Fail<T>(
                        new ExceptionalError(ex)
                            .WithMetadata("status", (int)response.StatusCode)
                            .WithMetadata("raw", content)
                    );
                }

                if (IsListResult(typeof(T), out Type? itemType))
                {
                    using JsonDocument doc = JsonDocument.Parse(content);
                    JsonElement root = doc.RootElement;

                    JsonElement itemsElement = root.GetProperty("items");

                    object? items = JsonSerializer.Deserialize(
                        itemsElement.GetRawText(),
                        typeof(List<>).MakeGenericType(itemType),
                        _pocketBaseOptions.JsonSerializerOptions
                    );

                    object result = Activator.CreateInstance(typeof(ListResult<>).MakeGenericType(itemType))!;

                    typeof(T).GetProperty("Page")!.SetValue(result, root.GetProperty("page").GetInt32());
                    typeof(T).GetProperty("PerPage")!.SetValue(result, root.GetProperty("perPage").GetInt32());
                    typeof(T).GetProperty("TotalItems")!.SetValue(result, root.GetProperty("totalItems").GetInt32());
                    typeof(T).GetProperty("TotalPages")!.SetValue(result, root.GetProperty("totalPages").GetInt32());
                    typeof(T).GetProperty("Items")!.SetValue(result, items);

                    return Result.Ok((T)result);
                }

                JsonSerializerOptions wrapperOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new PocketBaseDateTimeConverter() }
                };

                T? data = JsonSerializer.Deserialize<T>(content, wrapperOptions);

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

        static bool IsListResult(Type type, out Type itemType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ListResult<>))
            {
                itemType = type.GetGenericArguments()[0];
                return true;
            }

            itemType = null!;
            return false;
        }

        /// <inheritdoc />
        public async Task<Result<Stream>> SendForStreamAsync(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            UpdateAuthorizationHeader();
            HttpRequestMessage request = BuildRequest(method, path, body, query);
            HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                PocketBaseException ex = new PocketBaseException(response.StatusCode, content);
                return Result.Fail<Stream>(ex.Message).WithError(ex.Message);
            }
            Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return Result.Ok(stream);
        }

        /// <inheritdoc />
        public async Task<Result<byte[]>> SendForBytesAsync(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            UpdateAuthorizationHeader();
            HttpRequestMessage request = BuildRequest(method, path, body, query);
            HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                PocketBaseException ex = new PocketBaseException(response.StatusCode, content);
                return Result.Fail<byte[]>(ex.Message).WithError(ex.Message);
            }
            byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return Result.Ok(bytes);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<string> SendForSseAsync(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            UpdateAuthorizationHeader();
            HttpRequestMessage request = BuildRequest(method, path, body, query);
            request.Headers.Accept.Clear();
            request.Headers.Accept.ParseAdd("text/event-stream");
            HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using StreamReader reader = new StreamReader(stream);
            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null && !cancellationToken.IsCancellationRequested)
            {
                yield return line;
            }
        }

        /// <inheritdoc />
        public string BuildUrl(string endpoint)
        {
            string? baseUrl = _client.BaseAddress?.ToString();
            return $"{baseUrl?.TrimEnd('/')}/{endpoint.TrimStart('/')}";
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_ownsClient)
            {
                _client.Dispose();
            }
        }
    }
}
