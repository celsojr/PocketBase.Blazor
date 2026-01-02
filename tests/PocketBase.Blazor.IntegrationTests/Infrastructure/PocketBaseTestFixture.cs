namespace PocketBase.Blazor.IntegrationTests.Infrastructure;

public sealed class PocketBaseTestFixture : IAsyncLifetime
{
    public IPocketBase Client { get; private set; } = null!;
    public TestSettings Settings { get; } = new();

    public async Task InitializeAsync()
    {
        var options = new PocketBaseOptions();
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

        Client = new PocketBase(Settings.BaseUrl, options: options);

        var auth = await Client.Admins
            .AuthWithPasswordAsync(
                Settings.UserTesterEmail,
                Settings.UserTesterPassword 
            );

        auth.IsSuccess.Should().BeTrue("auth must succeed for integration tests");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

