namespace PocketBase.Blazor.UnitTests.Domain.Backup;

using Blazor.Http;
using FluentAssertions;

public class MultipartFileTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateInstance()
    {
        using var stream = new MemoryStream();
        using var file = new MultipartFile(stream, "test.zip", "application/zip", "backup");

        file.Name.Should().Be("backup");
        file.FileName.Should().Be("test.zip");
        file.ContentType.Should().Be("application/zip");
        file.Content.Should().BeSameAs(stream);
    }

    [Fact]
    public void Constructor_NullStream_ShouldThrow()
    {
        var act = () => new MultipartFile(null!, "test.zip");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_EmptyFileName_ShouldThrow()
    {
        using var stream = new MemoryStream();
        var act = () => new MultipartFile(stream, "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromBytes_ValidBytes_ShouldCreateFile()
    {
        var bytes = new byte[] { 0x01, 0x02, 0x03 };
        using var file = MultipartFile.FromBytes(bytes, "test.bin");

        file.FileName.Should().Be("test.bin");
        file.ContentType.Should().Be("application/octet-stream");
        file.Name.Should().Be("file");

        using var reader = new BinaryReader(file.Content);
        reader.ReadBytes(3).Should().Equal(bytes);
    }

    [Fact]
    public void FromFile_ValidPath_ShouldCreateFile()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempPath, "test content");
            using var file = MultipartFile.FromFile(tempPath, "text/plain");

            file.FileName.Should().Be(Path.GetFileName(tempPath));
            file.ContentType.Should().Be("text/plain");
            file.Content.Length.Should().BeGreaterThan(0);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void FromFile_EmptyPath_ShouldThrow()
    {
        var act = () => MultipartFile.FromFile("");

        act.Should().Throw<ArgumentException>();
    }
}
