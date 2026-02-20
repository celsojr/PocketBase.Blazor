namespace PocketBase.Blazor.IntegrationTests.Clients.Backup;

using System.Threading;
using System.Threading.Tasks;
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
        var backupName = $"test-delete-{Guid.NewGuid():N}.zip";
        await _pb.Backup.CreateAsync(backupName);

        // Verify it exists
        var listBefore = await _pb.Backup.GetFullListAsync();
        listBefore.Value.Should().Contain(b => b.Key!.Contains(backupName));

        // Act
        var result = await _pb.Backup.DeleteAsync(backupName);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify it's gone
        var listAfter = await _pb.Backup.GetFullListAsync();
        listAfter.Value.Should().NotContain(b => b.Key!.Contains(backupName));
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidName_ShouldSanitizeAndDelete()
    {
        // Arrange - Create with sanitized name
        var originalName = "Test Delete@Backup.zip";
        await _pb.Backup.CreateAsync(originalName);

        // Act - Try to delete with original (invalid) name
        var deleteResult = await _pb.Backup.DeleteAsync(originalName);

        // Assert - Should succeed due to sanitization
        deleteResult.IsSuccess.Should().BeTrue();

        // Verify backup is gone
        var listAfter = await _pb.Backup.GetFullListAsync();
        listAfter.Value.Should().NotContain(b => b.Key!.Contains("test_delete_backup"));
    }

    [Fact]
    public async Task DeleteAsync_WhenUnauthenticated_ShouldFail()
    {
        // Arrange - Create a backup first
        var backupName = $"test-unauth-delete-{Guid.NewGuid():N}";
        await _pb.Backup.CreateAsync(backupName);

        try
        {
            // Act - Try to delete with unauthenticated client
            await using var pb = new PocketBase(_pb.BaseUrl);
            var result = await pb.Backup.DeleteAsync(backupName);

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
        var backupName = $"test-cancel-delete-{Guid.NewGuid():N}";
        await _pb.Backup.CreateAsync(backupName);

        var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

        // Act
        var act = async () => await _pb.Backup.DeleteAsync(backupName, cts.Token);

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
