namespace PocketBase.Blazor.UnitTests.Domain.Responses;

using System.Text.Json;
using Blazor.UnitTests.TestHelpers.Builders;
using Blazor.UnitTests.TestHelpers.Extensions;
using Blazor.UnitTests.TestHelpers.TestBaseClasses;
using Xunit.Abstractions;

[Trait("Category", "Unit")]
[Trait("Layer", "Domain")]
public class PostResponseTests : BaseTest
{
    public PostResponseTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Deserialize_FromCompleteJsonFile_ShouldCreateValidPostResponse()
    {
        // Arrange
        var json = await File.ReadAllTextAsync("TestData/Json/Responses/PostResponse/CompletePost.json");

        // Act
        var result = JsonSerializer.Deserialize<PostResponse>(json, JsonOptions);

        // Assert
        Assert.NotNull(result);
        result.ShouldHaveValidId();
        result.ShouldHaveValidTimestamps();
        Assert.Equal("Complete Blog Post Example", result.Title);
        Assert.Equal("complete-blog-post-example", result.Slug);
        Assert.True(result.IsPublished);

        // Verify expand data
        Assert.NotNull(result.Expand);
        Assert.Equal(3, result.Expand.Count);
        Assert.Contains("author", result.Expand.Keys);
        Assert.Contains("category", result.Expand.Keys);
        Assert.Contains("tags", result.Expand.Keys);
    }

    [Theory]
    [MemberData(nameof(GetTestDataFiles))]
    public async Task Deserialize_FromVariousTestFiles_ShouldAllSucceed(string testFile)
    {
        // Arrange
        var json = await LoadTestDataAsStringAsync(testFile);

        // Act & Assert
        json.ShouldDeserializeFromJson<PostResponse>();
    }

    [Theory]
    [MemberData(nameof(GetTestDataFiles))]
    public async Task Deserialize_FromTestFile_ShouldCreateValidPostResponse(string testFile)
    {
        // Arrange
        var json = await LoadTestDataAsStringAsync(testFile);
    
        // Act
        var result = JsonSerializer.Deserialize<PostResponse>(json, JsonOptions);
    
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.NotEmpty(result.Id);
    }

    [Theory]
    [MemberData(nameof(GetTestDataFiles))]
    public async Task LoadTestData_ShouldDeserializeToPostResponse(string testFile)
    {
        // Act
        var result = await LoadTestDataAsync<PostResponse>(testFile);
    
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
    }

    [Fact]
    public void Builder_CreateDefault_ShouldProduceValidPostResponse()
    {
        // Arrange & Act
        var post = PostResponseBuilder.CreateDefault()
            .WithTitle("Builder Test")
            .WithSlug("builder-test")
            .WithSimpleExpand()
            .Build();

        // Assert
        Assert.NotNull(post);
        Assert.Equal("Builder Test", post.Title);
        Assert.Equal("builder-test", post.Slug);
        Assert.NotNull(post.Expand);
        Assert.Equal(2, post.Expand?.Count);
    }

    [Fact]
    public void Serialize_UsingBuilder_ShouldProduceValidJson()
    {
        // Arrange
        var builder = PostResponseBuilder.CreateDefault()
            .WithTitle("Serialization Test")
            .WithId("test_123");

        // Act
        var json = builder.BuildAsJson();

        // Assert
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("title", out var titleProp));
        Assert.Equal("Serialization Test", titleProp.GetString());

        Assert.True(doc.RootElement.TryGetProperty("id", out var idProp));
        Assert.Equal("test_123", idProp.GetString());

        LogJson(doc, "Generated JSON");
    }
}

