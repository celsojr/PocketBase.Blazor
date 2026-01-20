namespace PocketBase.Blazor.UnitTests.Domain.Hosting;

using System.Runtime.InteropServices;
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

        var host = await builder.BuildAsync();

        // Assert
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

        var os = GetOSPlatform();
        var extension = os == OSPlatform.Windows ? ".exe" : "";
        var appData = Environment.SpecialFolder.LocalApplicationData;

        var exePath = Path.Combine(
            Environment.GetFolderPath(appData),
            "pocketbase",
            $"pocketbase{extension}"
        );

        // Act
        await builder.UseExecutable(exePath).BuildAsync();
        var host = await builder.BuildAsync();

        // Assert
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
        host.Options.Host.Should().Be("127.0.0.1");
        host.Options.Port.Should().Be(8090);
        host.Options.Dir.Should().BeNull();
        host.Options.Dev.Should().BeFalse();
    }

    private static OSPlatform GetOSPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return OSPlatform.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OSPlatform.Linux;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return OSPlatform.OSX;

        throw new PlatformNotSupportedException();
    }
}

