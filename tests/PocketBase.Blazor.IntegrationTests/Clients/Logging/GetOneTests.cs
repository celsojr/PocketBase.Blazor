namespace PocketBase.Blazor.IntegrationTests.Clients.Logging;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class GetOneTests
{
    private readonly IPocketBase _pb;

    public GetOneTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task GetOneAsync_ReturnsLog_WhenValidId()
    {
        // Arrange - get an existing log id
        var listResult = await _pb.Log.GetListAsync(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });
        listResult.IsSuccess.Should().BeTrue();
        var logId = listResult.Value.Items.First().Id;

        // Act
        var result = await _pb.Log.GetOneAsync(logId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(logId);
        result.Value.Message.Should().NotBeNullOrEmpty();
        result.Value.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOneAsync_ReturnsNotFound_WhenInvalidId()
    {
        // Arrange
        var invalidId = "non_existent_log_123";

        // Act
        var result = await _pb.Log.GetOneAsync(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors[0].Should().NotBeNull();
        result.Errors[0].Message.Should().Contain("404");
    }

    [Fact]
    public async Task GetOneAsync_ReturnsLogWithOptions_WhenFieldsSpecified()
    {
        // Arrange
        var listResult = await _pb.Log.GetListAsync(perPage: 1);
        listResult.IsSuccess.Should().BeTrue();
        var logId = listResult.Value.Items.First().Id;

        var options = new CommonOptions
        {
            Fields = "id,message,level"
        };

        // Act
        var result = await _pb.Log.GetOneAsync(logId!, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(logId);
        result.Value.Message.Should().NotBeNullOrEmpty();
        // Note: When using fields filter, other properties might be null/default
    }

    [Fact]
    public async Task GetOneAsync_ThrowsArgumentException_WhenIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _pb.Log.GetOneAsync(""));
    }

    [Fact]
    public async Task GetOneAsync_ThrowsArgumentException_WhenIdIsWhitespace()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _pb.Log.GetOneAsync("   "));
    }

    [Fact]
    public async Task GetOneAsync_ThrowsArgumentNullException_WhenIdIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _pb.Log.GetOneAsync(null!));
    }
}
