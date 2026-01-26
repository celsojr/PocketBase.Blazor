namespace PocketBase.Blazor.IntegrationTests.Helpers;

using System.Text;

public static class JwtTokenValidator
{
    public static DateTimeOffset? GetTokenExpiry(string token)
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

