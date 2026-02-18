namespace PocketBase.Blazor.IntegrationTests.Clients;

using System.Threading.Tasks;
using Blazor.IntegrationTests.Helpers;
using Blazor.Responses.Auth;
using Xunit.Abstractions;

[Trait("Category", "Integration")]
public class RawPBClientTests
{
    private readonly ITestOutputHelper _output;

    public RawPBClientTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DebugWithRawHttpClient()
    {
        // First authenticate to get token
        var authClient = new RawPocketBaseClient("http://127.0.0.1:8092", "");
        var authResult = await authClient.PostAsync<AuthResponse>(
            "api/collections/_superusers/auth-with-password", new
            {
                identity = "admin_tester@email.com",
                password = "gDxTCmnq7K8xyKn"
            });

        var token = authResult?.Token; // Adjust based on actual response structure
        token.Should().NotBeNullOrEmpty();

        // Now use token for auth methods request
        var client = new RawPocketBaseClient("http://127.0.0.1:8092", token);

        // Get raw JSON to see what's coming back
        var rawJson = await client.GetRawAsync("api/collections");

        // Write to test output
        rawJson.Should().NotBeNullOrEmpty();
        _output.WriteLine(rawJson);

        // Deserialize if needed
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        var deserialized = JsonSerializer.Deserialize<AuthMethodsResponse>(rawJson, options);
        deserialized.Should().NotBeNull();
        deserialized?.Oauth2?.Providers.Should().NotBeNull();
    }
}
