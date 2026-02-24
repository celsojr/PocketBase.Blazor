namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

using System.Net;
using Blazor.Clients.Record;
using Blazor.Models;
using Blazor.Models.Collection;
using Blazor.Models.Collection.Fields;
using Blazor.Responses;
using Blazor.Responses.Auth;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class TruncateTests
{
    private readonly IPocketBase _pb;
    private readonly PocketBaseAdminFixture _fixture;

    public TruncateTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
        _fixture = fixture;
    }

    [Fact]
    public async Task Truncate_collection_successfully()
    {
        // Arrange - Create collection
        Result<CollectionModel> createResult = await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = "exampleToTruncate",
                Fields = new List<FieldModel>
                {
                    new TextFieldModel
                    {
                        Name = "title",
                        Required = true
                    },
                    new NumberFieldModel
                    {
                        Name = "count"
                    }
                }
            });

        createResult.IsSuccess.Should().BeTrue();
        CollectionModel collection = createResult.Value;

        collection.Should().NotBeNull();
        collection.Name.Should().NotBeNullOrWhiteSpace();

        // Add some records to the collection
        IRecordClient recordsClient = _pb.Collection(collection.Name);
        for (int i = 0; i < 5; i++)
        {
            Result<RecordModel> recordResult = await recordsClient.CreateAsync<RecordModel>(new
            {
                title = $"Record {i}",
                count = i * 10
            });
            recordResult.IsSuccess.Should().BeTrue();
        }

        // Verify records exist before truncate
        Result<ListResult<RecordModel>> recordsBefore = await recordsClient.GetListAsync<RecordModel>();
        recordsBefore.Value.TotalItems.Should().Be(5);

        // Act - Truncate the collection
        Result truncateResult = await _pb.Collections.TruncateAsync(collection.Id);

        // Assert
        truncateResult.IsSuccess.Should().BeTrue();

        // Verify all records are gone
        Result<ListResult<RecordModel>> recordsAfter = await recordsClient.GetListAsync<RecordModel>();
        recordsAfter.Value.TotalItems.Should().Be(0);

        // Verify collection still exists
        Result<CollectionResponse> getCollectionResult2 = await _pb.Collections.GetOneAsync<CollectionResponse>(collection.Id);
        getCollectionResult2.IsSuccess.Should().BeTrue();

        CollectionResponse collectionResponse = getCollectionResult2.Value;
        collectionResponse.Name.Should().Be("exampleToTruncate");
        collectionResponse.Type.Should().Be("base");
        collectionResponse.Fields.Should().NotBeEmpty();

        // Verify collection still exists (dictionary approach)
        Result<Dictionary<string, object?>> getCollectionResult = await _pb.Collections.GetOneAsync<Dictionary<string, object?>>(collection.Id);
        getCollectionResult.IsSuccess.Should().BeTrue();
        getCollectionResult.Value.ContainsKey("name").Should().BeTrue();
        getCollectionResult.Value.TryGetValue("name", out object? nameValue).Should().BeTrue();
        nameValue.Should().NotBeNull();
        nameValue.Should().BeOfType<JsonElement>();
        nameValue.ToString().Should().Be("exampleToTruncate");
    }

    [Fact]
    public async Task Truncate_collection_with_no_records_should_succeed()
    {
        // Arrange
        Result<CollectionModel> createResult = await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = "emptyToTruncate",
                Fields = new List<FieldModel>
                {
                    new TextFieldModel { Name = "name" }
                }
            });

        createResult.IsSuccess.Should().BeTrue();

        // Act
        Result truncateResult = await _pb.Collections.TruncateAsync(createResult.Value.Id);

        // Assert
        truncateResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Truncate_collection_should_fail_when_not_admin()
    {
        // Arrange - Create regular user client
        await using PocketBase client = new PocketBase(_fixture.Settings.BaseUrl);

        Result<AuthResponse> authResult = await client.Collection("users")
            .AuthWithPasswordAsync(
                _fixture.Settings.UserTesterEmail,
                _fixture.Settings.UserTesterPassword
            );

        authResult.IsSuccess.Should().BeTrue();

        // Create collection as admin
        Result<CollectionModel> createResult = await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = "exampleToTruncateUnauthorized",
                Fields = new List<FieldModel>
                {
                    new TextFieldModel
                    {
                        Name = "title",
                        Required = true
                    }
                }
            });

        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.Should().NotBeNull();
        createResult.Value.Name.Should().NotBeNullOrWhiteSpace();

        // Add some records
        IRecordClient recordsClient = _pb.Collection(createResult.Value.Name);
        await recordsClient.CreateAsync<RecordModel>(new { title = "Test Record" });

        // Act - Try to truncate as regular user
        Result truncateResult = await client.Collections.TruncateAsync(createResult.Value.Id);

        // Assert - Should fail
        truncateResult.IsSuccess.Should().BeFalse();
        truncateResult.Errors.Should().NotBeNull();

        ErrorResponse? error = JsonSerializer.Deserialize<ErrorResponse>(truncateResult.Errors[0].Message);

        error.Should().NotBeNull();
        error.Message.Should().Be("The authorized record is not allowed to perform this action.");
        error.Status.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Truncate_nonexistent_collection_should_fail()
    {
        // Arrange
        string nonExistentId = "non_existent_id_12345";

        // Act
        Result truncateResult = await _pb.Collections.TruncateAsync(nonExistentId);

        // Assert
        truncateResult.IsSuccess.Should().BeFalse();
        truncateResult.Errors.Should().NotBeNull();

        ErrorResponse? error = JsonSerializer.Deserialize<ErrorResponse>(truncateResult.Errors[0].Message);
        error.Should().NotBeNull();
        error.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Truncate_system_collection_should_succeed()
    {
        // Create a system collection for testing
        Result<CollectionModel> createResult = await _pb.Collections.CreateAsync(
            new BaseCollectionCreateModel
            {
                Name = "_test_truncate_safe_",

                // Make sure this is a system table,
                // placed under System fold in the web UI
                System = true,

                Fields = new List<FieldModel>
                {
                    new TextFieldModel { Name = "data" }
                }
            });

        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.Should().NotBeNull();
        createResult.Value.Name.Should().NotBeNullOrWhiteSpace();

        // Add some test data
        IRecordClient collection = _pb.Collection(createResult.Value.Name);
        await collection.CreateAsync<RecordModel>(new { data = "test1" });
        await collection.CreateAsync<RecordModel>(new { data = "test2" });

        // Test truncate
        Result truncateResult = await _pb.Collections.TruncateAsync(createResult.Value.Id);
        truncateResult.IsSuccess.Should().BeTrue();

        // Verify it's empty
        Result<ListResult<RecordResponse>> records = await collection.GetListAsync<RecordResponse>();
        records.Value.TotalItems.Should().Be(0);

        // Cannot be deleted not even by admins. Only DBAs are able to delete system tables.
        Result deletedResult = await _pb.Collections.DeleteAsync(createResult.Value.Id);
        deletedResult.IsSuccess.Should().BeFalse();
    }
}
