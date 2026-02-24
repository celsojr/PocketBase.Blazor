namespace PocketBase.Blazor.UnitTests.Domain.Backup;

using FluentAssertions;
using Blazor.Clients.Backup;
using Blazor.Http;

[Trait("Category", "Unit")]
public class GetDownloadUrlTests : IAsyncLifetime
{
    private readonly HttpTransport _transport;
    private readonly BackupClient _backupClient;

    public GetDownloadUrlTests()
    {
        string baseUrl = "https://pb.example.com";
        _transport = new HttpTransport(baseUrl);
        _backupClient = new BackupClient(_transport);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
        _transport.Dispose();
    }

    [Fact]
    public void GetDownloadUrl_ValidInput_ShouldReturnFullUrl()
    {
        // Arrange
        string key = "test-backup-123.zip";
        string token = "abc123token";

        // Act
        string url = _backupClient.GetDownloadUrl(key, token);

        // Assert
        url.Should().Be("https://pb.example.com/api/backups/test-backup-123.zip?token=abc123token");
    }

    [Fact]
    public void GetDownloadUrl_WithInvalidName_ShouldSanitizeKey()
    {
        // Arrange
        string key = "Test Backup@2024.zip";
        string token = "token123";

        // Act
        string url = _backupClient.GetDownloadUrl(key, token);

        // Assert - Should sanitize to lowercase with underscores
        url.Should().Be("https://pb.example.com/api/backups/test_backup_2024.zip?token=token123");
    }

    [Fact]
    public void GetDownloadUrl_WithoutZipExtension_ShouldAddExtension()
    {
        // Arrange
        string key = "mybackup";
        string token = "testtoken";

        // Act
        string url = _backupClient.GetDownloadUrl(key, token);

        // Assert
        url.Should().Be("https://pb.example.com/api/backups/mybackup.zip?token=testtoken");
    }

    [Fact]
    public void GetDownloadUrl_EmptyKey_ShouldThrowArgumentException()
    {
        // Act
        Func<string> act = () => _backupClient.GetDownloadUrl("", "token");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Backup key is required.*");
    }

    [Fact]
    public void GetDownloadUrl_EmptyToken_ShouldThrowArgumentException()
    {
        // Act
        Func<string> act = () => _backupClient.GetDownloadUrl("backup.zip", "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Token is required.*");
    }

    [Fact]
    public void GetDownloadUrl_WhitespaceKey_ShouldThrowArgumentException()
    {
        // Act
        Func<string> act = () => _backupClient.GetDownloadUrl("   ", "token");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Backup key is required.*");
    }

    [Fact]
    public void GetDownloadUrl_WhitespaceToken_ShouldThrowArgumentException()
    {
        // Act
        Func<string> act = () => _backupClient.GetDownloadUrl("backup.zip", "   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Token is required.*");
    }

    [Fact]
    public void GetDownloadUrl_ShouldUrlEncodeSpecialCharacters()
    {
        // Arrange
        string key = "backup with spaces.zip";
        string token = "token#with?special&chars";

        // Act
        string url = _backupClient.GetDownloadUrl(key, token);

        // Assert - Note: key is sanitized first, then encoded
        // "backup with spaces.zip" → "backup_with_spaces.zip" → "backup_with_spaces.zip"
        // "token#with?special&chars" → URL encoded
        url.Should().Be("https://pb.example.com/api/backups/backup_with_spaces.zip?token=token%23with%3Fspecial%26chars");
    }

    [Fact]
    public void GetDownloadUrl_BaseUrlWithTrailingSlash_ShouldNotCreateDoubleSlashes()
    {
        // Arrange - Create a client with base URL that has trailing slash
        string baseUrl = "https://pb.example.com/";
        HttpTransport transport = new HttpTransport(baseUrl);
        BackupClient backupClient = new BackupClient(transport);

        string key = "backup.zip";
        string token = "token123";

        // Act
        string url = backupClient.GetDownloadUrl(key, token);

        // Assert - Should not have double slashes
        url.Should().NotContain("//api");
        url.Should().Be("https://pb.example.com/api/backups/backup.zip?token=token123");

        // Cleanup
        transport.Dispose();
    }
}
