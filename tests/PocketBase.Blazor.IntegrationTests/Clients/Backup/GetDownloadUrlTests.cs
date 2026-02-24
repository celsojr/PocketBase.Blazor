namespace PocketBase.Blazor.IntegrationTests.Clients.Backup;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class DownloadBackupTests
{
    private readonly IPocketBase _pb;
    private readonly HttpClient _httpClient;

    public DownloadBackupTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
        _httpClient = new HttpClient();
    }

    [Fact]
    public async Task GetDownloadUrl_AndDownload_ExistingBackup_ShouldWork()
    {
        // Arrange - Create a backup
        string backupName = $"download-test-{Guid.NewGuid():N}.zip";
        await _pb.Backup.CreateAsync(backupName);

        // Wait for backup to complete
        await Task.Delay(2000);

        try
        {
            // Get a token
            Result<string> token = await _pb.Files.GetTokenAsync();
            token.Value.Should().NotBeNullOrEmpty();

            // Get the download URL
            string downloadUrl = _pb.Backup.GetDownloadUrl(backupName, token.Value);

            // Act - Download using HttpClient
            HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);

            // Assert
            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync();

            // Verify stream has content
            stream.Length.Should().BeGreaterThan(0);

            // Check if it's a ZIP file
            byte[] header = new byte[2];
            await stream.ReadAsync(header);

            // ZIP files start with "PK" (0x50 0x4B)
            // ZIP signature is literally the ASCII characters "PK" (Phil Katz, creator of PKZIP)
            // Might not work on big endian systems, but most modern systems are little-endian
            bool isZipFile = header.AsSpan(0, 2).SequenceEqual("PK"u8);
            isZipFile.Should().BeTrue("Backup files should be ZIP files");
        }
        finally
        {
            // Cleanup
            await _pb.Backup.DeleteAsync(backupName);
        }
    }

    [Fact]
    public async Task GetDownloadUrl_AndDownload_NonExistentBackup_ShouldFail()
    {
        // Arrange
        string nonExistentBackup = $"non-existent-{Guid.NewGuid():N}.zip";
        string token = "dummy-token"; // You'll need a valid token format

        // Act - Get URL
        string downloadUrl = _pb.Backup.GetDownloadUrl(nonExistentBackup, token);

        // Try to download
        HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDownloadUrl_AndDownload_WithInvalidToken_ShouldFail()
    {
        // Arrange - Create a backup
        string backupName = $"invalid-token-test-{Guid.NewGuid():N}.zip";
        await _pb.Backup.CreateAsync(backupName);

        await Task.Delay(2000);

        try
        {
            // Use an invalid token
            string invalidToken = "invalid-token-123";
            string downloadUrl = _pb.Backup.GetDownloadUrl(backupName, invalidToken);

            // Act
            HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally
        {
            await _pb.Backup.DeleteAsync(backupName);
        }
    }

    [Fact]
    public async Task GetDownloadUrl_AndDownload_CanSaveToFile()
    {
        // Arrange
        string backupName = $"save-to-file-{Guid.NewGuid():N}.zip";
        await _pb.Backup.CreateAsync(backupName);

        await Task.Delay(2000);

        string tempFilePath = Path.Combine(Path.GetTempPath(), $"{backupName}");

        try
        {
            // Get token
            Result<string> token = await _pb.Files.GetTokenAsync();
            token.Value.Should().NotBeNullOrEmpty();

            string downloadUrl = _pb.Backup.GetDownloadUrl(backupName, token.Value);

            // Act - Download and save
            HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);
            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync();
            await using (FileStream fileStream = File.Create(tempFilePath))
            {
                await stream.CopyToAsync(fileStream);
            }

            // Assert
            FileInfo fileInfo = new FileInfo(tempFilePath);
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeGreaterThan(0);

            // Verify it's a ZIP file
            await using FileStream verifyStream = File.OpenRead(tempFilePath);
            byte[] header = new byte[2];
            await verifyStream.ReadAsync(header);

            bool isZipFile = header.AsSpan(0, 2).SequenceEqual("PK"u8);
            isZipFile.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            await _pb.Backup.DeleteAsync(backupName);
        }
    }

    [Fact]
    public void GetDownloadUrl_WithEmptyParameters_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _pb.Backup.GetDownloadUrl("", "token"));
        Assert.Throws<ArgumentException>(() => _pb.Backup.GetDownloadUrl("backup.zip", ""));
        Assert.Throws<ArgumentException>(() => _pb.Backup.GetDownloadUrl("   ", "token"));
        Assert.Throws<ArgumentException>(() => _pb.Backup.GetDownloadUrl("backup.zip", "   "));
    }
}
