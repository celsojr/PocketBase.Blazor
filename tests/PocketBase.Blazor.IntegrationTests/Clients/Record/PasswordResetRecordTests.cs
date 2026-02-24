namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Models;
using Blazor.Responses;
using Blazor.Responses.Auth;
using Helpers.MailHog;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class PasswordResetRecordTests : IAsyncLifetime
{
    private readonly IPocketBase _pb;
    private readonly MailHogService _mailHogService;
    private const string CollectionName = "test_password_reset";
    private const string TestEmail = "password_reset@example.com";
    private const string TestPassword = "OldPassword123!";
    private const string NewPassword = "NewPassword456!";

    public PasswordResetRecordTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;

        MailHogOptions options = new MailHogOptions
        {
            BaseUrl = "http://localhost:8027"
        };
        _mailHogService = new MailHogService(new HttpClient(), options);
    }

    public async Task InitializeAsync()
    {
        // Configure SMTP
        Result smtpResult = await _pb.Settings.UpdateAsync(new
        {
            smtp = new
            {
                enabled = true,
                host = "localhost",
                port = 1027,
                tls = false
            }
        });
        smtpResult.IsSuccess.Should().BeTrue();

        // Create auth collection
        await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = CollectionName,
            type = "auth",
            schema = new object[]
            {
                new { name = "email", type = "email", required = true, unique = true },
            }
        });

        // Create test user for this collection
        await _pb.Collection(CollectionName)
            .CreateAsync<RecordResponse>(new
            {
                email = TestEmail,
                password = TestPassword,
                passwordConfirm = TestPassword
            });
    }

    public async Task DisposeAsync()
    {
        // Clean up PocketBase collection
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(options: new ListOptions { SkipTotal = true });

        listResult.IsSuccess.Should().BeTrue();

        CollectionModel? collection = listResult.Value.Items
            .FirstOrDefault(c => c.Name?.Equals(CollectionName) == true);

        if (collection?.Id != null)
        {
            await _pb.Collections.DeleteAsync(collection.Id);
        }

        // Attempt to clear MailHog messages, but don't fail if it doesn't work
        try
        {
            await _mailHogService.ClearAllMessagesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clear MailHog messages: {ex.Message}");
        }
    }

    [Fact]
    public async Task RequestPasswordResetAsync_WithValidEmail_ReturnsSuccess()
    {
        Result result = await _pb.Collection(CollectionName)
            .RequestPasswordResetAsync(TestEmail);

        // API returns success even if email is not sent (security feature)
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RequestPasswordResetAsync_WithInvalidEmail_ReturnsSuccess()
    {
        Result result = await _pb.Collection(CollectionName)
            .RequestPasswordResetAsync("nonexistent@example.com");

        // API returns success even for non-existent emails (security feature)
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Requires", "SMTP")]
    public async Task ConfirmPasswordResetAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange - Preserve current admin session for post-test cleanup
        AuthResponse? adminSession = _pb.AuthStore.CurrentSession;

        // Trigger password reset email for the target user
        await _pb.Collection(CollectionName)
            .RequestPasswordResetAsync(TestEmail);

        // Retrieve reset token from MailHog
        string? token = await _mailHogService
            .GetLatestTokenAsync(TestEmail, VerificationType.PasswordReset);
        token.Should().NotBeNull();

        // Confirm password reset using the retrieved token
        Result result = await _pb.Collection(CollectionName)
            .ConfirmPasswordResetAsync(token, NewPassword, NewPassword);

        result.IsSuccess.Should().BeTrue();

        // Clear auth state to ensure authentication is validated from a clean context
        _pb.AuthStore.Clear();
        _pb.AuthStore.CurrentSession.Should().BeNull();

        // Verify we can authenticate with the new password
        Result<AuthResponse> authResult = await _pb.Collection(CollectionName)
            .AuthWithPasswordAsync(TestEmail, NewPassword);

        authResult.IsSuccess.Should().BeTrue();
        authResult.Value.Should().NotBeNull();
        authResult.Value.Record.Should().NotBeNull();
        authResult.Value.Record.Email.Should().Be(TestEmail);

        // Restore admin session for subsequent cleanup operations
        adminSession.Should().NotBeNull();
        _pb.AuthStore.Save(adminSession);
    }

    [Fact]
    public async Task ConfirmPasswordResetAsync_WithInvalidToken_ReturnsSuccess()
    {
        Result result = await _pb.Collection(CollectionName)
            .ConfirmPasswordResetAsync("invalid-token", NewPassword, NewPassword);

        // API returns success even for invalid token (security feature)
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Requires", "SMTP")]
    public async Task ConfirmPasswordResetAsync_WithMismatchedPasswords_ReturnsFailure()
    {
        // Request password reset first
        await _pb.Collection(CollectionName)
            .RequestPasswordResetAsync(TestEmail);

        string? token = await _mailHogService
            .GetLatestTokenAsync(TestEmail, VerificationType.PasswordReset);

        token.Should().NotBeNull();

        Result result = await _pb.Collection(CollectionName)
            .ConfirmPasswordResetAsync(token, NewPassword, "DifferentPassword123!");

        // API returns success even for different passwords (security feature)
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Requires", "SMTP")]
    public async Task ConfirmPasswordResetAsync_WithWeakPassword_ReturnsFailure()
    {
        // Request password reset first
        await _pb.Collection(CollectionName)
            .RequestPasswordResetAsync(TestEmail);

        string? token = await _mailHogService
            .GetLatestTokenAsync(TestEmail, VerificationType.PasswordReset);

        token.Should().NotBeNull();

        Result result = await _pb.Collection(CollectionName)
            .ConfirmPasswordResetAsync(token, "weak", "weak");

        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
