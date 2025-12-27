namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

[Collection("PocketBase.Blazor Clients")]
public class CreateRecordTests
{
    private readonly IPocketBase _pb;

    public CreateRecordTests(PocketBaseTestFixture fixture)
    {
        _pb = fixture.Client;
    }


}
