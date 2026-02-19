namespace PocketBase.Blazor.IntegrationTests.Clients.Backup;

using System.Threading;
using System.Threading.Tasks;
using Blazor.Models;
using Blazor.Responses;
using Xunit;
using Xunit.Abstractions;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class RestoreTests
{
    private readonly IPocketBase _pb;
    private readonly ITestOutputHelper _output;

    public RestoreTests(PocketBaseAdminFixture fixture, ITestOutputHelper output)
    {
        _pb = fixture.Client;
        _output = output;
    }

    [Fact]
    public async Task RestoreAsync_ExistingBackup_ShouldSucceed()
    {
        // Skip on Windows since PocketBase doesn't support restore on Windows
        // Restoring should return 204 No Content, but actually fails internally
        if (OperatingSystem.IsWindows())
        {
            _output.WriteLine("Skipping restore test on Windows");
            _output.WriteLine("Reason: PocketBase doesn't support restore on Windows");
            return;
        }

        // Arrange - Create a backup
        var backupName = $"restore-test-{Guid.NewGuid():N}.zip";
        await _pb.Backup.CreateAsync(backupName);

        await Task.Delay(2000); // Wait for backup

        try
        {
            // Act
            var result = await _pb.Backup.RestoreAsync(backupName);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _output.WriteLine($"Restored backup: {backupName}");
        }
        finally
        {
            // Cleanup
            await _pb.Backup.DeleteAsync(backupName);
        }
    }

    [Fact]
    public async Task RestoreAsync_NonExistentBackup_ShouldFail()
    {
        // Arrange
        var nonExistent = $"non-existent-{Guid.NewGuid():N}.zip";

        // Act
        var result = await _pb.Backup.RestoreAsync(nonExistent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RestoreAsync_WhenUnauthenticated_ShouldFail()
    {
        // Arrange - Create a backup first
        var backupName = $"restore-unauth-{Guid.NewGuid():N}.zip";
        await _pb.Backup.CreateAsync(backupName);

        await Task.Delay(2000);

        try
        {
            // Act - Try to restore with unauthenticated client
            await using var pb = new PocketBase(_pb.BaseUrl);
            var result = await pb.Backup.RestoreAsync(backupName);

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
    public async Task RestoreAsync_WithInvalidName_ShouldSanitizeAndRestore()
    {
        // Skip on Windows since PocketBase doesn't support restore on Windows
        // Restoring should return 204 No Content, but actually fails internally
        if (OperatingSystem.IsWindows())
        {
            _output.WriteLine("Skipping restore test on Windows");
            _output.WriteLine("Reason: PocketBase doesn't support restore on Windows");
            return;
        }

        // Arrange - Create a backup with sanitized name
        var originalName = "Test Restore@Backup.zip";
        await _pb.Backup.CreateAsync(originalName);

        await Task.Delay(2000);

        try
        {
            // Act - Try to restore with original (invalid) name
            var result = await _pb.Backup.RestoreAsync(originalName);

            // Assert - Should succeed due to sanitization
            result.IsSuccess.Should().BeTrue();
        }
        finally
        {
            // Cleanup - Use sanitized name
            await _pb.Backup.DeleteAsync("test_restore_backup.zip");
        }
    }

    [Fact]
    public async Task RestoreAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Skip on Windows since PocketBase doesn't support restore on Windows
        // Restoring should return 204 No Content, but actually fails internally
        if (OperatingSystem.IsWindows())
        {
            _output.WriteLine("Skipping restore test on Windows");
            _output.WriteLine("Reason: PocketBase doesn't support restore on Windows");
            return;
        }

        // Arrange - Create a backup first
        var backupName = $"restore-cancel-{Guid.NewGuid():N}";
        await _pb.Backup.CreateAsync(backupName);

        await Task.Delay(2000);

        var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

        try
        {
            // Act
            var act = async () => await _pb.Backup.RestoreAsync(backupName, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
        finally
        {
            // Cleanup
            await _pb.Backup.DeleteAsync(backupName);
        }
    }

    [Fact]
    public async Task RestoreAsync_EmptyKey_ShouldThrowArgumentException()
    {
        // Act
        var act = async () => await _pb.Backup.RestoreAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RestoreAsync_WhitespaceKey_ShouldThrowArgumentException()
    {
        // Act
        var act = async () => await _pb.Backup.RestoreAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact(Skip = "This is a destructive test")]
    public async Task RestoreAsync_ShouldActuallyRestoreData()
    {
        // Skip on Windows since PocketBase doesn't support restore on Windows
        // Restoring should return 204 No Content, but actually fails internally
        if (OperatingSystem.IsWindows())
        {
            _output.WriteLine("Skipping restore test on Windows");
            _output.WriteLine("Reason: PocketBase doesn't support restore on Windows");
            return;
        }

        // WARNING: This is a destructive test - only run in isolated environment

        // 1. Create test data
        var collectionName = $"restore_test_{Guid.NewGuid():N}"[..20];
        var result = await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = collectionName,
            type = "base",
            fields = new object[] { new { name = "title", type = "text" } }
        });

        // 2. Create a record
        await _pb.Collection(collectionName)
            .CreateAsync<RecordResponse>(new { title = "Test Record" });

        // 3. Create backup
        var backupName = $"integration-restore-{Guid.NewGuid():N}.zip";
        await _pb.Backup.CreateAsync(backupName);
        await Task.Delay(2000);

        try
        {
            // 4. Delete the collection (simulate data loss)
            await _pb.Collections.DeleteAsync(collectionName);

            // 5. Restore backup
            var restoreResult = await _pb.Backup.RestoreAsync(backupName);
            restoreResult.IsSuccess.Should().BeTrue();

            // 6. Verify collection was restored
            await Task.Delay(2000); // Wait for restore
            var collections = await _pb.Collections.GetListAsync<CollectionModel>();
            collections.Value.Items.Should().Contain(c => c.Name == collectionName);
        }
        finally
        {
            // Cleanup
            try { await _pb.Collections.DeleteAsync(collectionName); } catch { }
            await _pb.Backup.DeleteAsync(backupName);
        }
    }
}
