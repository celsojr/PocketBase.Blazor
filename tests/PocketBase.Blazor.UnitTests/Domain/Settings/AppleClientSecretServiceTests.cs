namespace PocketBase.Blazor.UnitTests.Domain.Settings;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Blazor.Clients.Settings;
using Blazor.Http;
using Blazor.Requests.Settings;

[Trait("Category", "Unit")]
public class AppleClientSecretServiceTests
{
    private readonly SettingsClient _client;

    public AppleClientSecretServiceTests()
    {
        // This client won't make HTTP calls in these tests
        HttpTransport dummyTransport = new HttpTransport("http://127.0.0.1:8092");
        _client = new SettingsClient(dummyTransport);
    }

    [Fact]
    public void CreateClientSecret_Throws_WhenConfigNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => _client.CreateClientSecret(null!));
    }

    [Theory]
    [InlineData(null, "TEAM123", "KEY123", "PRIVATE_KEY", 3600)]
    [InlineData("", "TEAM123", "KEY123", "PRIVATE_KEY", 3600)]
    [InlineData("com.example.app", null, "KEY123", "PRIVATE_KEY", 3600)]
    [InlineData("com.example.app", "", "KEY123", "PRIVATE_KEY", 3600)]
    [InlineData("com.example.app", "TEAM123", null, "PRIVATE_KEY", 3600)]
    [InlineData("com.example.app", "TEAM123", "", "PRIVATE_KEY", 3600)]
    [InlineData("com.example.app", "TEAM123", "KEY123", null, 3600)]
    [InlineData("com.example.app", "TEAM123", "KEY123", "", 3600)]
    public void CreateClientSecret_Throws_WhenRequiredFieldMissing(string clientId, string teamId, string keyId, string privateKey, int expiresIn)
    {
        ClientSecretConfigRequest config = new ClientSecretConfigRequest
        {
            ClientId = clientId,
            TeamId = teamId,
            KeyId = keyId,
            PrivateKey = privateKey,
            Duration = expiresIn
        };

        Assert.Throws<ArgumentException>(
            () => _client.CreateClientSecret(config));
    }

    [Fact]
    public void CreateClientSecret_Throws_WhenExpiresInInvalid()
    {
        ClientSecretConfigRequest config = new ClientSecretConfigRequest
        {
            ClientId = "com.example.app",
            TeamId = "TEAM123",
            KeyId = "KEY123",
            PrivateKey = "PRIVATE_KEY",
            Duration = 0
        };

        Assert.Throws<ArgumentException>(
            () => _client.CreateClientSecret(config));
    }

    [Fact]
    public void CreateClientSecret_GeneratesValidJwt()
    {
        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        byte[] privateKeyBytes = ecdsa.ExportECPrivateKey();
        string privateKeyBase64 = Convert.ToBase64String(privateKeyBytes);

        ClientSecretConfigRequest config = new ClientSecretConfigRequest
        {
            ClientId = "com.example.app",
            TeamId = "TEAM123ABC",
            KeyId = "KEY123ABCX",
            PrivateKey = privateKeyBase64,
            Duration = 3600
        };

        string secret = _client.CreateClientSecret(config);

        secret.Should().NotBeNullOrEmpty();

        // Verify token structure
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken token = tokenHandler.ReadJwtToken(secret);

        token.Header["kid"].Should().Be(config.KeyId);
        token.Header["alg"].Should().Be("ES256");

        token.Payload["aud"].Should().Be("https://appleid.apple.com");
        token.Payload["iss"].Should().Be(config.TeamId);
        token.Payload["sub"].Should().Be(config.ClientId);
        token.Payload.Should().ContainKey("exp");
    }

    [Fact]
    public void CreateClientSecret_ReturnsJwt_WithCorrectClaims()
    {
        ClientSecretConfigRequest config = new ClientSecretConfigRequest
        {
            ClientId = "com.example.app",
            TeamId = "TEAM123ABC",
            KeyId = "KEY123ABCX",
            PrivateKey = GenerateTestPrivateKey(),
            Duration = 3600
        };

        string secret = _client.CreateClientSecret(config);

        secret.Should().NotBeNullOrEmpty();
        string[] tokenParts = secret.Split('.');
        tokenParts.Length.Should().Be(3);

        // Check header for kid
        Dictionary<string, object>? header = DecodeJwtPart(tokenParts[0]);
        header.Should().NotBeNullOrEmpty();
        header["kid"].ToString().Should().Be(config.KeyId);
        header["alg"].ToString().Should().Be("ES256");

        // Check payload
        Dictionary<string, object>? payload = DecodeJwtPart(tokenParts[1]);
        payload.Should().NotBeNullOrEmpty();
        payload["aud"].ToString().Should().Be("https://appleid.apple.com");
        payload["sub"].ToString().Should().Be(config.ClientId);
        payload["iss"].ToString().Should().Be(config.TeamId);
        payload.Should().ContainKey("exp");
    }

    private static Dictionary<string, object>? DecodeJwtPart(string base64Part)
    {
        byte[] bytes = Convert.FromBase64String(
            base64Part.PadRight(
                base64Part.Length + (4 - base64Part.Length % 4) % 4, '='));
        string json = Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
    }

    private static string GenerateTestPrivateKey()
    {
        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        return Convert.ToBase64String(ecdsa.ExportECPrivateKey());
    }
}

