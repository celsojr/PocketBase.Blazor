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
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        listResult.IsSuccess.Should().BeTrue();
        string collectionId = listResult.Value.Items.First().Id;

        Result<CollectionModel> getResult = await _pb.Collections.GetOneAsync<CollectionModel>(collectionId);
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Id.Should().Be(collectionId);
    }

    [Fact]
    public async Task Get_one_with_invalid_id_returns_error()
    {
        Result<CollectionModel> result = await _pb.Collections
            .GetOneAsync<CollectionModel>("invalid-id-that-doesnt-exist");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Get_one_with_empty_id_returns_error_result()
    {
        Result<CollectionModel> result = await _pb.Collections
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
        Result<CollectionModel> result = await _pb.Collections
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
        Result<CollectionModel> result = await _pb.Collections
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
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        string collectionId = listResult.Value.Items.First().Id;

        Result<CollectionModel> result = await _pb.Collections.GetOneAsync<CollectionModel>(
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
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        CollectionModel collection = listResult.Value.Items.First();

        Result<CollectionModel> getResult = await _pb.Collections
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
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        string collectionId = listResult.Value.Items.First().Id;

        Result<MinimalCollectionModel> result = await _pb.Collections
            .GetOneAsync<MinimalCollectionModel>(collectionId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(collectionId);
        result.Value.Name.Should().NotBeNullOrEmpty();
    }

    public record MinimalCollectionModel(string Id, string Name);

    [Fact]
    public async Task Get_one_respects_cancellation_token()
    {
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        string collectionId = listResult.Value.Items.First().Id;

        using CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(0); // Cancel immediately

        Func<Task> act = async () => await _pb.Collections
            .GetOneAsync<CollectionModel>(collectionId, cancellationToken: cts.Token);

        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task Get_one_returns_quickly_for_existing_collection()
    {
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections.GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });

        string collectionId = listResult.Value.Items.First().Id;

        Stopwatch stopwatch = Stopwatch.StartNew();
        Result<CollectionModel> result = await _pb.Collections
            .GetOneAsync<CollectionModel>(collectionId);
        stopwatch.Stop();

        result.IsSuccess.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public async Task Get_one_can_be_called_concurrently()
    {
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections.GetListAsync<CollectionModel>();
        List<string> collectionIds = listResult.Value.Items.Take(3).Select(c => c.Id).ToList();

        IEnumerable<Task<Result<CollectionModel>>> tasks = collectionIds.Select(id =>
            _pb.Collections.GetOneAsync<CollectionModel>(id));

        Result<CollectionModel>[] results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        results.Select(r => r.Value.Id).Should().BeEquivalentTo(collectionIds);
    }

    [Fact]
    public async Task Get_one_system_collection()
    {
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                Filter = "system = true",
                SkipTotal = true,
            });

        CollectionModel systemCollection = listResult.Value.Items.First();
        Result<CollectionModel> result = await _pb.Collections
            .GetOneAsync<CollectionModel>(systemCollection.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.System.Should().BeTrue();
    }

    [Fact]
    public async Task Get_one_base_collection()
    {
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                Filter = "type = 'base'",
                SkipTotal = true,
            });

        CollectionModel baseCollection = listResult.Value.Items.First();
        Result<CollectionModel> result = await _pb.Collections
            .GetOneAsync<CollectionModel>(baseCollection.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("base");
    }

    [Fact]
    public async Task Get_one_view_collection()
    {
        Result<ListResult<CollectionModel>> listResult = await _pb.Collections
            .GetListAsync<CollectionModel>(
            perPage: 1,
            options: new ListOptions()
            { 
                Filter = "type = 'view'",
                SkipTotal = true,
            });

        if (listResult.Value.Items.Count != 0)
        {
            CollectionModel viewCollection = listResult.Value.Items.First();
            Result<CollectionModel> result = await _pb.Collections
                .GetOneAsync<CollectionModel>(viewCollection.Id);

            result.IsSuccess.Should().BeTrue();
            result.Value.Type.Should().Be("view");
        }
    }
}

