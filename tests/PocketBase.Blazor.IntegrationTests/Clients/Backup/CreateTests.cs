namespace PocketBase.Blazor.IntegrationTests.Clients.Backup;

using System.Threading;
using System.Threading.Tasks;
using Xunit;

[Collection("PocketBase.Blazor.Admin")]
public class CreateTests
{
    private readonly IPocketBase _pb;

    public CreateTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task CreateAsync_WithName_ShouldCreateBackup()
    {
        // Arrange - Must be in the format [a-z0-9_-].zip
        var backupName = $"test-backup-{Guid.NewGuid():N}";

        // Act
        var result = await _pb.Backup.CreateAsync(backupName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify backup exists in list
        var listResult = await _pb.Backup.GetFullListAsync();
        listResult.Value.Should().Contain(b => b.Key.Contains(backupName));

        // Cleanup
        await _pb.Backup.DeleteAsync(backupName);
    }

    [Fact]
    public async Task CreateAsync_WithoutName_ShouldCreateAutomaticBackup()
    {
        // Act
        var result = await _pb.Backup.CreateAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Should create a backup with default name pattern
        var listResult = await _pb.Backup.GetFullListAsync();
        listResult.Value.Should().NotBeEmpty();
        
        // Find and clean up the created backup
        var latestBackup = listResult.Value.OrderByDescending(b => b.Modified).First();
        latestBackup.Key.Should().NotBeNull();
        await _pb.Backup.DeleteAsync(latestBackup.Key);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ShouldFail()
    {
        // Arrange
        var backupName = $"duplicate-test-{Guid.NewGuid():N}";
        await _pb.Backup.CreateAsync(backupName);

        try
        {
            // Act - Try to create with same name
            var result = await _pb.Backup.CreateAsync(backupName);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }
        finally
        {
            // Cleanup
            await _pb.Backup.DeleteAsync(backupName);
        }
    }

    [Fact]
    public async Task CreateAsync_WhenUnauthenticated_ShouldFail()
    {
        // Arrange
        await using var pb = new PocketBase(_pb.BaseUrl);

        // Act
        var result = await pb.Backup.CreateAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

        // Act
        var act = async () => await _pb.Backup.CreateAsync(cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task CreateAsync_WithInvalidName_ShouldNotFail()
    {
        // Arrange - Try invalid characters without zip extension
        var invalidName = "invalid/name\\with*chars";

        // Act
        var result = await _pb.Backup.CreateAsync(invalidName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();

        // Verify backup exists in list with sanitized name
        var listResult = await _pb.Backup.GetFullListAsync();
        listResult.Value.Should().Contain(b => b.Key.Contains("invalid_name_with_chars"));

        // Cleanup
        await _pb.Backup.DeleteAsync("invalid_name_with_chars.zip");
    }

    [Fact]
    public async Task CreateAsync_ThenDelete_ShouldWork()
    {
        // Arrange
        var backupName = $"create-delete-test-{Guid.NewGuid():N}";

        // Act - Create
        var createResult = await _pb.Backup.CreateAsync(backupName);
        createResult.IsSuccess.Should().BeTrue();

        // Verify exists
        var listBefore = await _pb.Backup.GetFullListAsync();
        listBefore.Value.Should().Contain(b => b.Key.Contains(backupName));

        // Act - Delete
        var deleteResult = await _pb.Backup.DeleteAsync(backupName);
        deleteResult.IsSuccess.Should().BeTrue();

        // Verify removed
        var listAfter = await _pb.Backup.GetFullListAsync();
        listAfter.Value.Should().NotContain(b => b.Key.Contains(backupName));
    }
}
