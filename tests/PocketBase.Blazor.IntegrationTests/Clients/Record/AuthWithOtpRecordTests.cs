namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

[Collection("PocketBase.Blazor.User")]
public class AuthWithOtpRecordTests
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseUserFixture _fixture;

    public AuthWithOtpRecordTests(PocketBaseUserFixture fixture)
    {
        _fixture = fixture;
        _pb = fixture.Client;
    }
}
