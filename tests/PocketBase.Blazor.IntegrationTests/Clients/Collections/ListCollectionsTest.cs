namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

using Blazor.Models;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class ListCollectionsTest
{
    private readonly IPocketBase _pb;

    public ListCollectionsTest(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task List_collections()
    {
        var result = await _pb
            .Collections
            .GetListAsync<CollectionModel>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task List_collections_with_pagination()
    {
        // Test first page
        var result = await _pb.Collections
            .GetListAsync<CollectionModel>(page: 1, perPage: 5);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCountLessThanOrEqualTo(5);
        result.Value.Page.Should().Be(1);
        result.Value.PerPage.Should().Be(5);
        result.Value.TotalItems.Should().BeGreaterThan(0);
        result.Value.TotalPages.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task List_collections_with_different_page_sizes()
    {
        var result1 = await _pb.Collections
            .GetListAsync<CollectionModel>(perPage: 1);

        var result5 = await _pb.Collections
            .GetListAsync<CollectionModel>(perPage: 5);

        result1.IsSuccess.Should().BeTrue();
        result5.IsSuccess.Should().BeTrue();

        // Should get different amounts of items
        result1.Value.Items.Should().HaveCount(1);
        result5.Value.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task List_collections_with_filter()
    {
        // Assuming there's at least one system collection
        var result = await _pb.Collections
            .GetListAsync<CollectionModel>(options: new ListOptions { Filter = "system = true" });

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();

        // All returned collections should be system collections
        result.Value.Items.Should().AllSatisfy(c => c.System.Should().BeTrue());
    }

    [Fact]
    public async Task List_collections_with_name_filter()
    {
        // Get all collections first to find a known name
        var allCollections = await _pb.Collections
            .GetListAsync<CollectionModel>();

        var knownCollection = allCollections.Value.Items.First();

        var result = await _pb.Collections
            .GetListAsync<CollectionModel>(options: new ListOptions { Filter = $"name = '{knownCollection.Name}'" });

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items.First().Name.Should().Be(knownCollection.Name);
    }

    [Fact]
    public async Task List_collections_sorted_by_name_ascending()
    {
        var result = await _pb.Collections
            .GetListAsync<CollectionModel>(options: new ListOptions { Sort = "+name" });

        result.IsSuccess.Should().BeTrue();

        // Verify items are in alphabetical order
        var names = result.Value.Items.Select(c => c.Name).ToList();
        var sortedNames = names.OrderBy(n => n).ToList();
        names.Should().Equal(sortedNames);
    }

    [Fact]
    public async Task List_collections_sorted_by_created_date_descending()
    {
        var result = await _pb.Collections
            .GetListAsync<CollectionModel>(options: new ListOptions { Sort = "-created" });

        result.IsSuccess.Should().BeTrue();

        // Verify items are in reverse chronological order
        var createdDates = result.Value.Items.Select(c => c.Created).ToList();
        var sortedDates = createdDates.OrderByDescending(d => d).ToList();
        createdDates.Should().Equal(sortedDates);
    }

    [Fact]
    public async Task List_collections_with_invalid_filter_returns_error()
    {
        var result = await _pb.Collections
            .GetListAsync<CollectionModel>(options: new ListOptions { Filter = "invalidField = 'value'" });

        // Depending on PocketBase implementation, might return error or empty
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task List_collections_with_invalid_page_returns_empty_or_error()
    {
        var result = await _pb.Collections
            .GetListAsync<CollectionModel>(page: 9999); // Non-existent page

        // Should either return empty or error
        if (result.IsSuccess)
        {
            result.Value.Items.Should().BeEmpty();
            result.Value.Page.Should().Be(9999);
        }
        else
        {
            result.IsSuccess.Should().BeFalse();
        }
    }

    [Fact]
    public async Task List_collections_respects_cancellation_token()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(0); // Cancel immediately

        Func<Task> act = async () => await _pb.Collections
            .GetListAsync<CollectionModel>(cancellationToken: cts.Token);

        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task List_collections_returns_correct_model_type()
    {
        var result = await _pb.Collections
            .GetListAsync<CollectionModel>();

        result.IsSuccess.Should().BeTrue();

        // Verify all items have required properties
        result.Value.Items.Should().AllSatisfy(collection =>
        {
            collection.Id.Should().NotBeNullOrEmpty();
            collection.Name.Should().NotBeNullOrEmpty();
            collection.Type.Should().NotBeNullOrEmpty();
            collection.Created.Should().NotBe(default);
            collection.Updated.Should().NotBe(default);
        });
    }

    [Fact]
    public async Task List_collections_can_return_different_model_type()
    {
        // Test with a minimal model if you have different use cases
        var result = await _pb.Collections
            .GetListAsync<MinimalCollectionModel>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();
    }

    public record MinimalCollectionModel(string Id, string Name);
}

