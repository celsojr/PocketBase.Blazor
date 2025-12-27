using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    public class HttpTransport : IHttpTransport
    {
        readonly HttpClient _client;
        readonly PocketBaseOptions _pocketBaseOptions;
        private PocketBaseStore? _store;

        /// <inheritdoc />
        public HttpTransport(string baseUrl, HttpClient? httpClient = null, PocketBaseOptions? options = null)
        {
            _client = httpClient ?? new HttpClient() { BaseAddress = new Uri(baseUrl) };

            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("User-Agent", "PocketBase.Blazor");

            _pocketBaseOptions = options ?? new PocketBaseOptions();
        }

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
            var request = BuildRequest(method, path, body, query);
            var response = await _client.SendAsync(request, cancellationToken);
            return await HandleResponse<T>(response, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result> SendAsync(HttpMethod method, string path, object? body = null, IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            UpdateAuthorizationHeader();
            var request = BuildRequest(method, path, body, query);
            var response = await _client.SendAsync(request, cancellationToken);
            return await HandleResponse(response, cancellationToken);
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

        static async Task<Result> HandleResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string content = string.Empty;
            try
            {
                content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var ex = new PocketBaseException(response.StatusCode, content);
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
                    var ex = new PocketBaseException(response.StatusCode, content);
                    return Result.Fail<T>(
                        new ExceptionalError(ex)
                            .WithMetadata("status", (int)response.StatusCode)
                            .WithMetadata("raw", content)
                    );
                }

                if (typeof(T) == typeof(object))
                    return Result.Ok<T>(default!);

                T? wrapperData;
                try
                {
                    var wrapperOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new PocketBaseDateTimeConverter() }
                    };

                    wrapperData = JsonSerializer.Deserialize<T>(content, wrapperOptions);
                }
                catch (JsonException ex)
                {
                    return Result.Fail<T>($"Failed to deserialize wrapper: {ex.Message}").WithError(content);
                }

                if (wrapperData != null)
                {
                    var wrapperType = wrapperData.GetType();
                    if (wrapperType.IsGenericType && wrapperType.GetGenericTypeDefinition() == typeof(ListResult<>))
                    {
                        var itemsProp = wrapperType.GetProperty("Items");
                        if (itemsProp != null)
                        {
                            var itemsValue = itemsProp.GetValue(wrapperData) as IEnumerable<object>;
                            if (itemsValue != null)
                            {
                                var itemType = wrapperType.GetGenericArguments()[0];
                                var newItemsList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType))!;

                                foreach (var item in itemsValue)
                                {
                                    object typedItem;

                                    if (item is JsonElement je)
                                    {
                                        typedItem = JsonSerializer.Deserialize(
                                            je.GetRawText(),
                                            itemType,
                                            _pocketBaseOptions.JsonSerializerOptions
                                        )!;
                                    }
                                    else
                                    {
                                        typedItem = item;
                                    }

                                    newItemsList.Add(typedItem);
                                }

                                itemsProp.SetValue(wrapperData, newItemsList);
                            }
                        }
                    }
                }

                return wrapperData != null ? Result.Ok(wrapperData) : Result.Fail<T>("Deserialized value is null");
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
