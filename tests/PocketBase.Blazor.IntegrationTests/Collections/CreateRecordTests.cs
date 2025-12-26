namespace PocketBase.Blazor.IntegrationTests.Collections;

[Collection("PocketBase collection")]
public class CreateRecordTests
{
    private readonly IPocketBase _pb;

    public CreateRecordTests(PocketBaseTestFixture fixture)
    {
        _pb = fixture.Client;
    }


}
