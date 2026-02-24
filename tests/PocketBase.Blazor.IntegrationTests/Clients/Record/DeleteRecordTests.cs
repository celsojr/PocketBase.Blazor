namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using System.Net;
using Blazor.IntegrationTests.Helpers;
using Blazor.Models;
using Blazor.Models.Collection;
using Blazor.Models.Collection.Fields;
using Blazor.Responses;

[Trait("Category", "Integration")]
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
        Result<RecordResponse> categoryResult = await _pb.Collection("categories")
            .CreateAsync<RecordResponse>(new
            {
                name = "Test Category for Delete",
                slug = "test-category-delete"
            });

        categoryResult.IsSuccess.Should().BeTrue();
        string categoryId = categoryResult.Value.Id;

        // Create a post to delete
        PostCreateRequest postRequest = new PostCreateRequest
        {
            Category = categoryId,
            Slug = "post-to-delete",
            Title = "Post to Delete",
            Author = "Tester",
            Content = "This post will be deleted",
            IsPublished = true
        };

        Result<PostResponse> createResult = await _pb.Collection("posts")
            .CreateAsync<PostResponse>(postRequest);

        createResult.IsSuccess.Should().BeTrue();
        string postId = createResult.Value.Id;

        // Act
        Result deleteResult = await _pb.Collection("posts").DeleteAsync(postId);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();

        // Verify the post no longer exists
        Result<PostResponse> getResult = await _pb.Collection("posts")
            .GetOneAsync<PostResponse>(postId);

        getResult.IsSuccess.Should().BeFalse();
        getResult.Errors.Should().NotBeEmpty();

        getResult.Errors[0].Message
            .Should().Contain("The requested resource wasn't found.");
        getResult.Errors[0].Message.Should().Contain("404");
    }

    [Fact]
    public async Task Delete_category_with_posts_should_succeed()
    {
        // Arrange - Create a category
        Result<RecordResponse> categoryResult = await _pb.Collection("categories")
            .CreateAsync<RecordResponse>(new
            {
                name = "Category with Posts",
                slug = "category-with-posts"
            });

        categoryResult.IsSuccess.Should().BeTrue();
        string categoryId = categoryResult.Value.Id;

        // Create a post in this category
        PostCreateRequest postRequest = new PostCreateRequest
        {
            Category = categoryId,
            Slug = "post-in-category",
            Title = "Post in Category",
            Author = "Tester",
            Content = "This post is in the category",
            IsPublished = true
        };

        Result<PostResponse> postResult = await _pb.Collection("posts")
            .CreateAsync<PostResponse>(postRequest);

        postResult.IsSuccess.Should().BeTrue();

        // Act - Delete the category
        Result deleteResult = await _pb.Collection("categories").DeleteAsync(categoryId);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();

        // Verify category is deleted
        Result<RecordResponse> getCategoryResult = await _pb.Collection("categories")
            .GetOneAsync<RecordResponse>(categoryId);

        getCategoryResult.IsSuccess.Should().BeFalse();
        getCategoryResult.Errors.Should().NotBeEmpty();

        getCategoryResult.Errors[0].Message
            .Should().Contain("The requested resource wasn't found.");
        getCategoryResult.Errors[0].Message.Should().Contain("404");
    }

    [Fact]
    public async Task Delete_non_existent_post_should_fail()
    {
        // Arrange
        string nonExistentId = "non-existent-id-123";

        // Act
        Result deleteResult = await _pb.Collection("posts").DeleteAsync(nonExistentId);

        // Assert
        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.Errors.Should().NotBeNull();

        ErrorResponse? error = JsonSerializer.Deserialize<ErrorResponse>(deleteResult.Errors[0].Message);

        error.Should().NotBeNull();
        error.Message.Should().Be("The requested resource wasn't found.");
        error.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_post_with_invalid_id_format_should_fail()
    {
        // Arrange
        string invalidId = "invalid-id-format";

        // Act
        Result deleteResult = await _pb.Collection("posts").DeleteAsync(invalidId);

        // Assert
        deleteResult.IsSuccess.Should().BeFalse();
        deleteResult.Errors.Should().NotBeNull();

        ErrorResponse? error = JsonSerializer.Deserialize<ErrorResponse>(deleteResult.Errors[0].Message);

        error.Should().NotBeNull();
        error.Message.Should().Be("The requested resource wasn't found.");
        error.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_multiple_posts_successfully()
    {
        // Arrange - Create a category
        Result<RecordResponse> categoryResult = await _pb.Collection("categories")
            .CreateAsync<RecordResponse>(new
            {
                name = "Category for Batch Delete",
                slug = "category-batch-delete"
            });

        categoryResult.IsSuccess.Should().BeTrue();
        string categoryId = categoryResult.Value.Id;

        // Create multiple posts
        List<string> postIds = new List<string>();
        for (int i = 1; i <= 3; i++)
        {
            PostCreateRequest postRequest = new PostCreateRequest
            {
                Category = categoryId,
                Slug = $"batch-post-{i}",
                Title = $"Batch Post {i}",
                Author = "Tester",
                Content = $"Content for batch post {i}",
                IsPublished = true
            };

            Result<PostResponse> createResult = await _pb.Collection("posts")
                .CreateAsync<PostResponse>(postRequest);

            createResult.IsSuccess.Should().BeTrue();
            postIds.Add(createResult.Value.Id);
        }

        // Act & Assert - Delete each post
        foreach (string postId in postIds)
        {
            Result deleteResult = await _pb.Collection("posts").DeleteAsync(postId);
            deleteResult.IsSuccess.Should().BeTrue();

            // Verify each post is deleted
            Result<PostResponse> getResult = await _pb.Collection("posts")
                .GetOneAsync<PostResponse>(postId);

            getResult.IsSuccess.Should().BeFalse();
            getResult.Errors.Should().NotBeEmpty();

            getResult.Errors[0].Message
                .Should().Contain("The requested resource wasn't found.");
            getResult.Errors[0].Message.Should().Contain("404");
        }
    }

    [Fact]
    public async Task Delete_parent_with_cascade_relation_should_delete_child()
    {
        // Arrange - Create parent collection
        Result<CollectionModel> parentResult = await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = "authors",
                Fields =
                {
                    new TextFieldModel { Name = "name", Required = true }
                }
            });

        string parentId = parentResult.Value.Id;

        // Create child collection with cascade relation
        Result<CollectionModel> childResult = await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = "books",
                Fields =
                {
                    new TextFieldModel { Name = "title", Required = true },
                    new RelationFieldModel
                    {
                        Name = "author",
                        CollectionId = parentId,
                        CascadeDelete = true // Enable cascade delete
                    }
                }
            });

        // Create parent record
        Result<RecordResponse> authorResult = await _pb.Collection("authors")
            .CreateAsync<RecordResponse>(new { name = "John Doe" });
        string authorId = authorResult.Value.Id;

        // Create child record linked to parent
        Result<RecordResponse> bookResult = await _pb.Collection("books")
            .CreateAsync<RecordResponse>(new
            {
                title = "Cascade Test Book",
                author = authorId
            });
        string bookId = bookResult.Value.Id;

        // Act - Delete parent (should cascade to child)
        Result deleteResult = await _pb.Collection("authors")
            .DeleteAsync(authorId);

        // Assert
        deleteResult.IsSuccess.Should().BeTrue();

        // Verify both parent and child are deleted
        Result<RecordResponse> getAuthorResult = await _pb.Collection("authors")
            .GetOneAsync<RecordResponse>(authorId);
        getAuthorResult.IsSuccess.Should().BeFalse();

        Result<RecordResponse> getBookResult = await _pb.Collection("books")
            .GetOneAsync<RecordResponse>(bookId);
        getBookResult.IsSuccess.Should().BeFalse();
    }
}
