namespace PocketBase.Blazor.IntegrationTests.Infrastructure;

using System.Text.Json.Serialization;
using Blazor.Responses.Auth;

public class PocketBaseAdminFixture : IAsyncLifetime
{
    public IPocketBase Client { get; private set; } = null!;
    public TestSettings Settings { get; } = new();

    public async Task InitializeAsync()
    {
        PocketBaseOptions options = new PocketBaseOptions();
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = true;

        Client = new PocketBase(Settings.BaseUrl, options: options);

        Result<AuthResponse> auth = await Client.Admins
            .AuthWithPasswordAsync(
                Settings.AdminTesterEmail,
                Settings.AdminTesterPassword
            );

        auth.IsSuccess.Should().BeTrue("auth must succeed for integration tests");
    }

    public async Task DisposeAsync() => await Client.DisposeAsync();
}
