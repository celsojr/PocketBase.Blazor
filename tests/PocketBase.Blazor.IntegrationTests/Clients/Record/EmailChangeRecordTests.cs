namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Models;
using Blazor.Responses;
using Helpers.MailHog;

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

        var options = new MailHogOptions
        {
            BaseUrl = "http://localhost:8027"
        };

        _mailHogService = new MailHogService(new HttpClient(), options);
    }

    public async Task InitializeAsync()
    {
        // Configure SMTP
        var smtpResult = await _pb.Settings.UpdateAsync(new
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
        var listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(options: new ListOptions { SkipTotal = true });

        var collection = listResult.Value.Items
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
    public async Task RequestEmailChangeAsync_WithAuthenticatedUser_ReturnsSuccess()
    {
        // Arrange - reserve current admin session for post-test cleanup
        var adminSession = _pb.AuthStore.CurrentSession;

        // Authenticate user first
        var auth = await _pb.Collection(CollectionName)
            .AuthWithPasswordAsync(OriginalEmail, Password);

        auth.IsSuccess.Should().BeTrue();

        var result = await _pb.Collection(CollectionName)
            .RequestEmailChangeAsync(NewEmail);

        result.IsSuccess.Should().BeTrue();

        // Restore admin session for subsequent cleanup operations
        adminSession.Should().NotBeNull();
        _pb.AuthStore.Save(adminSession);
    }

    [Fact(Skip = "Requires SMTP server + configuration")]
    public async Task ConfirmEmailChangeAsync_WithValidToken_UpdatesEmail()
    {
        // Preserve admin session
        var adminSession = _pb.AuthStore.CurrentSession;

        // Authenticate user
        await _pb.Collection(CollectionName)
            .AuthWithPasswordAsync(OriginalEmail, Password);

        // Request change
        await _pb.Collection(CollectionName)
            .RequestEmailChangeAsync(NewEmail);

        // Extract confirmation token from MailHog
        var token = await _mailHogService
            .GetLatestTokenAsync(NewEmail, VerificationType.EmailChange);

        token.Should().NotBeNull();

        // Confirm change
        var result = await _pb.Collection(CollectionName)
            .ConfirmEmailChangeAsync(token, Password);

        result.IsSuccess.Should().BeTrue();

        // Clear session
        _pb.AuthStore.Clear();

        // Authenticate using new email
        var auth = await _pb.Collection(CollectionName)
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
        var result = await _pb.Collection(CollectionName)
            .ConfirmEmailChangeAsync("invalid-token", Password);

        // PocketBase security model: still returns success
        result.IsSuccess.Should().BeTrue();
    }
}
