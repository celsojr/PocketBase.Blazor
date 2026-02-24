namespace PocketBase.Blazor.IntegrationTests.Clients.Crons;

using Blazor.Requests;
using Blazor.Responses.Cron;

[Trait("Category", "Integration")]
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
        string cronId = "hello";
        string expression = "*/2 * * * *"; // Every 2 minutes
        Dictionary<string, object?> payload = new Dictionary<string, object?>
        {
            ["name"] = "Pocketbase.Blazor!",
            ["count"] = 7
        };

        // Note: registering a custom cron only works when
        // using a custom self-hosted extended PocketBase instance

        // Act: register (or update) a cron
        Result<CronRegisterResponse> registerResult = await _pb.Crons.RegisterAsync(
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
        Result<IEnumerable<CronsResponse>> listResult = await _pb.Crons.GetFullListAsync();

        listResult.IsSuccess.Should().BeTrue();
        listResult.Value.Should().Contain(c => c.Id == cronId);
    }
}
