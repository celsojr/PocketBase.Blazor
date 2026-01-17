namespace PocketBase.Blazor.IntegrationTests.Clients.Crons;

using Blazor.Requests;

[Collection("PocketBase.Blazor.Admin")]
public class CreateCronsTests
{
    private readonly IPocketBase _pb;
    public CreateCronsTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    //[Fact]
    //public async Task Register_and_run_cron_successfully()
    //{
    //    // Arrange
    //    var cronId = "hello";
    //    var expression = "*/5 * * * *";

    //    // Act: register cron
    //    var registerResult = await _pb.Crons.RegisterAsync(
    //        new CronRegisterRequest
    //        {
    //            Id = cronId,
    //            Expression = expression
    //        });

    //    // Assert registration
    //    registerResult.IsSuccess.Should().BeTrue();
    //    registerResult.Value.Should().NotBeNull();
    //    registerResult.Value!.Id.Should().Be(cronId);

    //    // Act: list crons
    //    var listResult = await _pb.Crons.GetFullListAsync();

    //    listResult.IsSuccess.Should().BeTrue();
    //    listResult.Value.Should().Contain(c => c.Id == cronId);

    //    // Act: run cron manually
    //    var runResult = await _pb.Crons.RunAsync(cronId);

    //    runResult.IsSuccess.Should().BeTrue();
    //}
}

