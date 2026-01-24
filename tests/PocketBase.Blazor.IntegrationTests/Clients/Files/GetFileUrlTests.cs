namespace PocketBase.Blazor.IntegrationTests.Clients.Files;

[Collection("PocketBase.Blazor.Admin")]
public class GetFileUrlTests
{
    private readonly IPocketBase _pb;

    public GetFileUrlTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task GetUrl_ReturnsUrl_WithValidParameters()
    {
        var result = await _pb.Files.GetUrl("_pb_users_auth_", "ssl170o5679k806", "star_ne6lkj8zxl.jpg");

        result.Value.Should().NotBeEmpty();
        //result.Value.Should().Contain("api/files/_pb_users_auth_/ssl170o5679k806/star_ne6lkj8zxl.jpg");
    }

    //[Fact]
    //public async Task GetUrl_ReturnsEmpty_WhenFileNameEmpty()
    //{
    //    var result = await _pb.Files.GetUrl("users", "user123", "", null);

    //    result.Value.Should().BeEmpty();
    //}

    //[Fact]
    //public async Task GetUrl_ReturnsEmpty_WhenRecordIdEmpty()
    //{
    //    var result = await _pb.Files.GetUrl("users", "", "avatar.jpg", null);

    //    result.Value.Should().BeEmpty();
    //}

    //[Fact]
    //public async Task GetUrl_EncodesSpecialCharacters()
    //{
    //    var result = await _pb.Files.GetUrl("users/group", "user 123", "file name.jpg", null);

    //    result.Value.Should().NotBeEmpty();
    //    result.Value.Should().Contain("api/files/users%2Fgroup/user%20123/file%20name.jpg");
    //}

    //[Fact]
    //public async Task GetUrl_IncludesThumbQuery()
    //{
    //    var query = new Dictionary<string, object?>
    //    {
    //        ["thumb"] = "100x300"
    //    };

    //    var result = await _pb.Files.GetUrl("users", "user123", "avatar.jpg", query);

    //    result.Value.Should().Contain("thumb=100x300");
    //}

    //[Theory]
    //[InlineData("100x300", "100x300")]
    //[InlineData("100x300t", "100x300t")]
    //[InlineData("100x300b", "100x300b")]
    //[InlineData("100x300f", "100x300f")]
    //[InlineData("0x300", "0x300")]
    //[InlineData("100x0", "100x0")]
    //public async Task GetUrl_SupportsAllThumbFormats(string thumb, string expected)
    //{
    //    var query = new Dictionary<string, object?>
    //    {
    //        ["thumb"] = thumb
    //    };

    //    var result = await _pb.Files.GetUrl("users", "user123", "avatar.jpg", query);

    //    result.Value.Should().Contain($"thumb={expected}");
    //}

    //[Fact]
    //public async Task GetUrl_IncludesToken()
    //{
    //    var query = new Dictionary<string, object?>
    //    {
    //        ["token"] = "file_token_abc123"
    //    };

    //    var result = await _pb.Files.GetUrl("users", "user123", "avatar.jpg", query);

    //    result.Value.Should().Contain("token=file_token_abc123");
    //}

    //[Theory]
    //[InlineData("1")]
    //[InlineData("t")]
    //[InlineData("true")]
    //public async Task GetUrl_IncludesDownload_WhenTruthy(string downloadValue)
    //{
    //    var query = new Dictionary<string, object?>
    //    {
    //        ["download"] = downloadValue
    //    };

    //    var result = await _pb.Files.GetUrl("users", "user123", "avatar.jpg", query);

    //    result.Value.Should().Contain($"download={downloadValue}");
    //}

    //[Fact]
    //public async Task GetUrl_RemovesDownload_WhenFalse()
    //{
    //    var query = new Dictionary<string, object?>
    //    {
    //        ["download"] = "false",
    //        ["thumb"] = "100x100"
    //    };

    //    var result = await _pb.Files.GetUrl("users", "user123", "avatar.jpg", query);

    //    result.Value.Should().NotContain("download=");
    //    result.Value.Should().Contain("thumb=100x100");
    //}

    //[Fact]
    //public async Task GetUrl_HandlesMultipleQueryParams()
    //{
    //    var query = new Dictionary<string, object?>
    //    {
    //        ["thumb"] = "100x300f",
    //        ["token"] = "abc123",
    //        ["download"] = "true"
    //    };

    //    var result = await _pb.Files.GetUrl("posts", "post456", "image.png", query);

    //    result.Value.Should().Contain("thumb=100x300f");
    //    result.Value.Should().Contain("token=abc123");
    //    result.Value.Should().Contain("download=true");
    //}

    // ===

    //[Fact]
    //public async Task GetFileAsync_ReturnsFileBytes()
    //{
    //    var options = new FileOptions { Thumb = "100x300" };
    //    var result = await _pb.Files.GetFileAsync("users", "user123", "avatar.jpg", options);

    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Should().NotBeNull();
    //    result.Value.Length.Should().BeGreaterThan(0);
    //}

    //[Fact]
    //public void GetFileUrl_ReturnsCorrectUrl()
    //{
    //    var options = new FileOptions { Thumb = "100x300f", Download = true };
    //    var url = _pb.Files.GetFileUrl("users", "user123", "avatar.jpg", options);

    //    url.Should().Be("api/files/users/user123/avatar.jpg?thumb=100x300f&download=1");
    //}
}

