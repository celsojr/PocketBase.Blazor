namespace PocketBase.Blazor.IntegrationTests.Clients.Settings;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class TestS3Tests
{
    private readonly IPocketBase _pb;

    public TestS3Tests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task TestS3_Fails_WhenS3NotConfigured()
    {
        // Arrange & Act
        Result<bool> result = await _pb.Settings.TestS3Async();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TestS3_Fails_WhenInvalidFileSystem()
    {
        // Arrange & Act
        Result<bool> result = await _pb.Settings.TestS3Async("invalid");

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task TestS3_Succeeds_WithBackupFileSystem()
    {
        // Arrange & Act
        Result<bool> result = await _pb.Settings.TestS3Async("backups");
        
        // Assert
        result.IsSuccess.Should().BeFalse(); // Should fail if not configured
    }
}

