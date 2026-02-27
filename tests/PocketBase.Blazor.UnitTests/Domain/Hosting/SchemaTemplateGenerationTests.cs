namespace PocketBase.Blazor.UnitTests.Domain.Hosting;

using Blazor.Hosting;
using Blazor.Hosting.Interfaces;
using Blazor.Scaffolding;
using FluentAssertions;

[Trait("Category", "Unit")]
[Trait("Requires", "FileSystem")]
public class SchemaTemplateGenerationTests
{
    [Fact]
    public async Task UseSchemaTemplates_GeneratesMigrationFilesInConfiguredMigrationsDirectory()
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), $"pb_schema_templates_{Guid.NewGuid():N}");
        string dataDir = Path.Combine(tempRoot, "pb_data");
        string migrationsDir = Path.Combine(tempRoot, "pb_migrations");
        string fakeExecutablePath = Path.Combine(tempRoot, "pocketbase.exe");

        Directory.CreateDirectory(tempRoot);
        await File.WriteAllTextAsync(fakeExecutablePath, string.Empty);

        try
        {
            IPocketBaseHostBuilder builder = PocketBaseHostBuilder.CreateDefault();

            await using IPocketBaseHost host = await builder
                .UseExecutable(fakeExecutablePath)
                .UseOptions(options =>
                {
                    options.Dir = dataDir;
                    options.MigrationsDir = migrationsDir;
                    options.Dev = true;
                })
                .UseSchemaTemplate(CommonSchema.Blog)
                .UseSchemaTemplate(CommonSchema.Todo)
                .UseSchemaTemplate(CommonSchema.ECommerce)
                .BuildAsync();

            File.Exists(Path.Combine(migrationsDir, "2990000000_pb_blazor_blog_schema.js")).Should().BeTrue();
            File.Exists(Path.Combine(migrationsDir, "2990000001_pb_blazor_todo_schema.js")).Should().BeTrue();
            File.Exists(Path.Combine(migrationsDir, "2990000002_pb_blazor_ecommerce_schema.js")).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }
}
