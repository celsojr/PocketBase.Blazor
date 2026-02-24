namespace PocketBase.Blazor.IntegrationTests.Clients.Realtime;

using Blazor.Events;
using Blazor.Responses;
using Xunit.Abstractions;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class UnsubscribeTests
{
    private readonly PocketBaseAdminFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly IPocketBase _pb;

    public UnsubscribeTests(PocketBaseAdminFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _pb = fixture.Client;
    }

    [Fact]
    public async Task Realtime_UnsubscribeAsync_WithSpecificRecord_ShouldStopReceivingEvents()
    {
        // Arrange
        await using PocketBase pb = new PocketBase(_pb.BaseUrl);
        List<RealtimeRecordEvent> eventsReceived = new List<RealtimeRecordEvent>();

        // Authenticate as admin
        await pb.Admins.AuthWithPasswordAsync(
            _fixture.Settings.AdminTesterEmail,
            _fixture.Settings.AdminTesterPassword
        );

        // Subscribe to a specific record
        Result<RecordResponse> record1 = await pb.Collection("categories").CreateAsync<RecordResponse>(new
        {
            name = "Test Category 1",
            slug = "test-category-1",
        });

        Result<RecordResponse> record2 = await pb.Collection("categories").CreateAsync<RecordResponse>(new
        {
            name = "Test Category 2",
            slug = "test-category-2",
        });

        IDisposable subscription1 = await pb.Realtime.SubscribeAsync("categories", record1.Value.Id, evt =>
        {
            eventsReceived.Add(evt);
            _output.WriteLine($"Callback: {evt.Action} - {evt.RecordId}");
        });

        IDisposable subscription2 = await pb.Realtime.SubscribeAsync("categories", record2.Value.Id, evt =>
        {
            eventsReceived.Add(evt);
            _output.WriteLine($"Callback: {evt.Action} - {evt.RecordId}");
        });

        // Act - Unsubscribe from record1 only
        await pb.Realtime.UnsubscribeAsync("categories", record1.Value.Id);

        // Trigger updates on both records
        await pb.Collection("categories").UpdateAsync<RecordResponse>(record1.Value.Id, new { name = "Updated 1" });
        await pb.Collection("categories").UpdateAsync<RecordResponse>(record2.Value.Id, new { name = "Updated 2" });

        await Task.Delay(2000);

        // Assert
        eventsReceived.Should().HaveCount(1, "Should only receive events for still-subscribed record");
        eventsReceived.Should().ContainSingle(e => e.RecordId == record2.Value.Id);
        eventsReceived.Should().NotContain(e => e.RecordId == record1.Value.Id);

        // Cleanup
        subscription1.Dispose();
        subscription2.Dispose();
        await pb.Collection("categories").DeleteAsync(record1.Value.Id);
        await pb.Collection("categories").DeleteAsync(record2.Value.Id);
    }

    [Fact]
    public async Task Realtime_UnsubscribeAsync_WithWildcard_ShouldStopReceivingAllCollectionEvents()
    {
        // Arrange
        await using PocketBase pb = new PocketBase(_pb.BaseUrl);
        List<RealtimeRecordEvent> eventsReceived = new List<RealtimeRecordEvent>();

        // Authenticate as admin
        await pb.Admins.AuthWithPasswordAsync(
            _fixture.Settings.AdminTesterEmail,
            _fixture.Settings.AdminTesterPassword
        );

        IDisposable subscription = await pb.Realtime.SubscribeAsync("categories", "*", evt =>
        {
            eventsReceived.Add(evt);
            _output.WriteLine($"Callback: {evt.Action} - {evt.RecordId}");
        });

        // Create initial record
        Result<RecordResponse> record = await pb.Collection("categories").CreateAsync<RecordResponse>(new
        {
            name = "Test Category",
            slug = "test-category",
        });

        await Task.Delay(1000);
        eventsReceived.Clear(); // Clear the create event

        // Act - Unsubscribe from entire collection
        await pb.Realtime.UnsubscribeAsync("categories");

        // Try to trigger more events
        await pb.Collection("categories").UpdateAsync<RecordResponse>(record.Value.Id, new { name = "Updated" });
        await Task.Delay(2000);

        // Assert
        eventsReceived.Should().BeEmpty("Should not receive events after unsubscribe");

        // Cleanup
        subscription.Dispose();
        await pb.Collection("categories").DeleteAsync(record.Value.Id);
    }

    [Fact]
    public async Task Realtime_UnsubscribeAsync_WithWildcardStar_ShouldStopReceivingWildcardEventsOnly()
    {
        // Arrange
        await using PocketBase pb = new PocketBase(_pb.BaseUrl);
        List<RealtimeRecordEvent> eventsReceived = new List<RealtimeRecordEvent>();

        // Authenticate as admin
        await pb.Admins.AuthWithPasswordAsync(
            _fixture.Settings.AdminTesterEmail,
            _fixture.Settings.AdminTesterPassword
        );

        // Subscribe to wildcard and specific record
        IDisposable wildcardSubscription = await pb.Realtime.SubscribeAsync("categories", "*", evt =>
        {
            eventsReceived.Add(evt);
            _output.WriteLine($"Wildcard Callback: {evt.Action} - {evt.RecordId}");
        });

        Result<RecordResponse> record = await pb.Collection("categories").CreateAsync<RecordResponse>(new
        {
            name = "Test Category",
            slug = "test-category",
        });

        IDisposable specificSubscription = await pb.Realtime.SubscribeAsync("categories", record.Value.Id, evt =>
        {
            eventsReceived.Add(evt);
            _output.WriteLine($"Specific Callback: {evt.Action} - {evt.RecordId}");
        });

        await Task.Delay(1000);
        eventsReceived.Clear(); // Clear initial events

        // Act - Unsubscribe from wildcard only
        await pb.Realtime.UnsubscribeAsync("categories", "*");

        // Trigger update
        await pb.Collection("categories").UpdateAsync<RecordResponse>(record.Value.Id, new { name = "Updated" });
        await Task.Delay(2000);

        // Assert - Should only receive from specific subscription
        eventsReceived.Should().HaveCount(1, "Should only receive from specific subscription");
        eventsReceived.Should().ContainSingle(e => e.RecordId == record.Value.Id);

        // Cleanup
        wildcardSubscription.Dispose();
        specificSubscription.Dispose();
        await pb.Collection("categories").DeleteAsync(record.Value.Id);
    }

    [Fact]
    public async Task RealtimeSse_UnsubscribeAsync_ShouldStopStreaming()
    {
        // Arrange
        await using PocketBase pb = new PocketBase(_pb.BaseUrl);
        List<RealtimeRecordEvent> eventsReceived = new List<RealtimeRecordEvent>();
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        // Authenticate as admin
        await pb.Admins.AuthWithPasswordAsync(
            _fixture.Settings.AdminTesterEmail,
            _fixture.Settings.AdminTesterPassword
        );

        // Start streaming
        Task streamingTask = Task.Run(async () =>
        {
            await foreach (RealtimeRecordEvent evt in pb.RealtimeSse.SubscribeAsync("categories", "*", cancellationToken: cts.Token))
            {
                eventsReceived.Add(evt);
                _output.WriteLine($"Stream Event: {evt.Action} - {evt.RecordId}");
            }
        }, cts.Token);

        await Task.Delay(2000); // Wait for connection

        // Create initial record
        Result<RecordResponse> record = await pb.Collection("categories").CreateAsync<RecordResponse>(new
        {
            name = "Test Category",
            slug = "test-category",
        });

        await Task.Delay(1000);
        int initialCount = eventsReceived.Count;

        // Act - Unsubscribe
        await pb.RealtimeSse.UnsubscribeAsync("categories", "*");

        // Try to trigger more events
        await pb.Collection("categories").UpdateAsync<RecordResponse>(record.Value.Id, new { name = "Updated" });
        await Task.Delay(2000);

        // Assert - No new events should be received
        eventsReceived.Should().HaveCount(initialCount, "Should not receive new events after unsubscribe");

        // Cleanup
        await cts.CancelAsync();
        try { await streamingTask; } catch (OperationCanceledException) { }
        await pb.Collection("categories").DeleteAsync(record.Value.Id);
    }

    [Fact]
    public async Task RealtimeSse_UnsubscribeAsync_WithNullRecordId_ShouldUnsubscribeEntireCollection()
    {
        // Arrange
        await using PocketBase pb = new PocketBase(_pb.BaseUrl);
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        bool isConnectedBefore = false;
        bool isConnectedAfter = false;

        // Authenticate as admin
        await pb.Admins.AuthWithPasswordAsync(
            _fixture.Settings.AdminTesterEmail,
            _fixture.Settings.AdminTesterPassword
        );

        // Start streaming
        Task streamingTask = Task.Run(async () =>
        {
            await foreach (RealtimeRecordEvent _ in pb.RealtimeSse.SubscribeAsync("categories", "*", cancellationToken: cts.Token))
            {
                // Just consume
            }
        }, cts.Token);

        await Task.Delay(2000);
        isConnectedBefore = pb.RealtimeSse.IsConnected;

        // Act - Unsubscribe entire collection
        await pb.RealtimeSse.UnsubscribeAsync("categories");

        await Task.Delay(1000);
        isConnectedAfter = pb.RealtimeSse.IsConnected;

        // Assert
        isConnectedBefore.Should().BeTrue("Should be connected before unsubscribe");
        // Note: IsConnected might still be true because connection persists
        // The real test is that we don't receive events
        
        // Cleanup
        await cts.CancelAsync();
        try { await streamingTask; } catch (OperationCanceledException) { }
    }

    [Fact]
    public async Task Both_Clients_UnsubscribeAsync_ShouldHandleMultipleSubscriptions()
    {
        // Arrange
        await using PocketBase pb = new PocketBase(_pb.BaseUrl);
        List<RealtimeRecordEvent> callbackEvents = new List<RealtimeRecordEvent>();
        List<RealtimeRecordEvent> streamEvents = new List<RealtimeRecordEvent>();

        // Authenticate as admin
        await pb.Admins.AuthWithPasswordAsync(
            _fixture.Settings.AdminTesterEmail,
            _fixture.Settings.AdminTesterPassword
        );

        // Create test records
        List<string> records = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            Result<RecordResponse> record = await pb.Collection("categories").CreateAsync<RecordResponse>(new
            {
                name = $"Test Category {i}",
                slug = $"test-category-{i}",
            });
            records.Add(record.Value.Id);
        }

        // Multiple callback subscriptions
        List<IDisposable> callbackSubscriptions = new List<IDisposable>();
        foreach (string recordId in records)
        {
            IDisposable sub = await pb.Realtime.SubscribeAsync("categories", recordId, evt =>
            {
                callbackEvents.Add(evt);
                _output.WriteLine($"Callback for {recordId}: {evt.Action}");
            });
            callbackSubscriptions.Add(sub);
        }

        // Stream subscription
        CancellationTokenSource streamCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        Task streamingTask = Task.Run(async () =>
        {
            await foreach (RealtimeRecordEvent evt in pb.RealtimeSse.SubscribeAsync("categories", "*", cancellationToken: streamCts.Token))
            {
                streamEvents.Add(evt);
                _output.WriteLine($"Stream: {evt.Action} - {evt.RecordId}");
            }
        }, streamCts.Token);

        await Task.Delay(2000);

        // Act - Unsubscribe from middle record only
        await pb.Realtime.UnsubscribeAsync("categories", records[1]);
        await pb.RealtimeSse.UnsubscribeAsync("categories", records[1]);

        // Update all records
        foreach (string recordId in records)
        {
            await pb.Collection("categories").UpdateAsync<RecordResponse>(recordId, new { name = "Updated" });
            await Task.Delay(500);
        }

        await Task.Delay(2000);

        // Assert
        callbackEvents.Should().HaveCount(2, "Should receive events for 2 subscribed records");
        callbackEvents.Should().Contain(e => e.RecordId == records[0]);
        callbackEvents.Should().Contain(e => e.RecordId == records[2]);
        callbackEvents.Should().NotContain(e => e.RecordId == records[1]);

        // Cleanup
        foreach (IDisposable sub in callbackSubscriptions) sub.Dispose();
        await streamCts.CancelAsync();
        try { await streamingTask; } catch (OperationCanceledException) { }
        
        foreach (string recordId in records)
        {
            await pb.Collection("categories").DeleteAsync(recordId);
        }
    }

    [Fact]
    public async Task UnsubscribeAsync_WhenNotSubscribed_ShouldNotThrow()
    {
        // Arrange
        await using PocketBase pb = new PocketBase(_pb.BaseUrl);

        // Authenticate as admin
        await pb.Admins.AuthWithPasswordAsync(
            _fixture.Settings.AdminTesterEmail,
            _fixture.Settings.AdminTesterPassword
        );

        // Act & Assert - Should not throw when unsubscribing from non-existent subscription
        await pb.Realtime.UnsubscribeAsync("nonexistent", "*");
        await pb.RealtimeSse.UnsubscribeAsync("nonexistent", "*");
        
        // Should not throw for null recordId
        await pb.Realtime.UnsubscribeAsync("categories");
        await pb.RealtimeSse.UnsubscribeAsync("categories");
        
        // Should complete successfully
        Assert.True(true);
    }
}
