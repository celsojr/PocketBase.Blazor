namespace PocketBase.Blazor.IntegrationTests.Clients.Backup;

using System.Threading;
using System.Threading.Tasks;
using Blazor.Responses.Backup;
using Xunit;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class DeleteTests
{
    private readonly IPocketBase _pb;

    public DeleteTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task DeleteAsync_ExistingBackup_ShouldSucceed()
    {
        // Arrange - Create a backup
        string backupName = $"test-delete-{Guid.NewGuid():N}.zip";
        await _pb.Backup.CreateAsync(backupName);

        // Verify it exists
        Result<List<BackupInfoResponse>> listBefore = await _pb.Backup.GetFullListAsync();
        listBefore.Value.Should().Contain(b => b.Key!.Contains(backupName));

        // Act
        Result result = await _pb.Backup.DeleteAsync(backupName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify it's gone
        Result<List<BackupInfoResponse>> listAfter = await _pb.Backup.GetFullListAsync();
        listAfter.Value.Should().NotContain(b => b.Key!.Contains(backupName));
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidName_ShouldSanitizeAndDelete()
    {
        // Arrange - Create with sanitized name
        string originalName = "Test Delete@Backup.zip";
        await _pb.Backup.CreateAsync(originalName);

        // Act - Try to delete with original (invalid) name
        Result deleteResult = await _pb.Backup.DeleteAsync(originalName);

        // Assert - Should succeed due to sanitization
        deleteResult.IsSuccess.Should().BeTrue();

        // Verify backup is gone
        Result<List<BackupInfoResponse>> listAfter = await _pb.Backup.GetFullListAsync();
        listAfter.Value.Should().NotContain(b => b.Key!.Contains("test_delete_backup"));
    }

    [Fact]
    public async Task DeleteAsync_WhenUnauthenticated_ShouldFail()
    {
        // Arrange - Create a backup first
        string backupName = $"test-unauth-delete-{Guid.NewGuid():N}";
        await _pb.Backup.CreateAsync(backupName);

        try
        {
            // Act - Try to delete with unauthenticated client
            await using PocketBase pb = new PocketBase(_pb.BaseUrl);
            Result result = await pb.Backup.DeleteAsync(backupName);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }
        finally
        {
            // Cleanup with admin client
            await _pb.Backup.DeleteAsync(backupName);
        }
    }

    [Fact]
    public async Task DeleteAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange - Create a backup first
        string backupName = $"test-cancel-delete-{Guid.NewGuid():N}";
        await _pb.Backup.CreateAsync(backupName);

        CancellationTokenSource cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

        // Act
        Func<Task<Result>> act = async () => await _pb.Backup.DeleteAsync(backupName, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();

        // Cleanup (if not cancelled)
        try
        {
            await _pb.Backup.DeleteAsync(backupName);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
