namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Models;
using Blazor.Responses;
using Blazor.Responses.Auth;
using Helpers.MailHog;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class EmailChangeRecordTests : IAsyncLifetime
{
    private readonly IPocketBase _pb;
    private readonly MailHogService _mailHogService;

    private const string CollectionName = "test_email_change";
    private const string OriginalEmail = "email_change@example.com";
    private const string NewEmail = "updated_email@example.com";
    private const string Password = "EmailChange123!";

    public EmailChangeRecordTests(PocketBaseAdminFixture fixture)
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

        // Create user
        await _pb.Collection(CollectionName)
            .CreateAsync<RecordResponse>(new
            {
                email = OriginalEmail,
                password = Password,
                passwordConfirm = Password
            });
    }

    public async Task DisposeAsync()
    {
        // Delete test collection
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(options: new ListOptions { SkipTotal = true });

        CollectionModel? collection = listResult.Value.Items
            .FirstOrDefault(c => c.Name == CollectionName);

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
    [Trait("Requires", "SMTP")]
    public async Task RequestEmailChangeAsync_WithAuthenticatedUser_ReturnsSuccess()
    {
        // Arrange - reserve current admin session for post-test cleanup
        AuthResponse? adminSession = _pb.AuthStore.CurrentSession;

        // Authenticate user first
        Result<AuthResponse> auth = await _pb.Collection(CollectionName)
            .AuthWithPasswordAsync(OriginalEmail, Password);

        auth.IsSuccess.Should().BeTrue();

        Result result = await _pb.Collection(CollectionName)
            .RequestEmailChangeAsync(NewEmail);

        result.IsSuccess.Should().BeTrue();

        // Restore admin session for subsequent cleanup operations
        adminSession.Should().NotBeNull();
        _pb.AuthStore.Save(adminSession);
    }

    [Fact]
    [Trait("Requires", "SMTP")]
    public async Task ConfirmEmailChangeAsync_WithValidToken_UpdatesEmail()
    {
        // Preserve admin session
        AuthResponse? adminSession = _pb.AuthStore.CurrentSession;

        // Authenticate user
        await _pb.Collection(CollectionName)
            .AuthWithPasswordAsync(OriginalEmail, Password);

        // Request change
        await _pb.Collection(CollectionName)
            .RequestEmailChangeAsync(NewEmail);

        // Extract confirmation token from MailHog
        string? token = await _mailHogService
            .GetLatestTokenAsync(NewEmail, VerificationType.EmailChange);

        token.Should().NotBeNull();

        // Confirm change
        Result result = await _pb.Collection(CollectionName)
            .ConfirmEmailChangeAsync(token, Password);

        result.IsSuccess.Should().BeTrue();

        // Clear session
        _pb.AuthStore.Clear();

        // Authenticate using new email
        Result<AuthResponse> auth = await _pb.Collection(CollectionName)
            .AuthWithPasswordAsync(NewEmail, Password);

        auth.IsSuccess.Should().BeTrue();
        auth.Value.Record.Should().NotBeNull();
        auth.Value.Record.Email.Should().Be(NewEmail);

        // Restore admin session
        adminSession.Should().NotBeNull();
        _pb.AuthStore.Save(adminSession);
    }

    [Fact]
    public async Task ConfirmEmailChangeAsync_WithInvalidToken_ReturnsSuccess()
    {
        Result result = await _pb.Collection(CollectionName)
            .ConfirmEmailChangeAsync("invalid-token", Password);

        // PocketBase security model: still returns success
        result.IsSuccess.Should().BeTrue();
    }
}
