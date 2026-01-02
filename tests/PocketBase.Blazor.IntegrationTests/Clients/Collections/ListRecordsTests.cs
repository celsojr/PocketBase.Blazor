namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

//using Requests;
//using Responses;
//using IntegrationTests.Helpers;
using Models;

[Collection("PocketBase.Blazor Clients")]
public class ListRecordsTests
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseTestFixture _fixture;

    public ListRecordsTests(PocketBaseTestFixture fixture)
    {
        _fixture = fixture;
        _pb = fixture.Client;
    }

    [Fact]
    public async Task List_collections()
    {
        var adminPb = new PocketBase(_pb.BaseUrl);

        _ = await adminPb.Admins
            .AuthWithPasswordAsync(
                _fixture.Settings.AdminTesterEmail,
                _fixture.Settings.AdminTesterPassword,
                isAdmin: true
            );

        var result = await adminPb
            .Collections
            .GetListAsync<CollectionModel>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();
    }

    //[Fact]
    //public async Task List_posts_returns_items_as_objects()
    //{
    //    var result = await _pb
    //        .Collection("posts")
    //        .GetListAsync<object>();

    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Items.Should().NotBeEmpty();
    //    result.Value.Items.First()
    //        .ShouldBeJsonObject()
    //        .HaveProperty("id")
    //        .HaveProperty("created")
    //        .HaveProperty("updated")
    //        .HaveProperty("collectionId")
    //        .HaveProperty("collectionName");
    //}

    //[Fact]
    //public async Task List_posts_returns_items()
    //{
    //    var result = await _pb
    //        .Collection("posts")
    //        .GetListAsync<RecordResponse>();

    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Items.Should().NotBeEmpty();
    //}

    //[Fact]
    //public async Task GetList_respects_page_and_perPage()
    //{
    //    var result = await _pb
    //        .Collection("posts")
    //        .GetListAsync<RecordResponse>(page: 1, perPage: 3);

    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Items.Should().HaveCount(3);
    //    result.Value.Page.Should().Be(1);
    //    result.Value.PerPage.Should().Be(3);
    //    result.Value.TotalItems.Should().BeGreaterThan(3);
    //}

    //[Fact]
    //public async Task GetList_applies_filter()
    //{
    //    var result = await _pb
    //        .Collection("posts")
    //        .GetListAsync<RecordResponse>(
    //            options: new QueryOptionsRequest
    //            {
    //                Filter = "created > \"2025-12-21 15:30:03.028Z\""
    //            });

    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Items.Should().NotBeEmpty();
    //}

    //[Fact]
    //public async Task GetList_requires_auth_when_rules_apply()
    //{
    //    var unauthenticatedPb = new PocketBase(_pb.BaseUrl);

    //    var result = await unauthenticatedPb
    //        .Collection("posts")
    //        .GetListAsync<RecordResponse>();

    //    result.IsSuccess.Should().BeTrue();
    //    result.Value.Items.Should().BeEmpty();
    //}

}

