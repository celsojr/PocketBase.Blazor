namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Responses.Auth;
using static IntegrationTests.Helpers.JwtTokenValidator;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.User")]
public class AuthWithPasswordRecordTests
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseUserFixture _fixture;

    public AuthWithPasswordRecordTests(PocketBaseUserFixture fixture)
    {
        _pb = fixture.Client;
        _fixture = fixture;
    }

    [Fact]
    public async Task Auth_with_valid_credentials_returns_token()
    {
        Result<AuthResponse> result = await _pb.Collection("users")
            .AuthWithPasswordAsync(
                _fixture.Settings.UserTesterEmail,
                _fixture.Settings.UserTesterPassword
            );

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrEmpty();

        string[] tokenParts = result.Value.Token.Split('.');
        tokenParts.Should().HaveCount(3, "JWT tokens should have 3 parts");

        DateTimeOffset? expiry = GetTokenExpiry(result.Value.Token);
        expiry.Should().BeAfter(DateTimeOffset.Now, "Token should not be expired");
    }

    [Fact]
    public async Task Auth_with_invalid_email_returns_error()
    {
        Result<AuthResponse> result = await _pb.Collection("users")
            .AuthWithPasswordAsync("wrong@email.com", _fixture.Settings.UserTesterPassword);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Auth_with_invalid_password_returns_error()
    {
        Result<AuthResponse> result = await _pb.Collection("users")
            .AuthWithPasswordAsync(_fixture.Settings.UserTesterEmail, "wrongpassword");

        result.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData("   ", "password")]
    [InlineData("test@example.com", null)]
    [InlineData("test@example.com", "")]
    [InlineData("test@example.com", "   ")]
    public async Task Auth_with_invalid_arguments_throws_exception(string? email, string? password)
    {
        Func<Task> act = async () => await _pb.Collection("users")
            .AuthWithPasswordAsync(email!, password!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Auth_can_be_cancelled()
    {
        using CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(0); // Cancel immediately

        Func<Task> act = async () => await _pb.Collection("users")
            .AuthWithPasswordAsync(
                _fixture.Settings.UserTesterEmail,
                _fixture.Settings.UserTesterPassword,
                cancellationToken: cts.Token
            );

        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}

