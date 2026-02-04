namespace PocketBase.Blazor.IntegrationTests.Clients.Batch;

using System.Text;
using Blazor.Models;
using FluentAssertions;
using Blazor.Requests.Batch;
using Xunit;

//[Trait("Category", "FileUpload")]
[Collection("PocketBase.Blazor.Admin")]
public class BatchFileUploadTests
{
    private readonly IPocketBase _pb;

    public BatchFileUploadTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task CreateBatch_WithSingleFileUpload_ShouldSucceed()
    {
        // Arrange
        var collectionName = $"batch_file_{Guid.NewGuid():N}";

        await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = collectionName,
            type = "base",
            fields = new object[]
            {
                new { name = "title", type = "text" },
                new { name = "document", type = "file", maxSelect = 1 }
            }
        });

        try
        {
            var batch = _pb.CreateBatch();

            var fileContent = "Hello from batch file upload";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var file = BatchFile.FromBytes(fileBytes, "document", "test.txt", "text/plain");

            batch.Collection(collectionName)
                .Create(new { title = "File Test" }, [file]);

            // Act
            var result = await batch.SendAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);

            var batchResponse = result.Value[0];
            batchResponse.Status.Should().Be(200);

            // Assert the record was created with document field populated
            var createdRecord = batchResponse.Body;
            createdRecord.Should().NotBeNull();
            createdRecord["document"]?.ToString().Should().NotBeNullOrEmpty();

            var recordId = createdRecord["id"]?.ToString();
            recordId.Should().NotBeNullOrEmpty();

            // Fetch the full record to verify document details
            var record = await _pb.Collection(collectionName)
                .GetOneAsync<Dictionary<string, object?>>(recordId);

            record.Value.Should().NotBeNull();
            record.Value["document"]?.ToString().Should().NotBeNullOrEmpty();

            // Download and verify the document content
            var documentFileName = record.Value["document"]?.ToString();
            if (!string.IsNullOrEmpty(documentFileName))
            {
                var downloadedBytes = await _pb.Files
                    .GetBytesAsync(collectionName, recordId, documentFileName);
                downloadedBytes.Should().NotBeNull();

                var downloadedContent = Encoding.UTF8.GetString(downloadedBytes.Value);
                downloadedContent.Should().Be(fileContent);
            }
        }
        finally
        {
            await _pb.Collections.DeleteAsync(collectionName);
        }
    }

    [Fact]
    public async Task CreateBatch_WithMixedFileAndNonFileRequests_ShouldSucceed()
    {
        // Arrange
        var collectionName = $"batch_mixed_{Guid.NewGuid():N}";
        var fileContent = "Batch file content";

        await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = collectionName,
            type = "base",
            fields = new[]
            {
                new { name = "name", type = "text" },
                new { name = "attachment", type = "file" }
            }
        });

        try
        {
            var batch = _pb.CreateBatch();

            var expectedRecords = new[]
            {
                new { Name = "No File", HasFile = false, FileContent = (string?)null },
                new { Name = "With File", HasFile = true, FileContent = fileContent }!
            };

            // First request - JSON only (no file)
            batch.Collection(collectionName)
                .Create(new { name = expectedRecords[0].Name });

            // Second request - with file
            var file = BatchFile.FromBytes(
                Encoding.UTF8.GetBytes(expectedRecords[1].FileContent!),
                "attachment",
                "file.txt",
                "text/plain"
            );

            batch.Collection(collectionName)
                .Create(new { name = expectedRecords[1].Name }, [file]);

            // Act
            var result = await batch.SendAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        
            // Verify each response
            for (var i = 0; i < result.Value.Count; i++)
            {
                var response = result.Value[i];
                var expected = expectedRecords[i];
            
                response.Status.Should().Be(200);
            
                var recordId = response.Body?["id"]?.ToString();
                recordId.Should().NotBeNullOrEmpty();
            
                var record = await _pb.Collection(collectionName)
                    .GetOneAsync<Dictionary<string, object?>>(recordId!);
            
                record.Value.Should().NotBeNull();
                record.Value!["name"]?.ToString().Should().Be(expected.Name);
            
                var attachment = record.Value["attachment"]?.ToString();
            
                if (expected.HasFile)
                {
                    attachment.Should().NotBeNullOrEmpty();
                
                    // Download and verify file
                    var downloadedBytes = await _pb.Files
                        .GetBytesAsync(collectionName, recordId, attachment);
                    downloadedBytes.Should().NotBeNull();
                
                    var downloadedContent = Encoding.UTF8.GetString(downloadedBytes.Value);
                    downloadedContent.Should().Be(expected.FileContent);
                }
                else
                {
                    attachment.Should().BeNullOrEmpty();
                }
            }
        }
        finally
        {
            await _pb.Collections.DeleteAsync(collectionName);
        }
    }

    [Fact]
    public async Task UpdateBatch_WithFileReplacement_ShouldSucceed()
    {
        var collectionName = $"batch_update_file_{Guid.NewGuid():N}";

        await _pb.Collections.CreateAsync<CollectionModel>(new
        {
            name = collectionName,
            type = "base",
            fields = new[]
            {
                new { name = "doc", type = "file" }
            }
        });

        try
        {
            // Create initial record
            var create = await _pb.Collection(collectionName)
                .CreateAsync<CollectionModel>(new { });

            var recordId = create.Value.Id.ToString();

            var batch = _pb.CreateBatch();

            var newFile = BatchFile.FromBytes(
                Encoding.UTF8.GetBytes("Updated file"),
                "doc",
                "updated.txt",
                "text/plain"
            );

            batch.Collection(collectionName)
                .Update(recordId, new { }, [newFile]);

            var result = await batch.SendAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value[0].Status.Should().Be(200);
        }
        finally
        {
            await _pb.Collections.DeleteAsync(collectionName);
        }
    }


}
