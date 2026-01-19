namespace PocketBase.Blazor.IntegrationTests.Hosting;

using System.Text;
using Blazor.Hosting;
using Blazor.Hosting.Interfaces;

[Collection("PocketBase Integration")]
public class PocketBaseIntegrationTests : IAsyncLifetime
{
    private IPocketBaseHost _host;
    private HttpClient _httpClient;
    private int _port;
    
    public async Task InitializeAsync()
    {
        _port = new Random().Next(9000, 9999);
        var dataDir = $"./integration_data_{_port}";
        
        var builder = PocketBaseHostBuilder.CreateDefault();
        await builder
            .UseOptions(options =>
            {
                options.Host = "127.0.0.1";
                options.Port = _port;
                options.Dir = dataDir;
                options.Dev = true;
            })
            .BuildAsync();
        
        _host = await builder.BuildAsync();
        await _host.StartAsync();
        
        _httpClient = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{_port}") };
        
        // Wait for PocketBase to be ready
        await WaitForPocketBaseReady();
    }
    
    private async Task WaitForPocketBaseReady(int maxAttempts = 30)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/health");
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch { }
            
            await Task.Delay(1000);
        }
        
        throw new TimeoutException("PocketBase failed to start");
    }
    
    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();
        
        if (_host != null)
        {
            await _host.DisposeAsync();
        }
        
        // Clean up data directory
        var dataDir = $"./integration_data_{_port}";
        if (Directory.Exists(dataDir))
        {
            Directory.Delete(dataDir, true);
        }
    }
    
    [Fact]
    public async Task HealthEndpoint_ShouldRespond()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/health");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("code", "Health endpoint should return status");
    }
    
    [Fact]
    public async Task AdminAuthentication_ShouldWork()
    {
        // Arrange
        var adminEmail = "test@example.com";
        var adminPassword = "Test123456!";
        
        // Create admin first
        var createResponse = await _httpClient.PostAsync("/api/admins", 
            new StringContent(JsonSerializer.Serialize(new
            {
                email = adminEmail,
                password = adminPassword,
                passwordConfirm = adminPassword
            }), Encoding.UTF8, "application/json"));
        
        createResponse.EnsureSuccessStatusCode();
        
        // Act - Authenticate
        var authResponse = await _httpClient.PostAsync("/api/admins/auth-with-password",
            new StringContent(JsonSerializer.Serialize(new
            {
                identity = adminEmail,
                password = adminPassword
            }), Encoding.UTF8, "application/json"));
        
        // Assert
        authResponse.EnsureSuccessStatusCode();
        var authContent = await authResponse.Content.ReadAsStringAsync();
        authContent.Should().Contain("token", "Authentication should return token");
    }
    
    [Fact]
    public async Task CollectionsCRUD_ShouldWork()
    {
        // Arrange - Create collection
        var collectionData = new
        {
            name = "test_items",
            type = "base",
            schema = new object[]
            {
                new { name = "title", type = "text", required = true },
                new { name = "description", type = "text" }
            }
        };
        
        // Act - Create collection
        var createResponse = await _httpClient.PostAsync("/api/collections",
            new StringContent(JsonSerializer.Serialize(collectionData), 
                Encoding.UTF8, "application/json"));
        
        // Assert
        createResponse.EnsureSuccessStatusCode();
        
        // Verify collection exists
        var listResponse = await _httpClient.GetAsync("/api/collections");
        listResponse.EnsureSuccessStatusCode();
        
        var collections = await listResponse.Content.ReadAsStringAsync();
        collections.Should().Contain("test_items");
    }
}

