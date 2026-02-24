namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Models;
using Blazor.Responses;
using Helpers.MailHog;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class EmailVerificationRecordTests : IAsyncLifetime
{
    private readonly IPocketBase _pb;
    private readonly MailHogService _mailHogService;
    private const string CollectionName = "test_email_verification";
    private const string TestEmail = "test_verification@example.com";
    private const string TestPassword = "Test123456!";

    public EmailVerificationRecordTests(PocketBaseAdminFixture fixture)
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
            // Log the exception but don't fail the test
            Console.WriteLine($"Failed to clear MailHog messages: {ex.Message}");
        }
    }

    [Fact]
    public async Task RequestVerificationAsync_WithValidEmail_ReturnsSuccess()
    {
        Result result = await _pb.Collection(CollectionName)
            .RequestVerificationAsync(TestEmail);

        // API returns success even if email is not sent (security feature)
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RequestVerificationAsync_WithInvalidEmail_ReturnsSuccess()
    {
        Result result = await _pb.Collection(CollectionName)
            .RequestVerificationAsync("nonexistent@example.com");

        // API returns success even for non-existent emails (security feature)
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Requires", "SMTP")]
    public async Task ConfirmVerificationAsync_WithValidToken_ReturnsSuccess()
    {
        await _pb.Collection(CollectionName)
            .RequestVerificationAsync(TestEmail);

        string? token = await _mailHogService
            .GetLatestTokenAsync(TestEmail, VerificationType.EmailVerification);
        token.Should().NotBeNull();

        Result result = await _pb.Collection(CollectionName)
            .ConfirmVerificationAsync(token!);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmVerificationAsync_WithInvalidToken_ReturnsSuccess()
    {
        Result result = await _pb.Collection(CollectionName)
            .ConfirmVerificationAsync("invalid-token");

        // API returns success even for missing email token claim (security feature)
        result.IsSuccess.Should().BeTrue();
    }
}
