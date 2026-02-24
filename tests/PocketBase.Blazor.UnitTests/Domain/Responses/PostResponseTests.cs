namespace PocketBase.Blazor.UnitTests.Domain.Responses;

using System.Text.Json;
using Blazor.UnitTests.TestHelpers.Builders;
using Blazor.UnitTests.TestHelpers.Extensions;
using Blazor.UnitTests.TestHelpers.TestBaseClasses;
using Blazor.UnitTests.TestHelpers.Utilities;
using FluentAssertions;
using Xunit.Abstractions;

[Trait("Category", "Unit")]
[Trait("Requires", "FileSystem")]
public class PostResponseTests : BaseTest
{
    public PostResponseTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Deserialize_FromCompleteJsonFile_ShouldCreateValidPostResponse()
    {
        // Arrange
        string completePost = Path.Combine(TestPaths.PostResponsesDirectory, "CompletePost.json");
        string json = await File.ReadAllTextAsync(completePost);

        // Act
        PostResponse? result = JsonSerializer.Deserialize<PostResponse>(json, JsonOptions);

        // Assert
        result.Should().NotBeNull();
        result.ShouldHaveValidId();
        result.ShouldHaveValidTimestamps();
        result.Title.Should().Be("Complete Blog Post Example");
        result.Slug.Should().Be("complete-blog-post-example");
        result.IsPublished.Should().BeTrue();

        // Verify expand data
        result.Expand.Should().NotBeNull();
        result.Expand.Should().HaveCount(3);
        result.Expand.Should().ContainKey("author");
        result.Expand.Should().ContainKey("category");
        result.Expand.Should().ContainKey("tags");
    }

    [Theory]
    [MemberData(nameof(GetTestDataFiles))]
    public async Task Deserialize_FromVariousTestFiles_ShouldAllSucceed(string testFile)
    {
        // Arrange
        string json = await LoadTestDataAsStringAsync(testFile);

        // Act & Assert
        json.ShouldDeserializeFromJson<PostResponse>();
    }

    [Theory]
    [MemberData(nameof(GetTestDataFiles))]
    public async Task Deserialize_FromTestFile_ShouldCreateValidPostResponse(string testFile)
    {
        // Arrange
        string json = await LoadTestDataAsStringAsync(testFile);

        // Act
        PostResponse? result = JsonSerializer.Deserialize<PostResponse>(json, JsonOptions);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(GetTestDataFiles))]
    public async Task LoadTestData_ShouldDeserializeToPostResponse(string testFile)
    {
        // Act
        PostResponse result = await LoadTestDataAsync<PostResponse>(testFile);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Builder_CreateDefault_ShouldProduceValidPostResponse()
    {
        // Arrange & Act
        PostResponse post = PostResponseBuilder.CreateDefault()
            .WithTitle("Builder Test")
            .WithSlug("builder-test")
            .WithSimpleExpand()
            .Build();

        // Assert
        post.Should().NotBeNull();
        post.Title.Should().Be("Builder Test");
        post.Slug.Should().Be("builder-test");
        post.Expand.Should().NotBeNull();
        post.Expand.Should().HaveCount(2);
    }

    [Fact]
    public void Serialize_UsingBuilder_ShouldProduceValidJson()
    {
        // Arrange
        PostResponseBuilder builder = PostResponseBuilder.CreateDefault()
            .WithTitle("Serialization Test")
            .WithId("test_123");

        // Act
        string json = builder.BuildAsJson();

        // Assert
        JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("title", out JsonElement titleProp).Should().BeTrue();
        titleProp.GetString().Should().Be("Serialization Test");

        doc.RootElement.TryGetProperty("id", out JsonElement idProp).Should().BeTrue();
        idProp.GetString().Should().Be("test_123");

        LogJson(doc, "Generated JSON");
    }

    [Fact]
    public async Task PostResponse_ExpandedTags_ShouldDeserializeTagsFromExpand()
    {
        // Arrange
        string completePost = Path.Combine(TestPaths.PostResponsesDirectory, "CompletePost.json");
        string json = await File.ReadAllTextAsync(completePost);
        PostResponse? post = JsonSerializer.Deserialize<PostResponse>(json, JsonOptions);

        // Act
        List<TagResponse>? expandedTags = post?.ExpandedTags;

        // Assert
        post.Should().NotBeNull();
        expandedTags.Should().NotBeNull();
        expandedTags.Should().HaveCount(3);
    
        // Verify individual tag properties
        expandedTags[0].Id.Should().Be("tag_1");
        expandedTags[0].Name.Should().Be("C#");
    
        expandedTags[1].Id.Should().Be("tag_2");
        expandedTags[1].Name.Should().Be(".NET");
    
        expandedTags[2].Id.Should().Be("tag_3");
        expandedTags[2].Name.Should().Be("Testing");
    }

    [Fact]
    public void PostResponse_ExpandedTags_ShouldReturnEmptyListWhenNoTagsExpand()
    {
        // Arrange - JSON without tags in expand
        string json = @"
        {
            ""id"": ""post_123"",
            ""title"": ""Test Post"",
            ""expand"": {
                ""author"": {
                    ""id"": ""user_1"",
                    ""name"": ""Author Name""
                }
            }
        }";

        PostResponse? post = JsonSerializer.Deserialize<PostResponse>(json, JsonOptions);

        // Act
        List<TagResponse>? expandedTags = post?.ExpandedTags;

        // Assert
        expandedTags.Should().NotBeNull();
        expandedTags.Should().BeEmpty();
        expandedTags.Should().HaveCount(0);
    }

    [Fact]
    public void PostResponse_ExpandedTags_ShouldReturnEmptyListWhenExpandIsNull()
    {
        // Arrange - JSON without any expand
        string json = @"
        {
            ""id"": ""post_123"",
            ""title"": ""Test Post""
        }";

        PostResponse? post = JsonSerializer.Deserialize<PostResponse>(json, JsonOptions);

        // Act
        List<TagResponse>? expandedTags = post?.ExpandedTags;

        // Assert
        expandedTags.Should().NotBeNull();
        expandedTags.Should().BeEmpty();
    }
}

