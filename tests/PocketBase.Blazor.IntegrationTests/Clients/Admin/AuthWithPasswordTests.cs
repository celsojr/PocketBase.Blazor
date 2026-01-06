namespace PocketBase.Blazor.IntegrationTests.Clients.Admin;

using static IntegrationTests.Helpers.JwtTokenValidator;

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

        var tokenParts = result.Value.Token.Split('.');
        tokenParts.Should().HaveCount(3, "JWT tokens should have 3 parts");

        var expiry = GetTokenExpiry(result.Value.Token);
        expiry.Should().BeAfter(DateTimeOffset.Now, "Token should not be expired");
    }

    [Fact]
    public async Task Auth_with_invalid_email_returns_error()
    {
        var result = await _pb.Admins
            .AuthWithPasswordAsync("wrong@email.com", _fixture.Settings.AdminTesterPassword);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Auth_with_invalid_password_returns_error()
    {
        var result = await _pb.Admins
            .AuthWithPasswordAsync(_fixture.Settings.AdminTesterEmail, "wrongpassword");

        result.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData("   ", "password")]
    [InlineData("test@example.com", null)]
    [InlineData("test@example.com", "")]
    [InlineData("test@example.com", "   ")]
    public async Task Auth_with_invalid_arguments_throws_exception(string email, string password)
    {
        Func<Task> act = async () => await _pb.Admins
            .AuthWithPasswordAsync(email, password);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Auth_can_be_cancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(0); // Cancel immediately

        Func<Task> act = async () => await _pb.Admins
            .AuthWithPasswordAsync(
                _fixture.Settings.AdminTesterEmail,
                _fixture.Settings.AdminTesterPassword,
                cts.Token
            );

        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}

