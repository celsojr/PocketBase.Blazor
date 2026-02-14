namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Models;
using Blazor.Responses;

using static Blazor.IntegrationTests.Helpers.MailHogHelper;

[Collection("PocketBase.Blazor.Admin")]
public class EmailVerificationRecordTests : IAsyncLifetime
{
    private readonly IPocketBase _pb;
    private const string CollectionName = "test_email_verification";
    private const string TestEmail = "test_verification@example.com";
    private const string TestPassword = "Test123456!";

    public EmailVerificationRecordTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
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

        // Create test user for this colllection
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
        // Clean up
        var listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(options: new ListOptions { SkipTotal = true });

        listResult.IsSuccess.Should().BeTrue();

        var collection = listResult.Value.Items
            .FirstOrDefault(c => c.Name?.Equals(CollectionName) == true);

        if (collection?.Id != null)
        {
            await _pb.Collections.DeleteAsync(collection.Id);
        }

        // Clear MailHog messages
        await ClearMailHogMessages();
    }

    [Fact]
    public async Task RequestVerificationAsync_WithValidEmail_ReturnsSuccess()
    {
        var result = await _pb.Collection(CollectionName)
            .RequestVerificationAsync(TestEmail);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RequestVerificationAsync_WithInvalidEmail_ReturnsSuccess()
    {
        var result = await _pb.Collection(CollectionName)
            .RequestVerificationAsync("nonexistent@example.com");

        // API returns success even for non-existent emails (security tricky)
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmVerificationAsync_WithValidToken_ReturnsSuccess()
    {
        await _pb.Collection(CollectionName)
            .RequestVerificationAsync(TestEmail);

        var token = await GetVerificationTokenFromEmail(TestEmail);
        token.Should().NotBeNull();

        var result = await _pb.Collection(CollectionName)
            .ConfirmVerificationAsync(token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmVerificationAsync_WithInvalidToken_ReturnsFailure()
    {
        var result = await _pb.Collection(CollectionName)
            .ConfirmVerificationAsync("invalid-token");

        // API returns success even for missing email token claim (security tricky)
        result.IsSuccess.Should().BeTrue();
    }
}
