using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PocketBase.Blazor;

public class PocketBaseClient : IPocketBaseClient
{
    private readonly HttpClient _http;
    private readonly PocketBaseOptions _options;
    private readonly ILogger<PocketBaseClient>? _logger;

    public PocketBaseClient(HttpClient http, PocketBaseOptions options, ILogger<PocketBaseClient>? logger = null)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    public async Task<(T? Value, PocketBaseError? Error)> GetRecordAsync<T>(string collection, string id)
    {
        var url = $"/api/collections/{collection}/records/{id}";
        _logger?.LogDebug("GET {Url}", url);

        var resp = await _http.GetAsync(url);
        var (_, Value, Error) = await ReadResponse<T>(resp);

        return (Value, Error);
    }

    public async Task<(List<T> Items, PocketBaseError? Error)> GetListAsync<T>(string collection)
    {
        var url = $"/api/collections/{collection}/records";
        _logger?.LogDebug("GET {Url}", url);

        var resp = await _http.GetAsync(url);
        var (_, Value, Error) = await ReadResponse<PocketBaseListResponse<T>>(resp);

        return (Value?.Items ?? [], Error);
    }

    public async Task<(T? Value, PocketBaseError? Error)> CreateRecordAsync<T>(string collection, object payload)
    {
        var url = $"/api/collections/{collection}/records";
        _logger?.LogDebug("POST {Url}", url);

        var resp = await _http.PostAsJsonAsync(url, payload, _options.JsonSerializerOptions);
        var (_, Value, Error) = await ReadResponse<T>(resp);

        return (Value, Error);
    }

    private async Task<(bool Ok, T? Value, PocketBaseError? Error)> ReadResponse<T>(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode)
        {
            var value = await resp.Content.ReadFromJsonAsync<T>(_options.JsonSerializerOptions);
            return (true, value, null);
        }

        var error = await resp.Content.ReadFromJsonAsync<PocketBaseError>(_options.JsonSerializerOptions);
        if (error != null)
            return (false, default, error);

        return (false, default, new PocketBaseError
        {
            Status = (int)resp.StatusCode,
        });
    }
}

internal class PocketBaseListResponse<T>
{
    public List<T> Items { get; set; } = new List<T>();
}
