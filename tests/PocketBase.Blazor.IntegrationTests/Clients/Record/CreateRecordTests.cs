using PocketBase.Blazor.IntegrationTests.Helpers;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

[Collection("PocketBase.Blazor.Admin")]
public class CreateRecordTests
{
    private readonly IPocketBase _pb;

    public CreateRecordTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task Create_post_with_category_successfully()
    {
        // Arrange

        // 15 characters string to store as record ID.
        // If not set, it will be auto generated.
        string categoryId = $"{Guid.NewGuid():N}"[..15];

        var categoryResult = await _pb.Collection("categories")
            .CreateAsync<RecordResponse>(new
            {
                id = categoryId,
                name = "Testing Posts",
                slug = "testing-posts",
            });

        categoryResult.IsSuccess.Should().BeTrue();
        categoryResult.Value.Id.Should().Be(categoryId);

        // Act
        var postRequest = new PostCreateRequest
        {
            Category = categoryResult.Value.Id, // Or categoryId
            Slug = "eleven-post",
            Title = "Eleven Post",
            Author = "Tester",
            Content = "Hello <strong>HTML</strong> content!",
            IsPublished = true
        };

        var postResult = await _pb.Collection("posts")
            .CreateAsync<PostResponse>(postRequest);

        // Assert
        postResult.IsSuccess.Should().BeTrue();

        var post = postResult.Value;
        post.Should().NotBeNull();
        post.Title.Should().Be("Eleven Post");
        post.Category.Should().Be(categoryId);
        post.IsPublished.Should().BeTrue();
    }

}
