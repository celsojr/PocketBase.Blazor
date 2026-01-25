namespace PocketBase.Blazor.IntegrationTests.Clients.Files;

using Blazor.Options;

[Collection("PocketBase.Blazor.Admin")]
public class DonwloadTests
{
    private readonly IPocketBase _pb;

    public DonwloadTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task GetFileAsync_SavesStreamToFile()
    {
        var options = new FileOptions { Thumb = "100x100" };
        var result = await _pb.Files.GetStreamAsync("_pb_users_auth_", "2xwhe4qnnegixob", "img_9126_3_e16nay7hie.jpg", options);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var filePath = Path.Combine(Path.GetTempPath(), $"test_download_{Guid.NewGuid()}.jpg");
        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await result.Value.CopyToAsync(fileStream);
        }

        File.Exists(filePath).Should().BeTrue();
        var fileInfo = new FileInfo(filePath);
        fileInfo.Length.Should().BeGreaterThan(0);

        // Clean up
        File.Delete(filePath);
    }

    [Fact]
    public async Task GetFileAsync_SavesBytesToFile()
    {
        var options = new FileOptions { Thumb = "100x100" };
        var result = await _pb.Files.GetBytesAsync("_pb_users_auth_", "2xwhe4qnnegixob", "img_9126_3_e16nay7hie.jpg", options);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var filePath = Path.Combine(Path.GetTempPath(), $"test_download_{Guid.NewGuid()}.jpg");
        await File.WriteAllBytesAsync(filePath, result.Value);

        File.Exists(filePath).Should().BeTrue();
        var fileInfo = new FileInfo(filePath);
        fileInfo.Length.Should().BeGreaterThan(0);

        // Clean up
        File.Delete(filePath);
    }
}
