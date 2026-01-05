namespace PocketBase.Blazor.IntegrationTests.Clients.Admin;

[Collection("PocketBase.Blazor.Admin")]
public class AuthWithPasswordTests
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseAdminFixture _fixture;

    public AuthWithPasswordTests(PocketBaseAdminFixture fixture)
    {
        _fixture = fixture;
        _pb = fixture.Client;
    }

    [Fact]
    public async Task Auth_with_valid_credentials_returns_token()
    {
        var result = await _pb.Admins
            .AuthWithPasswordAsync(
                _fixture.Settings.UserTesterEmail,
                _fixture.Settings.UserTesterPassword
            );

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrEmpty();
    }
}

