namespace PocketBase.Blazor.IntegrationTests.Clients.Health;

using Responses;
using System.Net;

[Collection("PocketBase.Blazor Clients")]
public class HealthCheckTests
{
    private readonly IPocketBase _pb;

    public HealthCheckTests(PocketBaseUserFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task Health_check_returns_ok()
    {
        var result = await _pb.Health.CheckAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<HealthCheckResponse>();

        result.Value.Code.Should().Be((int)HttpStatusCode.OK);
        result.Value.Message.Should().Be("API is healthy.");
        result.Value.Data.Should().BeOfType<JsonElement>();
    }
}

