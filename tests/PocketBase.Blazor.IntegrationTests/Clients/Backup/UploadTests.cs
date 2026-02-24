namespace PocketBase.Blazor.IntegrationTests.Clients.Backup;

using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Http;
using Blazor.Responses.Backup;
using Xunit;

[Trait("Category", "Integration")]
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
        string fileName = $"upload-test-{Guid.NewGuid():N}.zip";
        string tempPath = Path.Combine(Path.GetTempPath(), fileName);

        // Create a proper ZIP file - is gonna be automatically deleted
        await using TempFile zipFile = await CreateValidZipFileAsync(tempPath);

        // Prepare the file for upload
        using MultipartFile file = MultipartFile.FromFile(zipFile.Path, "application/zip");

        // Act
        Result result = await _pb.Backup.UploadAsync(file);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify backup exists
        Result<List<BackupInfoResponse>> listResult = await _pb.Backup.GetFullListAsync();
        BackupInfoResponse? uploadedBackup = listResult.Value.FirstOrDefault(b =>
            b.Key!.Contains(fileName.Replace(".zip", "")));

        uploadedBackup.Should().NotBeNull();

        // Cleanup
        await _pb.Backup.DeleteAsync(uploadedBackup!.Key!);
    }

    [Fact]
    public async Task UploadAsync_WithBasicHeaderZip_ShouldFail()
    {
        // Arrange
        string fileName = $"basic-header-test-{Guid.NewGuid():N}.zip";
        string tempPath = Path.Combine(Path.GetTempPath(), fileName);

        // Create a ZIP file with only the basic header
        // PocketBase expects a valid ZIP structure
        await using (FileStream fs = File.Create(tempPath))
        {
            // ZIP file header
            await fs.WriteAsync((byte[])[.. "PK"u8]);
        }

        // Prepare the file for upload
        using MultipartFile file = MultipartFile.FromFile(tempPath, "application/zip");

        // Act
        Result result = await _pb.Backup.UploadAsync(file);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UploadAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        await using TempFile tempFile = CreateTestZipFile("cancel-test.zip");
        using MultipartFile file = MultipartFile.FromFile(tempFile.Path, "application/zip");
        CancellationTokenSource cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        Func<Task<Result>> act = async () => await _pb.Backup.UploadAsync(file, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UploadAsync_WhenUnauthenticated_ShouldFail()
    {
        // Arrange
        string fileName = $"unauth-test-{Guid.NewGuid():N}.zip";
        await using TempFile tempFile = CreateTestZipFile(fileName);

        try
        {
            using MultipartFile file = MultipartFile.FromFile(tempFile.Path, "application/zip");
            await using PocketBase pb = new PocketBase(_pb.BaseUrl);

            // Act
            Result result = await pb.Backup.UploadAsync(file);

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
        Func<Task<Result>> act = async () => await _pb.Backup.UploadAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UploadAsync_InvalidFileFormat_ShouldFail()
    {
        // Arrange - Upload a non-ZIP file
        byte[] invalidContent = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        using MultipartFile file = MultipartFile.FromBytes(invalidContent, "not-a-backup.txt", "text/plain");

        // Act
        Result result = await _pb.Backup.UploadAsync(file);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    private static TempFile CreateTestZipFile(string fileName)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), fileName);
        byte[] zipHeader = (byte[])[.. "PK"u8]; // Minimal ZIP header
        File.WriteAllBytes(tempPath, zipHeader);
        return new TempFile(tempPath);
    }

    private static async Task<TempFile> CreateValidZipFileAsync(string path)
    {
        using FileStream stream = File.Create(path);
        using ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create);

        // Add a simple text file to make it a valid ZIP
        ZipArchiveEntry entry = archive.CreateEntry("backup-info.txt");
        await using Stream entryStream = entry.Open();
        await using StreamWriter writer = new StreamWriter(entryStream);
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
