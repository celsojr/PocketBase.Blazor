namespace PocketBase.Blazor.UnitTests.Domain.Crons;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Clients.Crons;
using Blazor.Models;
using Blazor.Options;
using FluentAssertions;
using Xunit;

public class CronGeneratorTests
{
    private readonly ICronGenerator _generator = new CronGenerator();

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
                new() { Id = "hello", Handler = "HelloCron", Description = "Simple hello cron" },
                new() 
                { 
                    Id = "send_email", 
                    Handler = "SendEmailCron",
                    Description = "Email sending cron",
                    HandlerBody = "fmt.Println(\"Sending email via cron\", payload)"
                },
                new() 
                { 
                    Id = "custom_logic", 
                    Handler = "CustomLogicCron",
                    Description = "Cron with custom imports",
                    HandlerBody = "time.Sleep(1 * time.Second)\nfmt.Println(\"Custom logic executed\")",
                    ImportPackages = ["fmt", "time", "strings"]
                }
            ]
        };

        var options = new CronGenerationOptions
        {
            ProjectDirectory = tempDir,
            OutputDirectory = "internal/crons",
            BuildBinary = false
        };

        // Act
        await _generator.GenerateAsync(manifest, options, CancellationToken.None);

        // Assert
        var registryFile = Path.Combine(tempDir, options.OutputDirectory, "registry.go");
        var handlersFile = Path.Combine(tempDir, options.OutputDirectory, "handlers.go");

        File.Exists(registryFile).Should().BeTrue();
        File.Exists(handlersFile).Should().BeTrue();

        var registryContent = await File.ReadAllTextAsync(registryFile);
        registryContent.Should().Contain("hello");
        registryContent.Should().Contain("send_email");
        registryContent.Should().Contain("custom_logic");

        var handlersContent = await File.ReadAllTextAsync(handlersFile);
        handlersContent.Should().Contain("func HelloCron");
        handlersContent.Should().Contain("func SendEmailCron");
        handlersContent.Should().Contain("func CustomLogicCron");
        
        // Verify handler body customization
        handlersContent.Should().Contain("fmt.Println(\"Sending email via cron\", payload)");
        
        // Verify custom logic
        handlersContent.Should().Contain("time.Sleep(1 * time.Second)");
        handlersContent.Should().Contain("fmt.Println(\"Custom logic executed\")");
        
        // Verify imports
        handlersContent.Should().Contain("import (");
        handlersContent.Should().Contain("\"log\""); // default
        handlersContent.Should().Contain("\"fmt\""); // from custom logic
        handlersContent.Should().Contain("\"time\""); // from inline import comment
        handlersContent.Should().Contain("\"strings\""); // from ImportPackages
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
                new() { Id = "default_cron", Handler = "DefaultCron" }
            ]
        };

        var options = new CronGenerationOptions
        {
            ProjectDirectory = tempDir,
            OutputDirectory = "internal/crons",
            BuildBinary = false
        };

        // Act
        await _generator.GenerateAsync(manifest, options, CancellationToken.None);

        // Assert
        var handlersFile = Path.Combine(tempDir, options.OutputDirectory, "handlers.go");
        var handlersContent = await File.ReadAllTextAsync(handlersFile);
        
        handlersContent.Should().Contain("log.Println(\"cron 'default_cron' executed\", payload)");
    }

    [Fact]
    public async Task GenerateAsync_handles_import_comments_in_handler_body()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "pb_crons_test_imports");
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);

        var manifest = new CronManifest
        {
            Crons =
            [
                new() 
                { 
                    Id = "complex_cron", 
                    Handler = "ComplexCron",
                    HandlerBody = "// import \"fmt\", \"time\", \"strings\"\nnow := time.Now()\nmsg := fmt.Sprintf(\"Ran at %v\", now)\nif strings.Contains(msg, \"Ran\") {\n    fmt.Println(msg)\n}"
                }
            ]
        };

        var options = new CronGenerationOptions
        {
            ProjectDirectory = tempDir,
            OutputDirectory = "internal/crons",
            BuildBinary = false
        };

        // Act
        await _generator.GenerateAsync(manifest, options, CancellationToken.None);

        // Assert
        var handlersFile = Path.Combine(tempDir, options.OutputDirectory, "handlers.go");
        var handlersContent = await File.ReadAllTextAsync(handlersFile);
        
        // Verify imports are properly extracted
        handlersContent.Should().Contain("\"fmt\"");
        handlersContent.Should().Contain("\"time\"");
        handlersContent.Should().Contain("\"strings\"");
        
        // Verify import comment is removed from handler body
        handlersContent.Should().NotContain("// import");
        
        // Verify actual handler code is present
        handlersContent.Should().Contain("now := time.Now()");
        handlersContent.Should().Contain("if strings.Contains(msg, \"Ran\")");
    }
}

