namespace PocketBase.Blazor.UnitTests.Domain.Backup;

using FluentAssertions;
using Blazor.Clients.Backup;
using Blazor.Http;

public class GetDownloadUrlTests : IAsyncLifetime
{
    private readonly HttpTransport _transport;
    private readonly BackupClient _backupClient;

    public GetDownloadUrlTests()
    {
        var baseUrl = "https://pb.example.com";
        _transport = new HttpTransport(baseUrl);
        _backupClient = new BackupClient(_transport);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _transport.Dispose();
    }

    [Fact]
    public void GetDownloadUrl_ValidInput_ShouldReturnFullUrl()
    {
        // Arrange
        var key = "test-backup-123.zip";
        var token = "abc123token";

        // Act
        var url = _backupClient.GetDownloadUrl(key, token);

        // Assert
        url.Should().Be("https://pb.example.com/api/backups/test-backup-123.zip?token=abc123token");
    }

    [Fact]
    public void GetDownloadUrl_WithInvalidName_ShouldSanitizeKey()
    {
        // Arrange
        var key = "Test Backup@2024.zip";
        var token = "token123";

        // Act
        var url = _backupClient.GetDownloadUrl(key, token);

        // Assert - Should sanitize to lowercase with underscores
        url.Should().Be("https://pb.example.com/api/backups/test_backup_2024.zip?token=token123");
    }

    [Fact]
    public void GetDownloadUrl_WithoutZipExtension_ShouldAddExtension()
    {
        // Arrange
        var key = "mybackup";
        var token = "testtoken";

        // Act
        var url = _backupClient.GetDownloadUrl(key, token);

        // Assert
        url.Should().Be("https://pb.example.com/api/backups/mybackup.zip?token=testtoken");
    }

    [Fact]
    public void GetDownloadUrl_EmptyKey_ShouldThrowArgumentException()
    {
        // Act
        var act = () => _backupClient.GetDownloadUrl("", "token");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Backup key is required.*");
    }

    [Fact]
    public void GetDownloadUrl_EmptyToken_ShouldThrowArgumentException()
    {
        // Act
        var act = () => _backupClient.GetDownloadUrl("backup.zip", "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Token is required.*");
    }

    [Fact]
    public void GetDownloadUrl_WhitespaceKey_ShouldThrowArgumentException()
    {
        // Act
        var act = () => _backupClient.GetDownloadUrl("   ", "token");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Backup key is required.*");
    }

    [Fact]
    public void GetDownloadUrl_WhitespaceToken_ShouldThrowArgumentException()
    {
        // Act
        var act = () => _backupClient.GetDownloadUrl("backup.zip", "   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Token is required.*");
    }

    [Fact]
    public void GetDownloadUrl_ShouldUrlEncodeSpecialCharacters()
    {
        // Arrange
        var key = "backup with spaces.zip";
        var token = "token#with?special&chars";

        // Act
        var url = _backupClient.GetDownloadUrl(key, token);

        // Assert - Note: key is sanitized first, then encoded
        // "backup with spaces.zip" → "backup_with_spaces.zip" → "backup_with_spaces.zip"
        // "token#with?special&chars" → URL encoded
        url.Should().Be("https://pb.example.com/api/backups/backup_with_spaces.zip?token=token%23with%3Fspecial%26chars");
    }

    [Fact]
    public void GetDownloadUrl_BaseUrlWithTrailingSlash_ShouldNotCreateDoubleSlashes()
    {
        // Arrange - Create a client with base URL that has trailing slash
        var baseUrl = "https://pb.example.com/";
        var transport = new HttpTransport(baseUrl);
        var backupClient = new BackupClient(transport);

        var key = "backup.zip";
        var token = "token123";

        // Act
        var url = backupClient.GetDownloadUrl(key, token);

        // Assert - Should not have double slashes
        url.Should().NotContain("//api");
        url.Should().Be("https://pb.example.com/api/backups/backup.zip?token=token123");

        // Cleanup
        transport.Dispose();
    }
}
