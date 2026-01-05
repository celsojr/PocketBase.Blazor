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
                _fixture.Settings.AdminTesterEmail,
                _fixture.Settings.AdminTesterPassword
            );

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrEmpty();
        CheckTokenParts(result.Value.Token).Should().BeTrue();
    }

    private static bool CheckTokenParts(string token)
    {
        var opt = StringSplitOptions.RemoveEmptyEntries;
        var parts = token.Split('.', opt);
        if (parts.Length == 3)
        {
            return true;
        }
        return false;
    }

}

