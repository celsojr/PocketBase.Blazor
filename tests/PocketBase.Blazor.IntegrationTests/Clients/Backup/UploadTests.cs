namespace PocketBase.Blazor.IntegrationTests.Clients.Backup;

using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Http;
using Xunit;

[Collection("PocketBase.Blazor.Admin")]
public class UploadTests
{
    private readonly IPocketBase _pb;

    public UploadTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task UploadAsync_ValidBackupFile_ShouldSucceed()
    {
        // Arrange
        var fileName = $"upload-test-{Guid.NewGuid():N}.zip";
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);

        // Create a proper ZIP file - is gonna be automatically deleted
        await using var zipFile = await CreateValidZipFileAsync(tempPath);

        // Prepare the file for upload
        using var file = MultipartFile.FromFile(zipFile.Path, "application/zip");

        // Act
        var result = await _pb.Backup.UploadAsync(file);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify backup exists
        var listResult = await _pb.Backup.GetFullListAsync();
        var uploadedBackup = listResult.Value.FirstOrDefault(b =>
            b.Key!.Contains(fileName.Replace(".zip", "")));

        uploadedBackup.Should().NotBeNull();

        // Cleanup
        await _pb.Backup.DeleteAsync(uploadedBackup!.Key!);
    }

    [Fact]
    public async Task UploadAsync_WithBasicHeaderZip_ShouldFail()
    {
        // Arrange
        var fileName = $"basic-header-test-{Guid.NewGuid():N}.zip";
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);

        // Create a ZIP file with only the basic header
        // PocketBase expects a valid ZIP structure
        await using (var fs = File.Create(tempPath))
        {
            // ZIP file header
            await fs.WriteAsync((byte[])[.. "PK"u8]);
        }

        // Prepare the file for upload
        using var file = MultipartFile.FromFile(tempPath, "application/zip");

        // Act
        var result = await _pb.Backup.UploadAsync(file);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UploadAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        await using var tempFile = CreateTestZipFile("cancel-test.zip");
        using var file = MultipartFile.FromFile(tempFile.Path, "application/zip");
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = async () => await _pb.Backup.UploadAsync(file, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UploadAsync_WhenUnauthenticated_ShouldFail()
    {
        // Arrange
        var fileName = $"unauth-test-{Guid.NewGuid():N}.zip";
        await using var tempFile = CreateTestZipFile(fileName);

        try
        {
            using var file = MultipartFile.FromFile(tempFile.Path, "application/zip");
            await using var pb = new PocketBase(_pb.BaseUrl);

            // Act
            var result = await pb.Backup.UploadAsync(file);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }
        finally
        {
            if (File.Exists(tempFile.Path))
            {
                File.Delete(tempFile.Path);
            }
        }
    }

    [Fact]
    public async Task UploadAsync_NullFile_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _pb.Backup.UploadAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UploadAsync_InvalidFileFormat_ShouldFail()
    {
        // Arrange - Upload a non-ZIP file
        var invalidContent = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        using var file = MultipartFile.FromBytes(invalidContent, "not-a-backup.txt", "text/plain");

        // Act
        var result = await _pb.Backup.UploadAsync(file);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    private static TempFile CreateTestZipFile(string fileName)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), fileName);
        var zipHeader = (byte[])[.. "PK"u8]; // Minimal ZIP header
        File.WriteAllBytes(tempPath, zipHeader);
        return new TempFile(tempPath);
    }

    private static async Task<TempFile> CreateValidZipFileAsync(string path)
    {
        using var stream = File.Create(path);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

        // Add a simple text file to make it a valid ZIP
        var entry = archive.CreateEntry("backup-info.txt");
        await using var entryStream = entry.Open();
        await using var writer = new StreamWriter(entryStream);
        await writer.WriteAsync($"Test backup created at {DateTime.UtcNow:o}");
        return new TempFile(path);
    }

    private sealed class TempFile : IAsyncDisposable
    {
        public string Path { get; }

        public TempFile(string path)
        {
            Path = path;
        }

        public ValueTask DisposeAsync()
        {
            try { File.Delete(Path); } catch { }
            return ValueTask.CompletedTask;
        }
    }
}
