namespace PocketBase.Blazor.IntegrationTests.Clients.Batch;

using System.Text;
using Blazor.Clients.Batch;
using Blazor.Models;
using Blazor.Requests.Batch;
using Blazor.Responses.Backup;
using FluentAssertions;
using Xunit;

[Trait("Category", "Integration")]
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
        string collectionName = $"batch_file_{Guid.NewGuid():N}";

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
            IBatchClient batch = _pb.CreateBatch();

            string fileContent = "Hello from batch file upload";
            byte[] fileBytes = Encoding.UTF8.GetBytes(fileContent);
            BatchFile file = BatchFile.FromBytes(fileBytes, "document", "test.txt", "text/plain");

            batch.Collection(collectionName)
                .Create(new { title = "File Test" }, [file]);

            // Act
            Result<List<BatchResponse>> result = await batch.SendAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);

            BatchResponse batchResponse = result.Value[0];
            batchResponse.Status.Should().Be(200);

            // Assert the record was created with document field populated
            Dictionary<string, object>? createdRecord = batchResponse.Body;
            createdRecord.Should().NotBeNull();
            createdRecord["document"]?.ToString().Should().NotBeNullOrEmpty();

            string? recordId = createdRecord["id"]?.ToString();
            recordId.Should().NotBeNullOrEmpty();

            // Fetch the full record to verify document details
            Result<Dictionary<string, object?>> record = await _pb.Collection(collectionName)
                .GetOneAsync<Dictionary<string, object?>>(recordId);

            record.Value.Should().NotBeNull();
            record.Value["document"]?.ToString().Should().NotBeNullOrEmpty();

            // Download and verify the document content
            string? documentFileName = record.Value["document"]?.ToString();
            if (!string.IsNullOrEmpty(documentFileName))
            {
                Result<byte[]> downloadedBytes = await _pb.Files
                    .GetBytesAsync(collectionName, recordId, documentFileName);
                downloadedBytes.Should().NotBeNull();

                string downloadedContent = Encoding.UTF8.GetString(downloadedBytes.Value);
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
        Result smtpResult = await _pb.Settings.UpdateAsync(new
        {
            batch = new
            {
                enabled = true
            }
        });
        smtpResult.IsSuccess.Should().BeTrue();

        string collectionName = $"batch_mixed_{Guid.NewGuid():N}";
        string fileContent = "Batch file content";

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
            IBatchClient batch = _pb.CreateBatch();

            var expectedRecords = new[]
            {
                new { Name = "No File", HasFile = false, FileContent = (string?)null },
                new { Name = "With File", HasFile = true, FileContent = fileContent }!
            };

            // First request - JSON only (no file)
            batch.Collection(collectionName)
                .Create(new { name = expectedRecords[0].Name });

            // Second request - with file
            BatchFile file = BatchFile.FromBytes(
                Encoding.UTF8.GetBytes(expectedRecords[1].FileContent!),
                "attachment",
                "file.txt",
                "text/plain"
            );

            batch.Collection(collectionName)
                .Create(new { name = expectedRecords[1].Name }, [file]);

            // Act
            Result<List<BatchResponse>> result = await batch.SendAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);

            // Verify each response
            for (int i = 0; i < result.Value.Count; i++)
            {
                BatchResponse response = result.Value[i];
                var expected = expectedRecords[i];

                response.Status.Should().Be(200);

                string? recordId = response.Body?["id"]?.ToString();
                recordId.Should().NotBeNullOrEmpty();

                Result<Dictionary<string, object?>> record = await _pb.Collection(collectionName)
                    .GetOneAsync<Dictionary<string, object?>>(recordId!);

                record.Value.Should().NotBeNull();
                record.Value!["name"]?.ToString().Should().Be(expected.Name);

                string? attachment = record.Value["attachment"]?.ToString();

                if (expected.HasFile)
                {
                    attachment.Should().NotBeNullOrEmpty();

                    // Download and verify file
                    Result<byte[]> downloadedBytes = await _pb.Files
                        .GetBytesAsync(collectionName, recordId, attachment);
                    downloadedBytes.Should().NotBeNull();

                    string downloadedContent = Encoding.UTF8.GetString(downloadedBytes.Value);
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
        Result smtpResult = await _pb.Settings.UpdateAsync(new
        {
            batch = new
            {
                enabled = true
            }
        });
        smtpResult.IsSuccess.Should().BeTrue();

        string collectionName = $"batch_update_file_{Guid.NewGuid():N}";
        string initialFileContent = "Initial file";
        string updatedFileContent = "Updated file";

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
            IBatchClient createBatch = _pb.CreateBatch();
            BatchFile initialFile = BatchFile.FromBytes(
                Encoding.UTF8.GetBytes(initialFileContent),
                "doc",
                "initial.txt",
                "text/plain"
            );

            createBatch.Collection(collectionName)
                .Create(new { }, [initialFile]);

            Result<List<BatchResponse>> createResult = await createBatch.SendAsync();
            createResult.IsSuccess.Should().BeTrue();
            createResult.Value.Should().HaveCount(1);
            createResult.Value[0].Status.Should().Be(200);

            string? recordId = createResult.Value[0].Body?["id"]?.ToString();
            recordId.Should().NotBeNullOrEmpty();

            // Verify initial file via regular GET (not file download)
            Result<Dictionary<string, object?>> initialRecord = await _pb.Collection(collectionName)
                .GetOneAsync<Dictionary<string, object?>>(recordId!);

            initialRecord.Value.Should().NotBeNull();
            string? initialFileName = initialRecord.Value["doc"]?.ToString();
            initialFileName.Should().NotBeNullOrEmpty();

            // Step 2: Update with new file using another batch
            IBatchClient updateBatch = _pb.CreateBatch();
            BatchFile newFile = BatchFile.FromBytes(
                Encoding.UTF8.GetBytes(updatedFileContent),
                "doc",
                "updated.txt",
                "text/plain"
            );

            updateBatch.Collection(collectionName)
                .Update(recordId, null!, [newFile]);

            Result<List<BatchResponse>> result = await updateBatch.SendAsync();

            // Assert update batch response
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value[0].Status.Should().Be(200);

            // Verify the record after update
            Result<Dictionary<string, object?>> updatedRecord = await _pb.Collection(collectionName)
                .GetOneAsync<Dictionary<string, object?>>(recordId);

            updatedRecord.Value.Should().NotBeNull();

            // Check that the file field is populated with new filename
            string? updatedFileName = updatedRecord.Value["doc"]?.ToString();
            updatedFileName.Should().NotBeNullOrEmpty();

            // File name should be different from initial (updated)
            updatedFileName.Should().NotBe(initialFileName);

            // Download and verify the updated file content
            Result<byte[]> updatedDownloadedBytes = await _pb.Files
                .GetBytesAsync(collectionName, recordId, updatedFileName);
            updatedDownloadedBytes.Should().NotBeNull();

            string updatedDownloadedContent = Encoding.UTF8.GetString(updatedDownloadedBytes!.Value);
            updatedDownloadedContent.Should().Be(updatedFileContent);
        }
        finally
        {
            await _pb.Collections.DeleteAsync(collectionName);
        }
    }
}
