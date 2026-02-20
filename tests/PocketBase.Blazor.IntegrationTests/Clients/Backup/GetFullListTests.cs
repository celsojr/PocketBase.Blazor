namespace PocketBase.Blazor.IntegrationTests.Clients.Backup;

using System.Linq;
using Xunit.Abstractions;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class GetFullListTests
{
    private readonly IPocketBase _pb;
    private readonly ITestOutputHelper _output;

    public GetFullListTests(PocketBaseAdminFixture fixture, ITestOutputHelper output)
    {
        _pb = fixture.Client;
        _output = output;
    }

    [Fact]
    public async Task GetFullListAsync_AsAdmin_ShouldReturnBackupList()
    {
        // Arrange - Create a backup to ensure we have something to list
        var backupName = $"test-list-{Guid.NewGuid():N}.zip";
        await _pb.Backup.CreateAsync(backupName);

        // Act
        var result = await _pb.Backup.GetFullListAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        
        _output.WriteLine($"Found {result.Value.Count} backup(s)");
        foreach (var backup in result.Value)
        {
            _output.WriteLine($"Backup: {backup.Key} - Size: {backup.Size} - Modified: {backup.Modified}");
        }

        // Cleanup
        await _pb.Backup.DeleteAsync(backupName);
    }

    [Fact]
    public async Task GetFullListAsync_AsAdmin_ShouldReturnBackupList_WithFilteredFields()
    {
        // Arrange - Create a backup to ensure we have something to list
        var backupName = $"test-fields-{Guid.NewGuid():N}.zip";
        await _pb.Backup.CreateAsync(backupName);

        // Act
        var result = await _pb.Backup.GetFullListAsync(new CommonOptions { Fields = "key,size" } );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().Contain(b => b.Key!.Contains(backupName));

        foreach (var backup in result.Value)
        {
            backup.Size.Should().BeGreaterThan(0);
            _output.WriteLine($"Backup: {backup.Key} - Size: {backup.Size}");
        }

        // Cleanup
        await _pb.Backup.DeleteAsync(backupName);
    }

    [Fact]
    public async Task GetFullListAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

        // Act
        var act = async () => await _pb.Backup.GetFullListAsync(cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetFullListAsync_WhenUnauthenticated_ShouldFail()
    {
        // Arrange
        await using var pb = new PocketBase(_pb.BaseUrl);

        // Act
        var result = await pb.Backup.GetFullListAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        
        _output.WriteLine($"Unauthenticated error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
    }
}
