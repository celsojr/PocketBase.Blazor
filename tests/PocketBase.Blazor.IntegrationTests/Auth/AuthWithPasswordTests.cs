namespace PocketBase.Blazor.IntegrationTests.Auth;

[Collection("PocketBase collection")]
public class AuthWithPasswordTests //: IClassFixture<PocketBaseTestFixture>
{
    private readonly PocketBaseTestFixture _fixture;

    public AuthWithPasswordTests(PocketBaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Auth_with_valid_credentials_returns_token()
    {
        var pb = new PocketBase(_fixture.Settings.BaseUrl);

        var result = await pb
            .Collection("users")
            .AuthWithPasswordAsync(
                _fixture.Settings.TestUserEmail,
                _fixture.Settings.TestUserPassword
            );

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrEmpty();
    }
}
