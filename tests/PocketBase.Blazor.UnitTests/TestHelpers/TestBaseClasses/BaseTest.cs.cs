namespace PocketBase.Blazor.UnitTests.TestHelpers.TestBaseClasses;

using System.Text.Json;
using AutoFixture;
using Blazor.UnitTests.TestHelpers.Builders;
using Blazor.UnitTests.TestHelpers.Utilities;
using Bogus.DataSets;
using Blazor.UnitTests.TestHelpers.Extensions;
using Xunit.Abstractions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper Output;
    protected readonly JsonSerializerOptions JsonOptions;
    protected readonly Fixture Fixture;

    protected BaseTest(ITestOutputHelper output)
    {
        Output = output;
        JsonOptions = JsonTestHelper.DefaultOptions;
        Fixture = new Fixture();

        // Customize AutoFixture
        Fixture.Customize(new AutoFixtureCustomization());

        Initialize();
    }

    protected virtual void Initialize()
    {
        // Override in derived classes for setup
    }

    protected virtual void Cleanup()
    {
        // Override in derived classes for cleanup
    }

    protected void LogJson(object obj, string title = "Object JSON")
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Output.WriteLine($"\n{title}:\n{json}");
    }

    protected async Task<T> LoadTestDataAsync<T>(string relativePath)
    {
        var json = await LoadTestDataAsStringAsync(relativePath);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }

    protected static async Task<string> LoadTestDataAsStringAsync(string relativePath)
    {
        var fullPath = Path.Combine(TestPaths.PostResponsesDirectory, relativePath);
    
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Test data file not found: {fullPath}");
    
        return await File.ReadAllTextAsync(fullPath);
    }

    public static IEnumerable<object[]> GetTestDataFiles()
    {
        var files = new[]
        {
            "MinimalPost.json",
            "CompletePost.json",
            "WithExpand.json",
            "WithNullValues.json",
            "WithEmptyExpand.json"
        };
        
        return files.Select(f => new object[] { f });
    }

    public void Dispose()
    {
        Cleanup();
        GC.SuppressFinalize(this);
    }
}

public class AutoFixtureCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Register(() => DateTime.UtcNow);
        fixture.Register(() => Guid.NewGuid().ToString());

        // Customize PostResponse
        fixture.Customize<PostResponse>(composer => composer
            .Without(x => x.Expand)
            .With(x => x.IsPublished, true)
            .With(x => x.Slug, () => fixture.Create<Lorem>().Sentence().ToSlug()));
    }
}

