namespace PocketBase.Blazor.IntegrationTests.Clients.Settings;

using Blazor.Requests.Settings;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class GenerateAppleClientSecretTests
{
    private readonly IPocketBase _pb;

    public GenerateAppleClientSecretTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task GenerateAppleClientSecretAsync_Throws_WhenClientIdEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _pb.Settings.GenerateAppleClientSecretAsync(
                new ClientSecretConfigRequest
                {
                    ClientId = "",
                    TeamId = "TEAM123",
                    KeyId = "KEY123",
                    PrivateKey = "PRIVATE_KEY",
                    Duration = 3600
                }));
    }

    [Fact]
    public async Task GenerateAppleClientSecretAsync_Throws_WhenTeamIdEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _pb.Settings.GenerateAppleClientSecretAsync(
                new ClientSecretConfigRequest
                {
                    ClientId = "com.example.app",
                    TeamId = "",
                    KeyId = "KEY123",
                    PrivateKey = "PRIVATE_KEY",
                    Duration = 3600
                }));
    }

    [Fact]
    public async Task GenerateAppleClientSecretAsync_Throws_WhenKeyIdEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _pb.Settings.GenerateAppleClientSecretAsync(
                new ClientSecretConfigRequest
                {
                    ClientId = "com.example.app",
                    TeamId = "TEAM123",
                    KeyId = "",
                    PrivateKey = "PRIVATE_KEY",
                    Duration = 3600
                }));
    }

    [Fact]
    public async Task GenerateAppleClientSecretAsync_Throws_WhenPrivateKeyEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _pb.Settings.GenerateAppleClientSecretAsync(
                new ClientSecretConfigRequest
                {
                    ClientId = "com.example.app",
                    TeamId = "TEAM123",
                    KeyId = "KEY123",
                    PrivateKey = "",
                    Duration = 3600
                }));
    }

    [Fact]
    public async Task GenerateAppleClientSecretAsync_Throws_WhenDurationInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _pb.Settings.GenerateAppleClientSecretAsync(
                new ClientSecretConfigRequest
                {
                    ClientId = "com.example.app",
                    TeamId = "TEAM123",
                    KeyId = "KEY123",
                    PrivateKey = "PRIVATE_KEY",
                    Duration = 0
                }));

        await Assert.ThrowsAsync<ArgumentException>(
            () => _pb.Settings.GenerateAppleClientSecretAsync(
                new ClientSecretConfigRequest
                {
                    ClientId = "com.example.app",
                    TeamId = "TEAM123",
                    KeyId = "KEY123",
                    PrivateKey = "PRIVATE_KEY",
                    Duration = -1
                }));
    }

    [Fact]
    public async Task GenerateAppleClientSecretAsync_Fails_WithInvalidCredentials()
    {
        var result = await _pb.Settings.GenerateAppleClientSecretAsync(
            new ClientSecretConfigRequest
            {
                ClientId = "com.example.app",
                TeamId = "INVALID_TEAM",
                KeyId = "INVALID_KEY",
                PrivateKey = "INVALID_PRIVATE_KEY",
                Duration = 3600
            });

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact(Skip = "Requires real Apple Developer credentials")]
    public async Task GenerateAppleClientSecretAsync_RealTest()
    {
        // SECURITY ALERT: Private keys should never be stored in plain text.
        // Use secure storage like Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault.
        // Environment variables can be exposed in logs, debugging tools, and process listings.

        // DO NOT COMMIT REAL KEYS TO SOURCE CONTROL
        // Use .gitignore for local secret files
        // Rotate keys regularly

        var clientId = Environment.GetEnvironmentVariable("APPLE_CLIENT_ID");
        var teamId = Environment.GetEnvironmentVariable("APPLE_TEAM_ID");
        var keyId = Environment.GetEnvironmentVariable("APPLE_KEY_ID");
        var privateKey = Environment.GetEnvironmentVariable("APPLE_PRIVATE_KEY");

        //var privateKey = await File.ReadAllTextAsync("/etc/secrets/apple-private-key.pem");
        // Set file permissions: chmod 600 /etc/secrets/apple-private-key.pem

        var result = await _pb.Settings.GenerateAppleClientSecretAsync(
            new ClientSecretConfigRequest
            {
                ClientId = clientId,
                TeamId = teamId,
                KeyId = keyId,
                PrivateKey = privateKey,
                Duration = 3600
            });

        result.IsSuccess.Should().BeTrue();
        result.Value.Secret.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateAppleClientSecretAsync_Fails_WhenDurationTooLarge()
    {
        var result = await _pb.Settings.GenerateAppleClientSecretAsync(
            new ClientSecretConfigRequest
            {
                ClientId = "com.example.app",
                TeamId = "TEAM123",
                KeyId = "KEY123",
                PrivateKey = "PRIVATE_KEY",
                Duration = 15777001 // > max 15777000
            });

        result.IsSuccess.Should().BeFalse();
    }
}

