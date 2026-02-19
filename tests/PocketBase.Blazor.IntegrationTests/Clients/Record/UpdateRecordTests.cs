namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.IntegrationTests.Helpers;
using Blazor.Responses;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class UpdateRecordTests
{
    private readonly IPocketBase _pb;

    public UpdateRecordTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task Update_post_successfully()
    {
        // Arrange
        var firstCategoryResult = await _pb.Collection("categories")
            .CreateAsync<RecordResponse>(new
            {
                name = "Test Category 1",
                slug = "test-category-1"
            });
    
        firstCategoryResult.IsSuccess.Should().BeTrue();
    
        var anotherCategoryResult = await _pb.Collection("categories")
            .CreateAsync<RecordResponse>(new
            {
                name = "Test Category 2", 
                slug = "test-category-2"
            });
    
        anotherCategoryResult.IsSuccess.Should().BeTrue();
    
        var categoryId = firstCategoryResult.Value.Id;
        var anotherCategoryId = anotherCategoryResult.Value.Id;

        // Create the Post
        var postRequest = new PostCreateRequest
        {
            Category = categoryId,
            Slug = "test-post",
            Title = "Test Post",
            Author = "Tester",
            Content = "Initial content",
            IsPublished = false
        };

        var createResult = await _pb.Collection("posts")
            .CreateAsync<PostResponse>(postRequest);

        createResult.IsSuccess.Should().BeTrue();
        var post = createResult.Value;
        var postId = post.Id;

        // Act
        var updateRequest = new PostCreateRequest
        {
            Category = anotherCategoryId,
            Slug = $"{post.Slug}-updated",
            Title = $"{post.Title} Updated",
            Content = "Updated content",
            IsPublished = true
        };

        var updateResult = await _pb.Collection("posts")
            .UpdateAsync<PostResponse>(postId, updateRequest);

        // Assert
        updateResult.IsSuccess.Should().BeTrue();

        var updatedPost = updateResult.Value;
        updatedPost.Should().NotBeNull();
        updatedPost.Id.Should().Be(postId);
        updatedPost.Title.Should().Be($"{post.Title} Updated");
        updatedPost.Slug.Should().Be($"{post.Slug}-updated");
        updatedPost.Category.Should().Be(anotherCategoryId);
        updatedPost.IsPublished.Should().BeTrue();
        updatedPost.Content.Should().Be("Updated content");
    
        // Verify the update persisted
        var getResult = await _pb.Collection("posts")
            .GetOneAsync<PostResponse>(postId);

        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Title.Should().Be($"{post.Title} Updated");
        getResult.Value.Category.Should().Be(anotherCategoryId);
    }
}

