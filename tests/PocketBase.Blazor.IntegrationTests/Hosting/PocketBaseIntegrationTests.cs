namespace PocketBase.Blazor.IntegrationTests.Hosting;

using System;
using System.Text.Json.Serialization;
using Blazor.Hosting;
using Blazor.Hosting.Interfaces;
using Blazor.IntegrationTests.Helpers;
using Blazor.Responses.Auth;
using Microsoft.Extensions.Logging;

[Trait("Category", "Integration")]
[Trait("Requires", "FileSystem")]
[Trait("Requires", "GoRuntime")]
public class PocketBaseIntegrationTests : IAsyncLifetime
{
    private IPocketBase? _pb;
    private IPocketBaseHost? _host;
    private int _port;
    private HttpClient? _httpClient;
    private readonly string _scheme = "http";
    private readonly string _hostName = "127.0.0.1";

    public TestSettings Settings { get; } = new();

    public async Task InitializeAsync()
    {
        _port = new Random().Next(9000, 9999);

        IPocketBaseHostBuilder builder = PocketBaseHostBuilder.CreateDefault();

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        ILogger<PocketBaseHost> logger = loggerFactory.CreateLogger<PocketBaseHost>();

        await builder
            .UseLogger(logger)
            .UseOptions(options =>
            {
                options.Host = _hostName;
                options.Port = _port;
                options.DataDir = TestPaths.TestDataDirectory;
                options.MigrationsDir = TestPaths.TestMigrationDirectory;
                options.Dev = true;
            })
            .BuildAsync();

        _host = await builder.BuildAsync();

        // Note: Make sure there is no other active
        // instance being served through the same port
        await _host.StartAsync();

        _httpClient = new HttpClient() { BaseAddress = new Uri($"{_scheme}://{_hostName}:{_port}") };

        PocketBaseOptions options = new PocketBaseOptions();
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = true;

        _pb = new PocketBase($"{_scheme}://{_hostName}:{_port}", options: options);

        // Wait for PocketBase to be ready
        await WaitForPocketBaseReady();
    }

    private async Task WaitForPocketBaseReady(int maxAttempts = 10)
    {
        // Note: Production implementations should use robust retry mechanisms
        // that handle transient failures, network issues, and service startup delays
        // more gracefully than this simple linear retry approach.

        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                if (_httpClient != null)
                {
                    HttpResponseMessage response = await _httpClient.GetAsync("/api/health");
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

        if (_pb != null)
        {
            await _pb.DisposeAsync();
        }

        TestPaths.CleanUpDataDir();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldRespond()
    {
        // Act
        _httpClient.Should().NotBeNull();
        HttpResponseMessage response = await _httpClient.GetAsync("/api/health");

        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("code", "Health endpoint should return status");
        content.Should().Contain("200", "Status code should be 200 OK");
    }

    [Fact]
    public async Task AdminAuthentication_ShouldWork()
    {
        // Act & Assert
        _pb.Should().NotBeNull();

        Result<AuthResponse> result = await _pb.Admins
            .AuthWithPasswordAsync(
                Settings.AdminTesterEmail,
                Settings.AdminTesterPassword
            );

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrEmpty();
    }
}
