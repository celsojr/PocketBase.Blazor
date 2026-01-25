namespace PocketBase.Blazor.IntegrationTests.Clients.Files;

using Blazor.Options;

[Collection("PocketBase.Blazor.Admin")]
public class GetFileUrlTests
{
    private readonly IPocketBase _pb;

    public GetFileUrlTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    /*
        If your file field has the Thumb sizes option, you can get a thumb of the image file by
        adding the thumb query parameter to the url like this:
        http://127.0.0.1:8090/api/files/COLLECTION_ID_OR_NAME/RECORD_ID/FILENAME?thumb=100x300
        Currently limited to jpg, png, gif (its first frame) and partially webp (stored as png).

        The following thumb formats are currently supported:

        WxH (e.g. 100x300) - crop to WxH viewbox (from center)
        WxHt (e.g. 100x300t) - crop to WxH viewbox (from top)
        WxHb (e.g. 100x300b) - crop to WxH viewbox (from bottom)
        WxHf (e.g. 100x300f) - fit inside a WxH viewbox (without cropping)
        0xH (e.g. 0x300) - resize to H height preserving the aspect ratio
        Wx0 (e.g. 100x0) - resize to W width preserving the aspect ratio
        The original file would be returned, if the requested thumb size is not found or the file is not an image!
    */

    [Fact]
    public async Task GetUrl_ReturnsUrl_WithValidParameters()
    {
        var result = await _pb.Files.GetUrl("_pb_users_auth_", "ssl170o5679k806", "star_ne6lkj8zxl.jpg");

        result.Value.Should().NotBeEmpty();
        result.Value.Should().Be("api/files/_pb_users_auth_/ssl170o5679k806/star_ne6lkj8zxl.jpg");
    }

    [Fact]
    public async Task GetUrl_ReturnsEmpty_WhenFileNameEmpty()
    {
       var result = await _pb.Files.GetUrl("users", "user123", "", null!);

       result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUrl_ReturnsEmpty_WhenRecordIdEmpty()
    {
       var result = await _pb.Files.GetUrl("users", "", "avatar.jpg", null!);

       result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUrl_EncodesSpecialCharacters()
    {
       var result = await _pb.Files.GetUrl("users/group", "user 123", "file name.jpg", null!);

       result.Value.Should().NotBeEmpty();
       result.Value.Should().Contain("api/files/users%2Fgroup/user%20123/file%20name.jpg");
    }

    [Fact]
    public async Task GetUrl_IncludesThumbQuery()
    {
       var options = new FileOptions
       {
           Thumb = "100x300"
       };

       var result = await _pb.Files.GetUrl("users", "user123", "avatar.jpg", options);

       result.Value.Should().Contain("thumb=100x300");
    }

    [Theory]
    [InlineData("100x300", "100x300")]
    [InlineData("100x300t", "100x300t")]
    [InlineData("100x300b", "100x300b")]
    [InlineData("100x300f", "100x300f")]
    [InlineData("0x300", "0x300")]
    [InlineData("100x0", "100x0")]
    public async Task GetUrl_SupportsAllThumbFormats(string thumb, string expected)
    {
       var options = new FileOptions
       {
           Thumb = thumb
       };

       var result = await _pb.Files.GetUrl("users", "user123", "avatar.jpg", options);

       result.Value.Should().Contain($"thumb={expected}");
    }

    [Fact]
    public async Task GetUrl_IncludesToken()
    {
       var options = new FileOptions
       {
           Token = "file_token_abc123"
       };

       var result = await _pb.Files.GetUrl("users", "user123", "avatar.jpg", options);

       result.Value.Should().Contain("token=file_token_abc123");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("t")]
    [InlineData("true")]
    public async Task GetUrl_IncludesDownload_WhenTruthy(string downloadValue)
    {
       var options = new FileOptions
       {
           Download = downloadValue
       };

       var result = await _pb.Files.GetUrl("users", "user123", "avatar.jpg", options);

       result.Value.Should().Contain($"download={downloadValue}");
    }

    [Fact]
    public async Task GetUrl_RemovesDownload_WhenFalse()
    {
       var options = new FileOptions
       {
           Thumb = "100x100"
       };

       var result = await _pb.Files.GetUrl("users", "user123", "avatar.jpg", options);

       result.Value.Should().NotContain("download=");
       result.Value.Should().Contain("thumb=100x100");
    }

    [Fact]
    public async Task GetUrl_HandlesMultipleQueryParams()
    {
       var options = new FileOptions
       {
              Thumb = "100x300f",
              Token = "abc123",
              Download = "true"
       };

       var result = await _pb.Files.GetUrl("posts", "post456", "image.png", options);

       result.Value.Should().Contain("thumb=100x300f");
       result.Value.Should().Contain("token=abc123");
       result.Value.Should().Contain("download=true");
    }
}
