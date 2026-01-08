namespace PocketBase.Blazor.IntegrationTests.Infrastructure;

using System.Text.Json.Serialization;

public sealed class PocketBaseUserFixture : IAsyncLifetime
{
    public IPocketBase Client { get; private set; } = null!;
    public TestSettings Settings { get; } = new();

    public async Task InitializeAsync()
    {
        var options = new PocketBaseOptions();
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = true;

        Client = new PocketBase(Settings.BaseUrl, options: options);

        var auth = await Client.Collection("users")
            .AuthWithPasswordAsync(
                Settings.UserTesterEmail,
                Settings.UserTesterPassword 
            );

        auth.IsSuccess.Should().BeTrue("auth must succeed for integration tests");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

