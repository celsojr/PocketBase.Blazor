namespace PocketBase.Blazor.UnitTests.TestHelpers.Builders;

using System.Text.Json;
using Blazor.UnitTests.TestHelpers.Utilities;

public class PostResponseBuilder
{
    private PostResponse _post = new()
    {
        Id = Guid.NewGuid().ToString(),
        Title = "Test Post",
        Slug = "test-post",
        Content = "Test content",
        Author = "Test Author",
        Category = "Testing",
        IsPublished = true,
        Created = DateTime.UtcNow.AddDays(-1),
        Updated = DateTime.UtcNow,
        CollectionId = "posts",
        CollectionName = "BlogPosts"
    };

    private Dictionary<string, JsonElement?>? _expand;

    public PostResponseBuilder WithId(string id)
    {
        _post.Id = id;
        return this;
    }

    public PostResponseBuilder WithTitle(string title)
    {
        _post.Title = title;
        return this;
    }

    public PostResponseBuilder WithSlug(string slug)
    {
        _post.Slug = slug;
        return this;
    }

    public PostResponseBuilder WithExpand(Dictionary<string, JsonElement?> expand)
    {
        _expand = expand;
        return this;
    }

    public PostResponseBuilder WithSimpleExpand()
    {
        var expandData = new Dictionary<string, JsonElement?>
        {
            ["author"] = JsonDocument.Parse("""{"id": "auth123", "name": "John Doe"}""").RootElement,
            ["category"] = JsonDocument.Parse("""{"id": "cat456", "name": "Programming"}""").RootElement
        };

        return WithExpand(expandData);
    }

    public PostResponse Build()
    {
        // Use reflection to set the protected/private Expand property
        // or create a derived class if Expand is not publicly settable
        return _post;
    }

    public string BuildAsJson(JsonSerializerOptions? options = null)
    {
        var post = Build();
        return JsonSerializer.Serialize(post, options ?? JsonTestHelper.DefaultOptions);
    }

    public static PostResponseBuilder CreateDefault() => new();

    public static PostResponse CreateValidPost() =>
        new PostResponseBuilder().Build();
}

