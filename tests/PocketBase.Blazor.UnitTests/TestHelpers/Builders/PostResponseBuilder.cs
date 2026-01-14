namespace PocketBase.Blazor.UnitTests.TestHelpers.Builders;

using System.Text.Json;
using Blazor.UnitTests.TestHelpers.Utilities;
using Blazor.Responses;

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
        // Create a new PostResponse with the expand data
        // Since Expand property is init-only, we need to use reflection or create a new instance
        var post = Build(); // Get current post

        // Use reflection to set the Expand property
        var expandProperty = typeof(RecordResponse).GetProperty("Expand",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (expandProperty != null && expandProperty.CanWrite)
        {
            expandProperty.SetValue(post, expand);
        }
        else
        {
            // If property doesn't have a setter, we need to create a new PostResponse
            // with the expand data using object initializer syntax
            throw new InvalidOperationException("Expand property is not writable. Consider creating a new PostResponse instance.");
        }

        return this;
    }

    public PostResponseBuilder WithSimpleExpand()
    {
        _expand = new Dictionary<string, JsonElement?>
        {
            ["author"] = JsonSerializer.SerializeToElement(new { id = "auth123", name = "John Doe" }),
            ["category"] = JsonSerializer.SerializeToElement(new { id = "cat456", name = "Programming" })
        };

        return WithExpand(_expand);
    }

    public PostResponse Build()
    {
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

