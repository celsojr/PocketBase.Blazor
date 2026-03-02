namespace PocketBase.Blazor.UnitTests.Domain.Hosting;

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Blazor.Hosting;
using Blazor.Hosting.Interfaces;
using Blazor.Options;
using FluentAssertions;
using Xunit;

[Trait("Category", "Unit")]
[Trait("Requires", "FileSystem")]
public class HttpsConfigurationTests
{
    [Fact]
    public void BuildArguments_IncludesHttps_WhenUseHttpsTrue()
    {
        // Arrange
        string exe = Path.GetTempFileName();
        File.Delete(exe);
        File.WriteAllText(exe, string.Empty);

        PocketBaseHostOptions options = new PocketBaseHostOptions
        {
            Host = "127.0.0.1",
            Port = 9001,
            UseHttps = true,
            HttpsPort = 9443
        };

        try
        {
            // Act
            IPocketBaseHost host = new PocketBaseHost(exe, options);

            // Assert
            host.Process.Should().NotBeNull();
            host.Process.StartInfo.Arguments.Should().Contain("--http=127.0.0.1:9001");
            host.Process.StartInfo.Arguments.Should().Contain("--https=127.0.0.1:9443");
        }
        finally
        {
            File.Delete(exe);
        }
    }

    [Fact]
    public async Task UseEnvironmentVariables_Parses_HttpsVars()
    {
        // Arrange
        Environment.SetEnvironmentVariable("POCKETBASE_USE_HTTPS", "true");
        Environment.SetEnvironmentVariable("POCKETBASE_HTTPS_PORT", "9443");

        string exe = Path.GetTempFileName();
        File.Delete(exe);
        File.WriteAllText(exe, string.Empty);

        try
        {
            IPocketBaseHostBuilder builder = PocketBaseHostBuilder.CreateDefault();

            // Act
            IPocketBaseHost host = await builder
                                        .UseEnvironmentVariables()
                                        .UseExecutable(exe)
                                        .BuildAsync();

            // Assert
            host.Options.Should().NotBeNull();
            host.Options.UseHttps.Should().BeTrue();
            host.Options.HttpsPort.Should().Be(9443);
        }
        finally
        {
            Environment.SetEnvironmentVariable("POCKETBASE_USE_HTTPS", null);
            Environment.SetEnvironmentVariable("POCKETBASE_HTTPS_PORT", null);
            File.Delete(exe);
        }
    }

    [Fact]
    public async Task UseJsonConfiguration_Loads_HttpsSettings()
    {
        // Arrange
        var config = new
        {
            Host = "127.0.0.1",
            Port = 9002,
            UseHttps = true,
            HttpsPort = 9444,
            DataDir = "./data"
        };

        string file = Path.GetTempFileName();
        string exe = Path.GetTempFileName();
        File.Delete(file);
        File.Delete(exe);

        string json = JsonSerializer.Serialize(config);
        File.WriteAllText(file, json);
        File.WriteAllText(exe, string.Empty);

        try
        {
            IPocketBaseHostBuilder builder = PocketBaseHostBuilder.CreateDefault();

            // Act
            IPocketBaseHost host = await builder
                                        .UseJsonConfiguration(file)
                                        .UseExecutable(exe)
                                        .BuildAsync();

            // Assert
            host.Options.Should().NotBeNull();
            host.Options.UseHttps.Should().BeTrue();
            host.Options.HttpsPort.Should().Be(9444);
            host.Options.DataDir.Should().Be("./data");
        }
        finally
        {
            File.Delete(file);
            File.Delete(exe);
        }
    }
}
