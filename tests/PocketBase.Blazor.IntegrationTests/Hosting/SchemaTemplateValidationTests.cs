namespace PocketBase.Blazor.IntegrationTests.Hosting;

using Blazor.Hosting;
using Blazor.Hosting.Interfaces;
using Blazor.Scaffolding;
using FluentAssertions;

[Trait("Category", "Integration")]
[Trait("Requires", "FileSystem")]
[Trait("Requires", "Pocketbase")]
public class SchemaTemplateValidationTests
{
    [Fact]
    public async Task UseSchemaTemplate_EachTemplateStartsPocketBaseSuccessfully()
    {
        CommonSchema[] schemas =
        [
            CommonSchema.Blog,
            CommonSchema.Todo,
            CommonSchema.ECommerce
        ];

        foreach (CommonSchema schema in schemas)
        {
            await ValidateSchemaAsync(schema);
        }
    }

    private static async Task ValidateSchemaAsync(CommonSchema schema)
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), $"pb_schema_validation_{schema}_{Guid.NewGuid():N}");
        string dataDir = Path.Combine(tempRoot, "pb_data");
        string migrationsDir = Path.Combine(tempRoot, "pb_migrations");

        Directory.CreateDirectory(tempRoot);

        IPocketBaseHost? host = null;
        try
        {
            IPocketBaseHostBuilder builder = PocketBaseHostBuilder.CreateDefault();

            host = await builder
                .UseOptions(options =>
                {
                    options.Dir = dataDir;
                    options.MigrationsDir = migrationsDir;
                    options.Dev = true;
                })
                .UseSchemaTemplate(schema)
                .BuildAsync();

            string expectedMigrationFile = schema switch
            {
                CommonSchema.Blog => "2990000000_pb_blazor_blog_schema.js",
                CommonSchema.Todo => "2990000001_pb_blazor_todo_schema.js",
                CommonSchema.ECommerce => "2990000002_pb_blazor_ecommerce_schema.js",
                _ => throw new NotSupportedException($"Schema {schema} is not supported in this test.")
            };

            File.Exists(Path.Combine(migrationsDir, expectedMigrationFile))
                .Should().BeTrue($"migration file for {schema} should be generated before host startup.");

            using HttpClient client = new HttpClient();

            bool preExistingHealth = await WaitForHealthAsync(client, attempts: 1);
            preExistingHealth.Should().BeFalse(
                "A PocketBase instance is already responding on http://127.0.0.1:8090. " +
                "Stop it before running schema validation tests to avoid false positives.");

            await host.StartAsync();

            host.Process.Should().NotBeNull();
            await Task.Delay(1500);
            host.Process!.HasExited.Should().BeFalse($"PocketBase process should stay running for schema {schema}.");

            bool healthOk = await WaitForHealthAsync(client);
            healthOk.Should().BeTrue($"PocketBase /api/health should be reachable for schema {schema}.");
        }
        finally
        {
            if (host != null)
            {
                await host.DisposeAsync();
            }

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private static async Task<bool> WaitForHealthAsync(HttpClient client, int attempts = 10)
    {
        for (int i = 0; i < attempts; i++)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:8090/api/health");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch
            {
                // ignore transient startup failures while polling
            }

            await Task.Delay(500);
        }

        return false;
    }
}
