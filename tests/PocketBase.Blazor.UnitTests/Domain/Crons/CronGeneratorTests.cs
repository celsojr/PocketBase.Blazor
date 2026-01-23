namespace PocketBase.Blazor.UnitTests.Domain.Crons;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Clients.Crons;
using Blazor.Models;
using Blazor.Options;
using Blazor.UnitTests.TestHelpers.Utilities;
using FluentAssertions;
using Xunit;

public class CronGeneratorTests
{
    private readonly CronGenerator _generator = new();

    [Fact]
    public async Task GenerateAsync_creates_registry_and_handlers_files()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "pb_crons_test");
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);

        var manifest = new CronManifest
        {
            Crons =
            [
                new()
                {
                    Id = "hello",
                    Description = "Hello cron",
                    HandlerBody = await File.ReadAllTextAsync(
                        Path.Combine(TestPaths.LiquidResponsesDirectory, "hello-handler.liquid")),
                    ImportPackages = ["time"]
                },
                new()
                {
                    Id = "db-dump",
                    Description = "Database dump cron",
                    HandlerBody = await File.ReadAllTextAsync(
                        Path.Combine(TestPaths.LiquidResponsesDirectory, "db-dump-handler.liquid")),
                    ImportPackages = ["os", "os/exec", "path/filepath"]
                }
            ]
        };

        var options = new CronGenerationOptions
        {
            ProjectDirectory = tempDir,
            BuildBinary = false
        };

        // Act
        await _generator.GenerateAsync(manifest, options, CancellationToken.None);

        // Assert
        var mainFile = Path.Combine(tempDir, "main.go");
        var registryFile = Path.Combine(tempDir, options.OutputDirectory, "registry.go");
        var handlersFile = Path.Combine(tempDir, options.OutputDirectory, "handlers.go");
        var typesFile = Path.Combine(tempDir, options.OutputDirectory, "types.go");
        var runtimFile = Path.Combine(tempDir, options.OutputDirectory, "runtime.go");

        File.Exists(mainFile).Should().BeTrue();
        File.Exists(registryFile).Should().BeTrue();
        File.Exists(handlersFile).Should().BeTrue();
        File.Exists(typesFile).Should().BeTrue();
        File.Exists(runtimFile).Should().BeTrue();

        var handlersContent = await File.ReadAllTextAsync(handlersFile);
        handlersContent.Should().Contain(manifest.Crons[0].Id);
        handlersContent.Should().Contain(manifest.Crons[1].Id);

        // Verify handler body customization
        handlersContent.Should().Contain("log.Printf(\"[CRON] Hello, %s %d\", name, count)");
        handlersContent.Should().Contain("cmd := exec.Command(\"sqlite3\", \"./pb_data/data.db\", \".output \"+outputDir, \".dump\")");

        // Verify imports
        handlersContent.Should().Contain("import (");
        handlersContent.Should().Contain("\"log\""); // default
        handlersContent.Should().Contain("\"time\"");
        handlersContent.Should().Contain("\"os\"");
        handlersContent.Should().Contain("\"os/exec\"");
        handlersContent.Should().Contain("\"path/filepath\"");
    }

    [Fact]
    public async Task GenerateAsync_uses_default_handler_body_when_not_specified()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "pb_crons_test_default");
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);

        var manifest = new CronManifest
        {
            Crons =
            [
                new() { Id = "default_cron", HandlerBody = "" }
            ]
        };

        var options = new CronGenerationOptions
        {
            ProjectDirectory = tempDir,
            BuildBinary = false
        };

        // Act
        await _generator.GenerateAsync(manifest, options, CancellationToken.None);

        // Assert
        var handlersFile = Path.Combine(tempDir, options.OutputDirectory, "handlers.go");
        var handlersContent = await File.ReadAllTextAsync(handlersFile);
        
        handlersContent.Should().Contain("log.Println(\"cron 'default_cron' executed\", payload)");
    }
}

