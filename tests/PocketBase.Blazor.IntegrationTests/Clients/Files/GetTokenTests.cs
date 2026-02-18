namespace PocketBase.Blazor.IntegrationTests.Clients.Files;

using System.Text;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class GetTokenTests
{
    private readonly IPocketBase _pb;

    public GetTokenTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task GetToken_ReturnsToken_ForValidParameters()
    {
        var result = await _pb.Files.GetTokenAsync();
        result.Value.Should().NotBeEmpty();

        var tokenParts = result.Value.Split('.');
        tokenParts.Should().HaveCount(3, "JWT tokens should have 3 parts");

        var expiry = GetTokenExpiry(result.Value);
        expiry.Should().BeAfter(DateTimeOffset.Now, "Token should not be expired");

        // Verify it's short-lived
        var timeUntilExpiry = expiry - DateTimeOffset.UtcNow;
        timeUntilExpiry.Should().BeGreaterThan(TimeSpan.FromMinutes(1));
        timeUntilExpiry.Should().BeLessThan(TimeSpan.FromMinutes(10));
    }

    private static DateTimeOffset? GetTokenExpiry(string token)
    {
        var parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return null;

        var rawPayload = parts[1];
        var payload = Encoding.UTF8.GetString(ParsePayload(rawPayload));
        var encoded = JsonSerializer.Deserialize<IDictionary<string, object>>(payload)!;

        if (encoded["exp"] is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
        {
            var exp = jsonElement.GetInt32();
            var expiredAt = DateTimeOffset.FromUnixTimeSeconds(exp);
            return expiredAt;
        }
        return null;
    }

    private static byte[] ParsePayload(string payload)
    {
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }
        return Convert.FromBase64String(payload);
    }
}
