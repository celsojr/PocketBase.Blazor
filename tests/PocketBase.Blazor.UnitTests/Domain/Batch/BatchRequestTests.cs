namespace PocketBase.Blazor.UnitTests.Domain.Batch;

using Blazor.Enums;
using Blazor.Requests;
using FluentAssertions;

public class BatchRequestTests
{
    [Fact]
    public void BatchRequest_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var request = new BatchRequest("test-collection", BatchMethod.Create, new { name = "test" });

        // Assert
        request.Method.Should().Be("POST");
        request.Url.Should().Be("api/collections/test-collection/records");
        request.Json.Should().NotBeNull();
        request.Json!["data"].GetProperty("name").GetString().Should().Be("test");
    }

    [Fact]
    public void BatchRequest_Constructor_WithUpdate_ShouldIncludeId()
    {
        // Arrange & Act
        var request = new BatchRequest("test-collection", BatchMethod.Update, new { name = "updated" }, "record-123");

        // Assert
        request.Method.Should().Be("PATCH");
        request.Url.Should().Be("api/collections/test-collection/records/record-123");
    }

    [Fact]
    public void BatchRequest_Constructor_WithDelete_ShouldNotRequireBody()
    {
        // Arrange & Act
        var request = new BatchRequest("test-collection", BatchMethod.Delete, null, "record-123");

        // Assert
        request.Method.Should().Be("DELETE");
        request.Url.Should().Be("api/collections/test-collection/records/record-123");
        request.Json.Should().BeNull();
    }

    [Fact]
    public void BatchRequest_Constructor_WithoutIdForUpdate_ShouldThrow()
    {
        // Act
        var act = () => new BatchRequest("test-collection", BatchMethod.Update, new { });

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BatchRequest_Constructor_WithoutIdForDelete_ShouldThrow()
    {
        // Act
        var act = () => new BatchRequest("test-collection", BatchMethod.Delete, null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
