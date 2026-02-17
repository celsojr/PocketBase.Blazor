namespace PocketBase.Blazor.IntegrationTests.Clients.Batch;

using System.Threading;
using System.Threading.Tasks;
using Blazor.Models;
using Xunit;

[Collection("PocketBase.Blazor.Admin")]
public class CreateTests
{
    private readonly IPocketBase _pb;

    public CreateTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task CreateBatch_WhenBatchDisabled_ShouldThrowException()
    {
        // Arrange
        var smtpResult = await _pb.Settings.UpdateAsync(new
        {
            batch = new
            {
                enabled = false
            }
        });
        smtpResult.IsSuccess.Should().BeTrue();

        var batch = _pb.CreateBatch();

        // Dummy collection
        batch.Collection("any")
            .Create(new { test = "value" });

        // Act
        var result = await batch.SendAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        // Should contain batch not allowed error
        result.Errors.Should().Contain(e =>
            e.Message.Contains("Batch requests are not allowed", StringComparison.OrdinalIgnoreCase) ||
            e.Message.Contains("\"status\":403", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateBatch_WithSingleCreate_ShouldSucceed()
    {
        // Arrange - Enable batch requests
        var batchSettings = new
        {
            batch = new
            {
                enabled = true,
                maxRequests = 50,
                timeout = 3,
                maxBodySize = 0
            }
        };

        var enableResult = await _pb.Settings.UpdateAsync(batchSettings);
        enableResult.IsSuccess.Should().BeTrue();

        // Arrange - Create a test collection
        var collectionName = $"batch_test_{Guid.NewGuid():N}";

        // Create a test collection
        await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = collectionName,
            type = "base",
            fields = new[]
            {
                new { name = "title", type = "text" },
                new { name = "content", type = "text" }
            }
        });

        try
        {
            var batch = _pb.CreateBatch();

            batch.Collection(collectionName)
                .Create(new
                {
                    title = "Batch Test",
                    content = "Created via batch"
                });

            // Act
            var result = await batch.SendAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value[0].Status.Should().Be(200);
        }
        finally
        {
            // Cleanup
            await _pb.Collections.DeleteAsync(collectionName);

            // Disable batch requests (optional - restore original state)
            //var disableSettings = new { batch = new { enabled = false } };
            //await _pb.Settings.UpdateAsync(disableSettings);
        }
    }

    [Fact]
    public async Task CreateBatch_WithMultipleOperations_ShouldSucceed()
    {
        // Arrange
        var collectionName = $"batch_multi_{Guid.NewGuid():N}";

        await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = collectionName,
            type = "base",
            fields = new[] { new { name = "name", type = "text" } }
        });

        try
        {
            var batch = _pb.CreateBatch();

            // Create two records
            batch.Collection(collectionName).Create(new { name = "First" });
            batch.Collection(collectionName).Create(new { name = "Second" });

            // Act
            var result = await batch.SendAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value.Should().AllSatisfy(r => r.Status.Should().Be(200));
        }
        finally
        {
            await _pb.Collections.DeleteAsync(collectionName);
        }
    }

    [Fact]
    public async Task CreateBatch_WithUpdateAndDelete_ShouldSucceed()
    {
        // Arrange
        var collectionName = $"batch_crud_{Guid.NewGuid():N}";

        await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = collectionName,
            type = "base",
            fields = new[] { new { name = "value", type = "number" } }
        });

        try
        {
            // First create a record to update/delete
            var createResult = await _pb.Collection(collectionName)
                .CreateAsync<CollectionModel>(new { value = 100 });

            var recordId = createResult.Value.Id.ToString();

            var batch = _pb.CreateBatch();

            // Update the record
            batch.Collection(collectionName).Update(recordId, new { value = 200 });

            // Delete the record
            batch.Collection(collectionName).Delete(recordId);

            // Act
            var result = await batch.SendAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value[0].Status.Should().Be(200);
            result.Value[1].Status.Should().Be(204);
        }
        finally
        {
            await _pb.Collections.DeleteAsync(collectionName);
        }
    }

    [Fact]
    public async Task CreateBatch_WithUpsert_ShouldSucceed()
    {
        // Arrange
        var smtpResult = await _pb.Settings.UpdateAsync(new
        {
            batch = new
            {
                enabled = true
            }
        });
        smtpResult.IsSuccess.Should().BeTrue();

        var collectionName = $"batch_upsert_{Guid.NewGuid():N}";

        await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = collectionName,
            type = "base",
            fields = new[] { new { name = "email", type = "email" } }
        });

        try
        {
            var batch = _pb.CreateBatch();

            batch.Collection(collectionName)
                .Upsert(new { email = "test@example.com" });

            // Act
            var result = await batch.SendAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value[0].Status.Should().Be(200);
        }
        finally
        {
            await _pb.Collections.DeleteAsync(collectionName);
        }
    }

    [Fact]
    public async Task CreateBatch_WithInvalidCollection_ShouldFail()
    {
        // Arrange
        var batch = _pb.CreateBatch();
        batch.Collection("non_existent_collection").Create(new { test = "value" });

        // Act
        var result = await batch.SendAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateBatch_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var collectionName = $"batch_cancel_{Guid.NewGuid():N}";

        await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = collectionName,
            type = "base",
            fields = new[] { new { name = "data", type = "text" } }
        });

        try
        {
            var batch = _pb.CreateBatch();

            batch.Collection(collectionName)
                .Create(new { data = "test" });

            var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act
            var act = async () => await batch.SendAsync(cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
        finally
        {
            await _pb.Collections.DeleteAsync(collectionName);
        }
    }

    [Fact]
    public async Task CreateBatch_WhenUnauthenticated_ShouldFail()
    {
        // Arrange
        await using var pb = new PocketBase(_pb.BaseUrl);
        var batch = pb.CreateBatch();
        batch.Collection("any").Create(new { test = "value" });

        // Act
        var result = await batch.SendAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateBatch_WithMixedCollections_ShouldSucceed()
    {
        // Arrange - Create two collections
        var collection1 = $"batch_mix1_{Guid.NewGuid():N}";
        var collection2 = $"batch_mix2_{Guid.NewGuid():N}";

        await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = collection1,
            type = "base",
            fields = new[] { new { name = "field1", type = "text" } }
        });

        await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = collection2,
            type = "base",
            fields = new[] { new { name = "field2", type = "number" } }
        });

        try
        {
            var batch = _pb.CreateBatch();

            // Add operations to different collections
            batch.Collection(collection1).Create(new { field1 = "Text value" });
            batch.Collection(collection2).Create(new { field2 = 42 });

            // Act
            var result = await batch.SendAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }
        finally
        {
            await _pb.Collections.DeleteAsync(collection1);
            await _pb.Collections.DeleteAsync(collection2);
        }
    }
}
