namespace PocketBase.Blazor.IntegrationTests.Clients.Settings;

using Blazor.Requests.Settings;

[Collection("PocketBase.Blazor.Admin")]
public class UpdateSettingsTests
{
    private readonly IPocketBase _pb;

    public UpdateSettingsTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task UpdateAsync_Succeeds_WithPartialUpdate()
    {
        var update = new { meta = new { appName = "UpdatedApp" } };

        var result = await _pb.Settings.UpdateAsync(update);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Succeeds_WithTypedRequest()
    {
        var update = new SettingsUpdateRequest
        {
            Meta = new MetaSettingsUpdateRequest { AppName = "TypedUpdate" }
        };

        var result = await _pb.Settings.UpdateAsync(update);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Succeeds_WithDictionary()
    {
        var update = new Dictionary<string, object?>
        {
            ["meta"] = new Dictionary<string, object?>
            {
                ["appName"] = "DictUpdate"
            }
        };

        var result = await _pb.Settings.UpdateAsync(update);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _pb.Settings.UpdateAsync(null));
    }
}

