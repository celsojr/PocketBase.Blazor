namespace PocketBase.Blazor.UnitTests.Domain.Hosting;

using Blazor.Hosting;
using Blazor.Hosting.Interfaces;
using FluentAssertions;

[Trait("Category", "Unit")]
[Trait("Requires", "FileSystem")]
[Trait("Requires", "GoRuntime")]
public class ConfigurationTests
{
    [Fact]
    public async Task UseEnvironmentVariables_ShouldOverrideDefaults()
    {
        // Arrange
        Environment.SetEnvironmentVariable("POCKETBASE_HOST", "0.0.0.0");
        Environment.SetEnvironmentVariable("POCKETBASE_PORT", "8888");
        Environment.SetEnvironmentVariable("POCKETBASE_DIR", "./custom_data");
        Environment.SetEnvironmentVariable("POCKETBASE_DEV", "true");

        try
        {
            IPocketBaseHostBuilder builder = PocketBaseHostBuilder.CreateDefault();

            // Act
            await builder
                .UseEnvironmentVariables()
                .BuildAsync();

            IPocketBaseHost host = await builder.BuildAsync();

            // Assert
            host.Options.Should().NotBeNull();
            host.Options.Host.Should().Be("0.0.0.0");
            host.Options.Port.Should().Be(8888);
            host.Options.Dir.Should().Be("./custom_data");
            host.Options.Dev.Should().BeTrue();
        }
        finally
        {
            // Clean up
            Environment.SetEnvironmentVariable("POCKETBASE_HOST", null);
            Environment.SetEnvironmentVariable("POCKETBASE_PORT", null);
            Environment.SetEnvironmentVariable("POCKETBASE_DIR", null);
            Environment.SetEnvironmentVariable("POCKETBASE_DEV", null);
        }
    }

    [Fact]
    public async Task UseConfigurationFile_ShouldLoadSettings()
    {
        // Arrange
        string configJson = @"{
            ""Host"": ""192.168.1.100"",
            ""Port"": 9090,
            ""Dir"": ""./config_data"",
            ""Dev"": false
        }";

        File.WriteAllText("pocketbase.config.json", configJson);

        try
        {
            IPocketBaseHostBuilder builder = PocketBaseHostBuilder.CreateDefault();

            // Act
            await builder.UseConfigurationFile("pocketbase.config.json").BuildAsync();
            IPocketBaseHost host = await builder.BuildAsync();

            // Assert
            host.Options.Should().NotBeNull();
            host.Options.Host.Should().Be("192.168.1.100");
            host.Options.Port.Should().Be(9090);
            host.Options.Dir.Should().Be("./config_data");
            host.Options.Dev.Should().BeFalse();
        }
        finally
        {
            File.Delete("pocketbase.config.json");
        }
    }
}
