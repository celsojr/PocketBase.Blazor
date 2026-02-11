namespace PocketBase.Blazor.IntegrationTests.Clients.Admin;

using Blazor.Responses;
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

    [Fact]
    public async Task Impersonate_with_valid_admin_and_record_id_returns_impersonation_token()
    {
        // Arrange - Get a user record to impersonate
        var usersResult = await _pb.Collection("users")
            .GetListAsync<RecordResponse>();
        usersResult.IsSuccess.Should().BeTrue();
        var userRecord = usersResult.Value.Items.First();

        // Act - Impersonate for 1 hour (3600 seconds)
        var result = await _pb.Admins
            .ImpersonateAsync("users", userRecord.Id, 3600);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrEmpty();
        result.Value.Record.Should().NotBeNull();
        result.Value.Record.Id.Should().Be(userRecord.Id);
        result.Value.Record.CollectionName.Should().Be("users");

        var tokenParts = result.Value.Token.Split('.');
        tokenParts.Should().HaveCount(3, "JWT tokens should have 3 parts");

        var expiry = GetTokenExpiry(result.Value.Token);
        expiry.Should().BeAfter(DateTimeOffset.Now, "Token should not be expired");
    }

    [Fact]
    public async Task Impersonate_with_non_admin_user_returns_unauthorized_error()
    {
        // Arrange - First authenticate as regular user (not admin)
        var userAuth = await _pb.Collection("users")
            .AuthWithPasswordAsync(
                _fixture.Settings.UserTesterEmail,
                _fixture.Settings.UserTesterPassword
            );

        userAuth.IsSuccess.Should().BeTrue();
        userAuth.Value.Record.Should().NotBeNull();

        // Get another user record to impersonate
        var usersResult = await _pb.Collection("users")
            .GetListAsync<RecordResponse>();
        usersResult.IsSuccess.Should().BeTrue();

        var userRecord = usersResult.Value.Items
            .FirstOrDefault(u => u.Id != userAuth.Value.Record.Id);
        userRecord.Should().NotBeNull();

        // Act - Impersonate for 1 hour (3600 seconds)
        var result = await _pb.Admins
            .ImpersonateAsync("users", userRecord.Id, 3600);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e =>
            e.Message.Contains("403") ||
            e.Message.Contains("not allowed", StringComparison.OrdinalIgnoreCase) ||
            e.Message.Contains("authorized", StringComparison.OrdinalIgnoreCase)
        );
    }
}
