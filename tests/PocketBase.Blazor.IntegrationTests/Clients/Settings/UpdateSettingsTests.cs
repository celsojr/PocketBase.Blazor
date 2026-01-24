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
        // Arrange
        var update = new { meta = new { appName = "UpdatedApp" } };

        // Act
        var result = await _pb.Settings.UpdateAsync(update);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Succeeds_WithTypedRequest()
    {
        // Arrange
        var update = new SettingsUpdateRequest
        {
            Meta = new MetaSettingsUpdateRequest { AppName = "TypedUpdate" }
        };

        // Act
        var result = await _pb.Settings.UpdateAsync(update);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Succeeds_WithDictionary()
    {
        // Arrange
        var update = new Dictionary<string, object?>
        {
            ["meta"] = new Dictionary<string, object?>
            {
                ["appName"] = "DictUpdate"
            }
        };

        // Act
        var result = await _pb.Settings.UpdateAsync(update);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _pb.Settings.UpdateAsync(null!));
    }
}

