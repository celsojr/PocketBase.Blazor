namespace PocketBase.Blazor.IntegrationTests.Clients.Admin;

[Collection("PocketBase.Blazor Clients")]
public class AuthWithPasswordTests
{
    private readonly PocketBaseUserFixture _fixture;

    public AuthWithPasswordTests(PocketBaseUserFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Auth_with_valid_credentials_returns_token()
    {
        var pb = new PocketBase(_fixture.Settings.BaseUrl);

        var result = await pb.Admins
            .AuthWithPasswordAsync(
                _fixture.Settings.UserTesterEmail,
                _fixture.Settings.UserTesterPassword
            );

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrEmpty();
    }
}

