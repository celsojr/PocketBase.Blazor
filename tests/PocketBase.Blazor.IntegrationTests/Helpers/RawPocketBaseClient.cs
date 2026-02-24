namespace PocketBase.Blazor.IntegrationTests.Helpers;

using System.Net.Http.Headers;
using System.Text.Json;

public class RawPocketBaseClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public RawPocketBaseClient(string baseUrl, string authToken)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    public async Task<string> GetRawAsync(string endpoint)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        string json = JsonSerializer.Serialize(data, _jsonOptions);
        StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        string responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
    }
}
