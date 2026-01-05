namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

[Collection("PocketBase.Blazor Clients")]
public class CreateRecordTests
{
    private readonly IPocketBase _pb;

    public CreateRecordTests(PocketBaseUserFixture fixture)
    {
        _pb = fixture.Client;
    }
}

