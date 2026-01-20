namespace PocketBase.Blazor.IntegrationTests.Hosting;

using System.Text.Json.Serialization;
using Blazor.Hosting;
using Blazor.Hosting.Interfaces;
using Blazor.IntegrationTests.Helpers;
using Microsoft.Extensions.Logging;

[Collection("PocketBase.Blazor Integration")]
public class PocketBaseIntegrationTests : IAsyncLifetime
{
    private IPocketBase? _pb;
    private IPocketBaseHost? _host;
    private int _port;
    private HttpClient? _httpClient;

    public TestSettings Settings { get; } = new();

    public async Task InitializeAsync()
    {
        _port = new Random().Next(9000, 9999);

        var builder = PocketBaseHostBuilder.CreateDefault();

        var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        var logger = loggerFactory.CreateLogger<PocketBaseHost>();

        await builder
            .UseLogger(logger)
            .UseOptions(options =>
            {
                options.Host = "127.0.0.1";
                options.Port = _port;
                options.Dir = TestPaths.TestDataDirectory;
                options.MigrationsDir = TestPaths.TestMigrationDirectory;
                options.Dev = true;
            })
            .BuildAsync();

        _host = await builder.BuildAsync();
        await _host.StartAsync();

        _httpClient = new HttpClient() { BaseAddress = new Uri("http://127.0.0.1:8090") };

        var options = new PocketBaseOptions();
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = true;

        // Allowing different ports is still under investigation
        // Only working when behind a reverse proxy in a local machine
        _pb = new PocketBase("http://127.0.0.1:8090", options: options);

        // Wait for PocketBase to be ready
        await WaitForPocketBaseReady();
    }

    private async Task WaitForPocketBaseReady(int maxAttempts = 30)
    {
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                if (_httpClient != null)
                {
                    var response = await _httpClient.GetAsync("/api/health");
                    if (response.IsSuccessStatusCode)
                        return;
                }
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
        if (Directory.Exists(TestPaths.TestDataDirectory))
        {
            foreach (var file in Directory.GetFiles(TestPaths.TestDataDirectory))
            {
                if (Path.GetFileName(file) != ".gitignore")
                    File.Delete(file);
            }
        }
    }

    [Fact]
    public async Task HealthEndpoint_ShouldRespond()
    {
        // Act
        _httpClient.Should().NotBeNull();
        var response = await _httpClient.GetAsync("/api/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("code", "Health endpoint should return status");
        content.Should().Contain("200", "Status code should be 200 OK");
    }

    [Fact]
    public async Task AdminAuthentication_ShouldWork()
    {
        // Act & Assert
        _pb.Should().NotBeNull();

        var result = await _pb.Admins
            .AuthWithPasswordAsync(
                Settings.AdminTesterEmail,
                Settings.AdminTesterPassword
            );

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrEmpty();
    }

    // TODO: WIP
    //[Fact]
    //public async Task CollectionsCRUD_ShouldWork()
    //{
    //    // Arrange - Create collection
    //    var collectionData = new
    //    {
    //        name = "test_items",
    //        type = "base",
    //        schema = new object[]
    //        {
    //            new { name = "title", type = "text", required = true },
    //            new { name = "description", type = "text" }
    //        }
    //    };

    //    // Act - Create collection
    //    _httpClient.Should().NotBeNull();
    //    var createResponse = await _httpClient.PostAsync("/api/collections",
    //        new StringContent(JsonSerializer.Serialize(collectionData),
    //            Encoding.UTF8, "application/json"));

    //    // Assert
    //    createResponse.EnsureSuccessStatusCode();

    //    // Verify collection exists
    //    var listResponse = await _httpClient.GetAsync("/api/collections");
    //    listResponse.EnsureSuccessStatusCode();

    //    var collections = await listResponse.Content.ReadAsStringAsync();
    //    collections.Should().Contain("test_items");
    //}
}

