namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using System.Net;
using Blazor.IntegrationTests.Helpers;
using Blazor.Responses;

[Collection("PocketBase.Blazor.Admin")]
public class DeleteRecordTests
{
    private readonly IPocketBase _pb;

    public DeleteRecordTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task Delete_post_successfully()
    {
        // Arrange - Create a category
        var categoryResult = await _pb.Collection("categories")
            .CreateAsync<RecordResponse>(new
            {
                name = "Test Category for Delete",
                slug = "test-category-delete"
            });

        categoryResult.IsSuccess.Should().BeTrue();
        var categoryId = categoryResult.Value.Id;

        // Create a post to delete
        var postRequest = new PostCreateRequest
        {
            Category = categoryId,
            Slug = "post-to-delete",
            Title = "Post to Delete",
            Author = "Tester",
            Content = "This post will be deleted",
            IsPublished = true
        };

        var createResult = await _pb.Collection("posts")
            .CreateAsync<PostResponse>(postRequest);

        createResult.IsSuccess.Should().BeTrue();
        var postId = createResult.Value.Id;

        // Act
        var deleteResult = await _pb.Collection("posts").DeleteAsync(postId);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();

        // Verify the post no longer exists
        var getResult = await _pb.Collection("posts")
            .GetOneAsync<PostResponse>(postId);

        getResult.IsSuccess.Should().BeFalse();
        //getResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_category_with_posts_should_succeed()
    {
        // Arrange - Create a category
        var categoryResult = await _pb.Collection("categories")
            .CreateAsync<RecordResponse>(new
            {
                name = "Category with Posts",
                slug = "category-with-posts"
            });

        categoryResult.IsSuccess.Should().BeTrue();
        var categoryId = categoryResult.Value.Id;

        // Create a post in this category
        var postRequest = new PostCreateRequest
        {
            Category = categoryId,
            Slug = "post-in-category",
            Title = "Post in Category",
            Author = "Tester",
            Content = "This post is in the category",
            IsPublished = true
        };

        var postResult = await _pb.Collection("posts")
            .CreateAsync<PostResponse>(postRequest);

        postResult.IsSuccess.Should().BeTrue();

        // Act - Delete the category
        var deleteResult = await _pb.Collection("categories").DeleteAsync(categoryId);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();

        // Verify category is deleted
        var getCategoryResult = await _pb.Collection("categories")
            .GetOneAsync<RecordResponse>(categoryId);

        getCategoryResult.IsSuccess.Should().BeFalse();
        //getCategoryResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_non_existent_post_should_fail()
    {
        // Arrange
        var nonExistentId = "non-existent-id-123";

        // Act
        var deleteResult = await _pb.Collection("posts").DeleteAsync(nonExistentId);

        // Assert
        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.Errors.Should().NotBeNull();

        var error = JsonSerializer.Deserialize<ErrorResponse>(deleteResult.Errors[0].Message);

        error.Should().NotBeNull();
        error.Message.Should().Be("The requested resource wasn't found.");
        error.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_post_with_invalid_id_format_should_fail()
    {
        // Arrange
        var invalidId = "invalid-id-format";

        // Act
        var deleteResult = await _pb.Collection("posts").DeleteAsync(invalidId);

        // Assert
        deleteResult.IsSuccess.Should().BeFalse();
        // Might return BadRequest or NotFound depending on PocketBase validation
        //deleteResult.StatusCode.Should()
        //    .BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_multiple_posts_successfully()
    {
        // Arrange - Create a category
        var categoryResult = await _pb.Collection("categories")
            .CreateAsync<RecordResponse>(new
            {
                name = "Category for Batch Delete",
                slug = "category-batch-delete"
            });

        categoryResult.IsSuccess.Should().BeTrue();
        var categoryId = categoryResult.Value.Id;

        // Create multiple posts
        var postIds = new List<string>();
        for (int i = 1; i <= 3; i++)
        {
            var postRequest = new PostCreateRequest
            {
                Category = categoryId,
                Slug = $"batch-post-{i}",
                Title = $"Batch Post {i}",
                Author = "Tester",
                Content = $"Content for batch post {i}",
                IsPublished = true
            };

            var createResult = await _pb.Collection("posts")
                .CreateAsync<PostResponse>(postRequest);

            createResult.IsSuccess.Should().BeTrue();
            postIds.Add(createResult.Value.Id);
        }

        // Act & Assert - Delete each post
        foreach (var postId in postIds)
        {
            var deleteResult = await _pb.Collection("posts").DeleteAsync(postId);
            deleteResult.IsSuccess.Should().BeTrue();

            // Verify each post is deleted
            var getResult = await _pb.Collection("posts")
                .GetOneAsync<PostResponse>(postId);

            getResult.IsSuccess.Should().BeFalse();
            //getResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

