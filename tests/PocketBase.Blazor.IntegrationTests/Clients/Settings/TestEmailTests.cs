namespace PocketBase.Blazor.IntegrationTests.Clients.Settings;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class TestEmailTests
{
    private readonly IPocketBase _pb;

    public TestEmailTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task TestEmailAsync_Throws_WhenEmailEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _pb.Settings.TestEmailAsync("users", "", "verification"));
    }

    [Fact]
    public async Task TestEmailAsync_Throws_WhenTemplateEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _pb.Settings.TestEmailAsync("users", "test@example.com", ""));
    }

    [Fact]
    public async Task TestEmailAsync_Fails_WhenSmtpNotConfigured()
    {
        var result = await _pb.Settings.TestEmailAsync("users", "test@example.com", "verification");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    [Trait("Requires", "SMTP")]
    public async Task TestEmailAsync_Succeeds_WhenSmtpConfigured()
    {
        // docker run -d -p 1027:1025 -p 8027:8025 mailhog/mailhog
        // MailHog web UI: http://localhost:8027

        // Configure SMTP first
        await _pb.Settings.UpdateAsync(new
        {
            smtp = new
            {
                enabled = true,
                host = "localhost",
                port = 1027, // MailHog SMTP port (8027 is for web UI)
                tls = false
            }
        });

        // Null collection name or id fallbacks to _superusers collection if not set
        var result = await _pb.Settings.TestEmailAsync(null!, "recipient@example.com", "verification");
    
        result.IsSuccess.Should().BeTrue();
    }
}

