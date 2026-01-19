namespace PocketBase.Blazor.UnitTests.Domain.Hosting;

using Blazor.Hosting;
using FluentAssertions;

public class PocketBaseHostBuilderTests
{
    private const string TestDataDir = "./test_pb_data";

    [Fact]
    public async Task CreateDefault_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var builder = PocketBaseHostBuilder.CreateDefault();

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public async Task UseOptions_ShouldConfigureCorrectly()
    {
        // Arrange
        var builder = PocketBaseHostBuilder.CreateDefault();

        // Act
        await builder
            .UseOptions(options =>
            {
                options.Host = "127.0.0.1";
                options.Port = 8888;
                options.Dir = TestDataDir;
                options.Dev = true;
            })
            .BuildAsync();

        // Assert
        var host = await builder.BuildAsync();
        host.Options.Should().NotBeNull();
        host.Options.Host.Should().Be("127.0.0.1");
        host.Options.Port.Should().Be(8888);
        host.Options.Dir.Should().Be(TestDataDir);
        host.Options.Dev.Should().BeTrue();
    }

    [Fact]
    public async Task UseExecutable_WithValidPath_ShouldConfigure()
    {
        // Arrange
        var builder = PocketBaseHostBuilder.CreateDefault();
        var exePath = @"C:\tools\pocketbase.exe";

        // Act
        await builder.UseExecutable(exePath).BuildAsync();

        // Assert
        var host = await builder.BuildAsync();
        host.ExecutablePath.Should().Be(exePath);
    }

    [Fact]
    public async Task UseExecutable_WithInvalidPath_ShouldThrow()
    {
        // Arrange
        var builder = PocketBaseHostBuilder.CreateDefault();
        var invalidPath = @"C:\nonexistent\pocketbase.exe";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await builder.UseExecutable(invalidPath).BuildAsync());
    }

    [Fact]
    public async Task BuildAsync_WithoutOptions_ShouldUseDefaults()
    {
        // Arrange
        var builder = PocketBaseHostBuilder.CreateDefault();

        // Act
        var host = await builder.BuildAsync();

        // Assert
        host.Options.Should().NotBeNull();
        host.Options.Host.Should().Be("localhost");
        host.Options.Port.Should().Be(8090);
        host.Options.Dir.Should().Contain("pb_data");
        host.Options.Dev.Should().BeFalse();
    }
}

