namespace PocketBase.Blazor.IntegrationTests.Clients.Batch;

using System.Text;
using Blazor.Models;
using FluentAssertions;
using Blazor.Requests.Batch;
using Xunit;

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
        // Arrange - Configure batch
        var smtpResult = await _pb.Settings.UpdateAsync(new
        {
            batch = new
            {
                enabled = true
            }
        });
        smtpResult.IsSuccess.Should().BeTrue();

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
        // Arrange
        var smtpResult = await _pb.Settings.UpdateAsync(new
        {
            batch = new
            {
                enabled = true
            }
        });
        smtpResult.IsSuccess.Should().BeTrue();

        var collectionName = $"batch_update_file_{Guid.NewGuid():N}";
        var initialFileContent = "Initial file";
        var updatedFileContent = "Updated file";

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
            // Step 1: Create initial record WITH file using batch
            var createBatch = _pb.CreateBatch();
            var initialFile = BatchFile.FromBytes(
                Encoding.UTF8.GetBytes(initialFileContent),
                "doc",
                "initial.txt",
                "text/plain"
            );
        
            createBatch.Collection(collectionName)
                .Create(new { }, [initialFile]);
        
            var createResult = await createBatch.SendAsync();
            createResult.IsSuccess.Should().BeTrue();
            createResult.Value.Should().HaveCount(1);
            createResult.Value[0].Status.Should().Be(200);

            var recordId = createResult.Value[0].Body?["id"]?.ToString();
            recordId.Should().NotBeNullOrEmpty();

            // Verify initial file via regular GET (not file download)
            var initialRecord = await _pb.Collection(collectionName)
                .GetOneAsync<Dictionary<string, object?>>(recordId!);
        
            initialRecord.Value.Should().NotBeNull();
            var initialFileName = initialRecord.Value["doc"]?.ToString();
            initialFileName.Should().NotBeNullOrEmpty();

            // Step 2: Update with new file using another batch
            var updateBatch = _pb.CreateBatch();
            var newFile = BatchFile.FromBytes(
                Encoding.UTF8.GetBytes(updatedFileContent),
                "doc",
                "updated.txt",
                "text/plain"
            );

            updateBatch.Collection(collectionName)
                .Update(recordId, null!, [newFile]);

            var result = await updateBatch.SendAsync();

            // Assert update batch response
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value[0].Status.Should().Be(200);

            // Verify the record after update
            var updatedRecord = await _pb.Collection(collectionName)
                .GetOneAsync<Dictionary<string, object?>>(recordId);
        
            updatedRecord.Value.Should().NotBeNull();
        
            // Check that the file field is populated with new filename
            var updatedFileName = updatedRecord.Value["doc"]?.ToString();
            updatedFileName.Should().NotBeNullOrEmpty();
        
            // File name should be different from initial (updated)
            updatedFileName.Should().NotBe(initialFileName);
        
            // Download and verify the updated file content
            var updatedDownloadedBytes = await _pb.Files
                .GetBytesAsync(collectionName, recordId, updatedFileName);
            updatedDownloadedBytes.Should().NotBeNull();
        
            var updatedDownloadedContent = Encoding.UTF8.GetString(updatedDownloadedBytes!.Value);
            updatedDownloadedContent.Should().Be(updatedFileContent);
        }
        finally
        {
            await _pb.Collections.DeleteAsync(collectionName);
        }
    }
}
