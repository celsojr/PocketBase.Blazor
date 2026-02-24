namespace PocketBase.Blazor.IntegrationTests.Clients.Settings;

using Blazor.Responses.Settings;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class ListSettingsTests
{
    private readonly IPocketBase _pb;

    public ListSettingsTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task GetAsync_ReturnsSettings()
    {
        // Arrange & Act
        Result<SettingsResponse> result = await _pb.Settings.GetListAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Meta.Should().NotBeNull();
        result.Value.Meta.AppName.Should().NotBeNullOrEmpty();
    }
}

