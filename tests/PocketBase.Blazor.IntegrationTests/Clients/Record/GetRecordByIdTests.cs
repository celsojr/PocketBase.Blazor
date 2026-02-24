namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.IntegrationTests.Helpers;
using Blazor.Models;
using Blazor.Responses;

[Trait("Category", "Integration")]
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
        Result<ListResult<RecordResponse>> listResult = await _pb.Collection("users")
            .GetListAsync<RecordResponse>(
                perPage: 1,
                options: new ListOptions()
                {
                    SkipTotal = true,
                }
            );

        listResult.IsSuccess.Should().BeTrue();
        string recordId = listResult.Value.Items.First().Id;

        Result<RecordResponse> getResult = await _pb.Collection("users")
            .GetOneAsync<RecordResponse>(recordId);

        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Id.Should().Be(recordId);
    }

    [Fact]
    public async Task Get_one_record_with_invalid_id_returns_error()
    {
        Result<RecordResponse> result = await _pb.Collection("users")
            .GetOneAsync<RecordResponse>("invalid-id-that-doesnt-exist");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Get_posts_with_expanded_category_should_include_category_data()
    {
        Result<ListResult<PostResponse>> result = await _pb.Collection("posts")
            .GetListAsync<PostResponse>(
                perPage: 1,
                options: new ListOptions()
                {
                    Expand = "category",
                    SkipTotal = true
                });

        result.Value.Items.Should().NotBeEmpty();

        PostResponse post = result.Value.Items.First();

        Helpers.CategoryResponse? category = post.ExpandedCategory;
        category.Should().NotBeNull();
        category!.Name.Should().NotBeNullOrWhiteSpace();
        category.Slug.Should().NotBeNullOrWhiteSpace();
        category.Created?.Kind.Should().Be(DateTimeKind.Utc);
    }
}

