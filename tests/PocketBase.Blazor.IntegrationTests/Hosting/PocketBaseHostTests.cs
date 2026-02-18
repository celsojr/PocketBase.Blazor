namespace PocketBase.Blazor.IntegrationTests.Hosting;

using System.Diagnostics;
using Blazor.Hosting;
using Blazor.Hosting.Interfaces;
using Microsoft.Extensions.Logging;

public class PocketBaseHostTests : IAsyncLifetime
{
    private IPocketBaseHost? _host;
    private readonly string _testDataDir = "./test_data_" + Guid.NewGuid().ToString()[..8];

    public async Task InitializeAsync()
    {
        var builder = PocketBaseHostBuilder.CreateDefault();

        var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        var logger = loggerFactory.CreateLogger<PocketBaseHost>();

        await builder
            .UseLogger(logger)
            .UseOptions(options =>
            {
                //options.Host = "127.0.0.1";
                //options.Port = GetRandomPort();
                options.Dir = _testDataDir;
                options.Dev = false;
            })
            .BuildAsync();

        _host = await builder.BuildAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host != null)
        {
            await _host.DisposeAsync();
        }

        // Clean up test directory
        if (Directory.Exists(_testDataDir))
        {
            Directory.Delete(_testDataDir, true);
        }
    }

    //private static int GetRandomPort() => Random.Shared.Next(9000, 9999);

    [Fact]
    public async Task StartAsync_ShouldLaunchProcess()
    {
        // Act - Make sure there is no other active
        // instance being served through the same port
        _host.Should().NotBeNull();
        await _host.StartAsync();

        // Assert
        _host.Process.Should().NotBeNull();
        _host.Process.HasExited.Should().BeFalse();
        _host.Options.Should().NotBeNull();

        // Verify process is responsive
        using var client = new HttpClient();
        var response = await client.GetAsync(
            $"http://{_host.Options.Host}:{_host.Options.Port}/api/health");

        response.Should().NotBeNull();
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_WhenAlreadyRunning_ShouldThrow()
    {
        // Arrange
        _host.Should().NotBeNull();
        await _host.StartAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _host.StartAsync());
    }

    [Fact]
    public async Task StopAsync_ShouldTerminateProcess()
    {
        // Arrange
        _host.Should().NotBeNull();
        await _host.StartAsync();

        // Act
        await _host.StopAsync();

        // Assert
        _host.Process.Should().NotBeNull();
        _host.Process.HasExited.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_ShouldCleanupResources()
    {
        // Arrange
        _host.Should().NotBeNull();
        await _host.StartAsync();

        _host.Process.Should().NotBeNull();
        var processId = _host.Process.Id;

        // Act
        await _host.DisposeAsync();

        // Assert - Verify process is terminated
        Assert.Throws<ArgumentException>(() =>
            Process.GetProcessById(processId));
    }

    [Fact]
    public async Task RestartAsync_ShouldStopAndStart()
    {
        // Arrange
        _host.Should().NotBeNull();
        await _host.StartAsync();

        _host.Process.Should().NotBeNull();
        var originalProcessId = _host.Process.Id;

        // Act - Make sure there is no other active
        // instance being served through the same port
        await _host.RestartAsync();

        // Assert
        _host.Process.Id.Should().NotBe(originalProcessId);
        _host.Process.HasExited.Should().BeFalse();
    }
}

