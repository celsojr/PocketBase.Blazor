namespace PocketBase.Blazor.UnitTests.Domain.Batch;

using System;
using Blazor.Enums;
using Blazor.Requests.Batch;
using FluentAssertions;
using Xunit;

public class BatchRequestTests
{
    [Fact]
    public void Constructor_Create_ShouldSetMethodUrlAndJson()
    {
        // Arrange & Act
        var request = new BatchRequest(
            collectionName: "test-collection",
            method: BatchMethod.Create,
            body: new { name = "test" }
        );

        // Assert
        request.Method.Should().Be("POST");
        request.Url.Should().Be("/api/collections/test-collection/records");

        request.Body.Should().NotBeNull();
        request.Body!.Value.TryGetProperty("name", out var name).Should().BeTrue();
        name.Should().NotBeNull();
        name.GetString().Should().Be("test");
    }

    [Fact]
    public void Constructor_Update_ShouldSetMethodUrlAndJson()
    {
        // Arrange & Act
        var request = new BatchRequest(
            collectionName: "test-collection",
            method: BatchMethod.Update,
            body: new { name = "updated" },
            id: "record-123"
        );

        // Assert
        request.Method.Should().Be("PATCH");
        request.Url.Should().Be("/api/collections/test-collection/records/record-123");

        request.Body.Should().NotBeNull();

        request.Body!.Value.TryGetProperty("name", out var name).Should().BeTrue();
        name.Should().NotBeNull();
        name.GetString().Should().Be("updated");
    }

    [Fact]
    public void Constructor_Delete_ShouldSetMethodAndUrl_AndHaveNoJson()
    {
        // Arrange & Act
        var request = new BatchRequest(
            collectionName: "test-collection",
            method: BatchMethod.Delete,
            body: null,
            id: "record-123"
        );

        // Assert
        request.Method.Should().Be("DELETE");
        request.Url.Should().Be("/api/collections/test-collection/records/record-123");
        request.Body.Should().BeNull();
    }

    [Fact]
    public void Constructor_Update_WithoutId_ShouldThrow()
    {
        // Act
        Action act = () => new BatchRequest(
            collectionName: "test-collection",
            method: BatchMethod.Update,
            body: new { name = "fail" }
        );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_Delete_WithoutId_ShouldThrow()
    {
        // Act
        Action act = () => new BatchRequest(
            collectionName: "test-collection",
            method: BatchMethod.Delete,
            body: null
        );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
