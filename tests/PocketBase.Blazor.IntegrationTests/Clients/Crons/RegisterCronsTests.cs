namespace PocketBase.Blazor.IntegrationTests.Clients.Crons;

using Blazor.Requests;

[Collection("PocketBase.Blazor.Admin")]
public class RegisterCronsTests
{
    private readonly IPocketBase _pb;
    public RegisterCronsTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact(Skip = "Custom self-hosted Pocketbase client is requires for this test")]
    public async Task Register_and_run_cron_successfully()
    {
        // Arrange
        var cronId = "hello";
        var expression = "*/2 * * * *"; // Every 2 minutes
        var payload = new Dictionary<string, object?>
        {
            ["name"] = "Pocketbase.Blazor!",
            ["count"] = 7
        };

        // Note: registering a custom cron only works when
        // using a custom self-hosted extended PocketBase instance

        // Act: register (or update) a cron
        var registerResult = await _pb.Crons.RegisterAsync(
            new CronRegisterRequest
            {
                Id = cronId,
                Expression = expression,
                Payload = payload
            });

        // Assert
        registerResult.IsSuccess.Should().BeTrue();
        registerResult.Value.Should().NotBeNull();
        registerResult.Value!.Id.Should().Be(cronId);
        registerResult.Value!.Status.Should().Be("cron registered");

        // Act: list crons
        var listResult = await _pb.Crons.GetFullListAsync();

        listResult.IsSuccess.Should().BeTrue();
        listResult.Value.Should().Contain(c => c.Id == cronId);
    }
}

