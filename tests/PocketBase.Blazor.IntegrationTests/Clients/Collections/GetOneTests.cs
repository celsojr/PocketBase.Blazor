namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

using System.Diagnostics;
using System.Net;
using Blazor.IntegrationTests.Helpers;
using Blazor.Models;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class GetOneTests
{
    private readonly IPocketBase _pb;

    public GetOneTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task Get_one_collection_by_id()
    {
        var listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        listResult.IsSuccess.Should().BeTrue();
        var collectionId = listResult.Value.Items.First().Id;

        var getResult = await _pb.Collections.GetOneAsync<CollectionModel>(collectionId);
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Id.Should().Be(collectionId);
    }

    [Fact]
    public async Task Get_one_with_invalid_id_returns_error()
    {
        var result = await _pb.Collections
            .GetOneAsync<CollectionModel>("invalid-id-that-doesnt-exist");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Get_one_with_empty_id_returns_error_result()
    {
        var result = await _pb.Collections
            .GetOneAsync<CollectionModel>("");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();

        result.Errors[0].Message.Should().Contain("Missing required item id");
        result.Errors[0].Metadata.GetValueOrDefault("status")
            .Should().Be((int)HttpStatusCode.NotFound, new BoxedIntEqualityComparer());
    }

    [Fact]
    public async Task Get_one_with_whitespace_id_returns_error_result()
    {
        var result = await _pb.Collections
            .GetOneAsync<CollectionModel>("   ");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();
 
        result.Errors[0].Message.Should().Contain("Missing required item id");
        result.Errors[0].Metadata.GetValueOrDefault("status")
            .Should().Be((int)HttpStatusCode.NotFound, new BoxedIntEqualityComparer());
    }

    [Fact]
    public async Task Get_one_with_null_id_returns_error_result()
    {
        var result = await _pb.Collections
            .GetOneAsync<CollectionModel>(null!);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        
        result.Errors[0].Message.Should().Contain("Missing required item id");
        result.Errors[0].Metadata.GetValueOrDefault("status")
            .Should().Be((int)HttpStatusCode.NotFound, new BoxedIntEqualityComparer());
    }

    [Fact]
    public async Task Get_one_with_fields_option()
    {
        var listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        var collectionId = listResult.Value.Items.First().Id;

        var result = await _pb.Collections.GetOneAsync<CollectionModel>(
            collectionId,
            options: new ListOptions { Fields = "id,name,created" });

        result.IsSuccess.Should().BeTrue();

        result.Value.Id.Should().NotBeNullOrEmpty();
        result.Value.Name.Should().NotBeNullOrEmpty();
        result.Value.Created.Should().NotBe(default);
    }

    [Fact]
    public async Task Get_one_returns_complete_model_data()
    {
        var listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        var collection = listResult.Value.Items.First();

        var getResult = await _pb.Collections
            .GetOneAsync<CollectionModel>(collection.Id);

        getResult.IsSuccess.Should().BeTrue();

        getResult.Value.Id.Should().Be(collection.Id);
        getResult.Value.Name.Should().Be(collection.Name);
        getResult.Value.Type.Should().Be(collection.Type);
        getResult.Value.System.Should().Be(collection.System);
        getResult.Value.Created.Should().Be(collection.Created);
        getResult.Value.Updated.Should().Be(collection.Updated);
    }

    [Fact]
    public async Task Get_one_with_custom_model_type()
    {
        var listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        var collectionId = listResult.Value.Items.First().Id;

        var result = await _pb.Collections
            .GetOneAsync<MinimalCollectionModel>(collectionId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(collectionId);
        result.Value.Name.Should().NotBeNullOrEmpty();
    }

    public record MinimalCollectionModel(string Id, string Name);

    [Fact]
    public async Task Get_one_respects_cancellation_token()
    {
        var listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        var collectionId = listResult.Value.Items.First().Id;

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(0); // Cancel immediately

        Func<Task> act = async () => await _pb.Collections
            .GetOneAsync<CollectionModel>(collectionId, cancellationToken: cts.Token);

        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task Get_one_returns_quickly_for_existing_collection()
    {
        var listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        var collectionId = listResult.Value.Items.First().Id;

        var stopwatch = Stopwatch.StartNew();
        var result = await _pb.Collections
            .GetOneAsync<CollectionModel>(collectionId);
        stopwatch.Stop();

        result.IsSuccess.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public async Task Get_one_can_be_called_concurrently()
    {
        var listResult = await _pb.Collections.GetListAsync<CollectionModel>();
        var collectionIds = listResult.Value.Items.Take(3).Select(c => c.Id).ToList();

        var tasks = collectionIds.Select(id =>
            _pb.Collections.GetOneAsync<CollectionModel>(id));

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        results.Select(r => r.Value.Id).Should().BeEquivalentTo(collectionIds);
    }

    [Fact]
    public async Task Get_one_system_collection()
    {
        var listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                Filter = "system = true",
                SkipTotal = true,
            });

        var systemCollection = listResult.Value.Items.First();
        var result = await _pb.Collections
            .GetOneAsync<CollectionModel>(systemCollection.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.System.Should().BeTrue();
    }

    [Fact]
    public async Task Get_one_base_collection()
    {
        var listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                Filter = "type = 'base'",
                SkipTotal = true,
            });

        var baseCollection = listResult.Value.Items.First();
        var result = await _pb.Collections
            .GetOneAsync<CollectionModel>(baseCollection.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("base");
    }

    [Fact]
    public async Task Get_one_view_collection()
    {
        var listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                Filter = "type = 'view'",
                SkipTotal = true,
            });

        if (listResult.Value.Items.Count != 0)
        {
            var viewCollection = listResult.Value.Items.First();
            var result = await _pb.Collections
                .GetOneAsync<CollectionModel>(viewCollection.Id);

            result.IsSuccess.Should().BeTrue();
            result.Value.Type.Should().Be("view");
        }
    }
}

