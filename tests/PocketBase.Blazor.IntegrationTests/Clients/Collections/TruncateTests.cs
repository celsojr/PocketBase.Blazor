namespace PocketBase.Blazor.IntegrationTests.Clients.Collections;

using System.Net;
using Blazor.Models;
using Blazor.Models.Collection;
using Blazor.Models.Collection.Fields;
using Blazor.Responses;

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
        var createResult = await _pb.Collections.CreateAsync(
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
        var collection = createResult.Value;

        collection.Should().NotBeNull();
        collection.Name.Should().NotBeNullOrWhiteSpace();

        // Add some records to the collection
        var recordsClient = _pb.Collection(collection.Name);
        for (int i = 0; i < 5; i++)
        {
            var recordResult = await recordsClient.CreateAsync<RecordModel>(new
            {
                title = $"Record {i}",
                count = i * 10
            });
            recordResult.IsSuccess.Should().BeTrue();
        }

        // Verify records exist before truncate
        var recordsBefore = await recordsClient.GetListAsync<RecordModel>();
        recordsBefore.Value.TotalItems.Should().Be(5);

        // Act - Truncate the collection
        var truncateResult = await _pb.Collections.TruncateAsync(collection.Id);

        // Assert
        truncateResult.IsSuccess.Should().BeTrue();

        // Verify all records are gone
        var recordsAfter = await recordsClient.GetListAsync<RecordModel>();
        recordsAfter.Value.TotalItems.Should().Be(0);

        // Verify collection still exists
        var getCollectionResult2 = await _pb.Collections.GetOneAsync<CollectionResponse>(collection.Id);
        getCollectionResult2.IsSuccess.Should().BeTrue();

        var collectionResponse = getCollectionResult2.Value;
        collectionResponse.Name.Should().Be("exampleToTruncate");
        collectionResponse.Type.Should().Be("base");
        collectionResponse.Fields.Should().NotBeEmpty();

        // Verify collection still exists (dictionary approach)
        var getCollectionResult = await _pb.Collections.GetOneAsync<Dictionary<string, object?>>(collection.Id);
        getCollectionResult.IsSuccess.Should().BeTrue();
        getCollectionResult.Value.ContainsKey("name").Should().BeTrue();
        getCollectionResult.Value.TryGetValue("name", out var nameValue).Should().BeTrue();
        nameValue.Should().NotBeNull();
        nameValue.Should().BeOfType<JsonElement>();
        nameValue.ToString().Should().Be("exampleToTruncate");
    }

    [Fact]
    public async Task Truncate_collection_with_no_records_should_succeed()
    {
        // Arrange
        var createResult = await _pb.Collections.CreateAsync(
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
        var truncateResult = await _pb.Collections.TruncateAsync(createResult.Value.Id);

        // Assert
        truncateResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Truncate_collection_should_fail_when_not_admin()
    {
        // Arrange - Create regular user client
        await using var client = new PocketBase(_fixture.Settings.BaseUrl);

        var authResult = await client.Collection("users")
            .AuthWithPasswordAsync(
                _fixture.Settings.UserTesterEmail,
                _fixture.Settings.UserTesterPassword
            );

        authResult.IsSuccess.Should().BeTrue();

        // Create collection as admin
        var createResult = await _pb.Collections.CreateAsync(
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
        var recordsClient = _pb.Collection(createResult.Value.Name);
        await recordsClient.CreateAsync<RecordModel>(new { title = "Test Record" });

        // Act - Try to truncate as regular user
        var truncateResult = await client.Collections.TruncateAsync(createResult.Value.Id);

        // Assert - Should fail
        truncateResult.IsSuccess.Should().BeFalse();
        truncateResult.Errors.Should().NotBeNull();

        var error = JsonSerializer.Deserialize<ErrorResponse>(truncateResult.Errors[0].Message);

        error.Should().NotBeNull();
        error.Message.Should().Be("The authorized record is not allowed to perform this action.");
        error.Status.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Truncate_nonexistent_collection_should_fail()
    {
        // Arrange
        var nonExistentId = "non_existent_id_12345";

        // Act
        var truncateResult = await _pb.Collections.TruncateAsync(nonExistentId);

        // Assert
        truncateResult.IsSuccess.Should().BeFalse();
        truncateResult.Errors.Should().NotBeNull();

        var error = JsonSerializer.Deserialize<ErrorResponse>(truncateResult.Errors[0].Message);
        error.Should().NotBeNull();
        error.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Truncate_system_collection_should_succeed()
    {
        // Create a system collection for testing
        var createResult = await _pb.Collections.CreateAsync(
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
        var collection = _pb.Collection(createResult.Value.Name);
        await collection.CreateAsync<RecordModel>(new { data = "test1" });
        await collection.CreateAsync<RecordModel>(new { data = "test2" });

        // Test truncate
        var truncateResult = await _pb.Collections.TruncateAsync(createResult.Value.Id);
        truncateResult.IsSuccess.Should().BeTrue();

        // Verify it's empty
        var records = await collection.GetListAsync<RecordResponse>();
        records.Value.TotalItems.Should().Be(0);

        // Cannot be deleted not even by admins. Only DBAs are able to delete system tables.
        var deletedResult = await _pb.Collections.DeleteAsync(createResult.Value.Id);
        deletedResult.IsSuccess.Should().BeFalse();
    }
}
