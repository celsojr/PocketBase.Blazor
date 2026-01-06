namespace PocketBase.Blazor.IntegrationTests.Infrastructure;

using System.Text.Json.Serialization;

public class PocketBaseAdminFixture : IAsyncLifetime
{
    public IPocketBase Client { get; private set; } = null!;
    public TestSettings Settings { get; } = new();

    public async Task InitializeAsync()
    {
        var options = new PocketBaseOptions();
        //options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.JsonSerializerOptions.WriteIndented = true;

        Client = new PocketBase(Settings.BaseUrl, options: options);

        var auth = await Client.Admins
            .AuthWithPasswordAsync(
                Settings.AdminTesterEmail,
                Settings.AdminTesterPassword
            );

        auth.IsSuccess.Should().BeTrue("auth must succeed for integration tests");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

