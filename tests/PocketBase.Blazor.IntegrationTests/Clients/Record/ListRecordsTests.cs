namespace PocketBase.Blazor.IntegrationTests.Clients.Record;

using Blazor.Models;
using IntegrationTests.Helpers;
using Responses;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.User")]
public class ListRecordsTests
{
    private readonly IPocketBase _pb;

    public ListRecordsTests(PocketBaseUserFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task List_posts_returns_items_as_objects()
    {
        Result<ListResult<object>> result = await _pb
            .Collection("posts")
            .GetListAsync<object>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();
        result.Value.Items.First()
            .ShouldBeJsonObject()
            .HaveProperty("id")
            .HaveProperty("created")
            .HaveProperty("updated")
            .HaveProperty("collectionId")
            .HaveProperty("collectionName");
    }

    [Fact]
    public async Task List_posts_returns_items()
    {
        Result<ListResult<RecordResponse>> result = await _pb
            .Collection("posts")
            .GetListAsync<RecordResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetList_respects_page_and_perPage()
    {
        Result<ListResult<RecordResponse>> result = await _pb
            .Collection("posts")
            .GetListAsync<RecordResponse>(page: 1, perPage: 3);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.Page.Should().Be(1);
        result.Value.PerPage.Should().Be(3);
        result.Value.TotalItems.Should().BeGreaterThan(3);
    }

    [Fact]
    public async Task GetList_applies_filter()
    {
        Result<ListResult<RecordResponse>> result = await _pb
            .Collection("posts")
            .GetListAsync<RecordResponse>(
                options: new ListOptions
                {
                    Filter = "created > \"2025-12-21 15:30:03.028Z\"",
                    SkipTotal = true // Performance optimization
                });

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();
        result.Value.TotalItems.Should().Be(-1);
    }

    [Fact]
    public async Task GetList_requires_auth_when_rules_apply()
    {
        await using PocketBase unauthenticatedPb = new PocketBase(_pb.BaseUrl);

        Result<ListResult<RecordResponse>> result = await unauthenticatedPb
            .Collection("posts")
            .GetListAsync<RecordResponse>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
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
        post.Expand.Should().NotBeNull();
        post.Expand.Should().ContainKey("category");

        JsonElement? fieldExpansion = post.Expand["category"];
        fieldExpansion.Should().NotBeNull();

        PocketBaseOptions options = new PocketBaseOptions();
        options.JsonSerializerOptions.WriteIndented = true;

        string fieldJson = JsonSerializer.Serialize(fieldExpansion, options.JsonSerializerOptions);
        CategoryResponse? category = JsonSerializer.Deserialize<CategoryResponse>(fieldJson, options.JsonSerializerOptions);

        category.Should().NotBeNull();
        category.Name.Should().NotBeNullOrWhiteSpace();
        category.Slug.Should().NotBeNullOrWhiteSpace();
        category.Created.Kind.Should().Be(DateTimeKind.Utc);
    }
}

record CategoryResponse(string Name, string Slug, DateTime Created);
