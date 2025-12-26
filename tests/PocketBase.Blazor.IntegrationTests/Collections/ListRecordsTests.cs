namespace PocketBase.Blazor.IntegrationTests.Collections;

[Collection("PocketBase collection")]
public class ListRecordsTests
{
    private readonly IPocketBase _pb;

    public ListRecordsTests(PocketBaseTestFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task List_posts_returns_items()
    {
        var result = await _pb
            .Collection("posts")
            .GetListAsync<Post>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();
    }

}

class Post
{
}

