namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Responses;

[Collection("PocketBase.Blazor.User")]
public class GetRecordByIdTests
{
    private readonly IPocketBase _pb;

    public GetRecordByIdTests(PocketBaseUserFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task Get_one_record_by_id()
    {
        var listResult = await _pb.Collection("users")
            .GetListAsync<RecordResponse>(
                perPage: 1,
                options: new ListOptions()
                {
                    SkipTotal = true,
                }
            );

        listResult.IsSuccess.Should().BeTrue();
        var recordId = listResult.Value.Items.First().Id;

        var getResult = await _pb.Collection("users")
            .GetOneAsync<RecordResponse>(recordId);

        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Id.Should().Be(recordId);
    }

    [Fact]
    public async Task Get_one_record_with_invalid_id_returns_error()
    {
        var result = await _pb.Collection("users")
            .GetOneAsync<RecordResponse>("invalid-id-that-doesnt-exist");

        result.IsSuccess.Should().BeFalse();
    }
}

