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

    public async Task<T?> GetRecordAsync<T>(string collection, string id)
    {
        var url = $"/api/collections/{collection}/records/{id}";
        _logger?.LogDebug("GET {Url}", url);
        return await _http.GetFromJsonAsync<T>(url, _options.JsonSerializerOptions);
    }

    public async Task<List<T>> GetListAsync<T>(string collection)
    {
        var url = $"/api/collections/{collection}/records";
        _logger?.LogDebug("GET {Url}", url);

        var response = await _http.GetFromJsonAsync<PocketBaseListResponse<T>>(url, _options.JsonSerializerOptions);
        return response?.Items ?? new List<T>();
    }

    public async Task<T?> CreateRecordAsync<T>(string collection, object payload)
    {
        var url = $"/api/collections/{collection}/records";
        _logger?.LogDebug("POST {Url}", url);

        var resp = await _http.PostAsJsonAsync(url, payload, _options.JsonSerializerOptions);
        if (!resp.IsSuccessStatusCode) return default;

        return await resp.Content.ReadFromJsonAsync<T>(_options.JsonSerializerOptions);
    }
}

internal class PocketBaseListResponse<T>
{
    public List<T> Items { get; set; } = new List<T>();
}
