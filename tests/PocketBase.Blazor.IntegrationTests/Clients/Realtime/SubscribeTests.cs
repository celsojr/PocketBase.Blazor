namespace PocketBase.Blazor.IntegrationTests.Clients.Realtime;

using Blazor.Events;
using Blazor.Responses;
using Xunit.Abstractions;
using Extensions;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class SubscribeTests
{
    private readonly PocketBaseAdminFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly IPocketBase _pb;

    public SubscribeTests(PocketBaseAdminFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _pb = _fixture.Client;
    }

    [Fact]
    public async Task SubscribeAsync_ShouldReceiveEvents_WhenDataChanges()
    {
        List<RealtimeRecordEvent> eventsReceived = new List<RealtimeRecordEvent>();

        using (await _pb.Collection("categories").SubscribeAsync("*", evt =>
        {
            _output.WriteLine($"Received: {evt.Action} - {evt.RecordId}");
            eventsReceived.Add(evt);
        }))
        {
            Result<RecordResponse> record = await _pb.Collection("categories").CreateAsync<RecordResponse>(new
            {
                name = "Test Category",
                slug = "test-category",
            });

            record.IsSuccess.Should().BeTrue();

            await _pb.Collection("categories").DeleteAsync(record.Value.Id);
            await Task.Delay(3_000);
        }

        eventsReceived.Should().NotBeEmpty();
        eventsReceived.Should().Contain(evt => evt.Action == "create");
        eventsReceived.Should().Contain(evt => evt.Action == "delete");
    }

    [Fact]
    public async Task RealtimeSse_ShouldStreamParsedEvents()
    {
        List<RealtimeRecordEvent> events = new List<RealtimeRecordEvent>();
        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Subscribe to trigger events
        Task subscriptionTask = Task.Run(async () =>
        {
            await foreach (RealtimeRecordEvent evt in _pb.RealtimeSse.SubscribeAsync("categories", "*", cancellationToken: cts.Token))
            {
                _output.WriteLine($"Parsed Event: {evt.Action} - {evt.RecordId}");
                events.Add(evt);

                if (events.Count >= 1) // Wait for at least one real event
                {
                    await cts.CancelAsync();
                    break;
                }
            }
        }, cts.Token);

        // Give subscription time to establish
        await Task.Delay(1000);

        // Trigger an event
        Result<RecordResponse> record = await _pb.Collection("categories")
            .CreateAsync<RecordResponse>(new
            {
                name = "SSE Test Category",
                slug = "sse-test-category",
            });

        // Give time for event to propagate
        await Task.Delay(1000);

        try
        {
            await subscriptionTask;
        }
        catch (OperationCanceledException)
        {
            // The cancellation inside the subscriptionTask will throw this exception
            _output.WriteLine("Stream completed");
        }
        finally
        {
            // We don't care about cleanup events here
            await _pb.Collection("categories")
                .DeleteAsync(record.Value.Id);
        }

        events.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RealtimeSse_IsConnected_ShouldReflectConnectionState()
    {
        // Create a separate instance for this test to avoid affecting other tests
        await using PocketBase pb = new PocketBase(_pb.BaseUrl);

        // Authenticate as admin
        await pb.Admins.AuthWithPasswordAsync(
            _fixture.Settings.AdminTesterEmail,
            _fixture.Settings.AdminTesterPassword
        );

        // Initially should be disconnected
        pb.RealtimeSse.IsConnected.Should().BeFalse();

        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        bool eventReceived = false;

        // Start streaming (this will trigger connection)
        Task streamingTask = Task.Run(async () =>
        {
            await foreach (RealtimeRecordEvent evt in pb.RealtimeSse.SubscribeAsync(
                "categories", "*", cancellationToken: cts.Token))
            {
                eventReceived = true;
                _output.WriteLine($"Received event: {evt.Action} - {evt.RecordId}");
                await cts.CancelAsync();
                break;
            }
        }, cts.Token);

        // Give connection time to establish
        await Task.Delay(2000);

        // Should now be connected
        pb.RealtimeSse.IsConnected.Should().BeTrue();

        // Create a record to trigger an event
        Result<RecordResponse> record = await pb.Collection("categories")
            .CreateAsync<RecordResponse>(new
            {
                name = "Connection Test",
                slug = "connection-test",
            });

        try
        {
            await streamingTask;
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Streaming cancelled as expected");
        }
        finally
        {
            // Cleanup record
            await pb.Collection("categories")
                .DeleteAsync(record.Value.Id);
        }

        // Verify we received an event
        eventReceived.Should().BeTrue();

        // The instance will be disposed via using statement

        // Cleanup
        // No need to manually call DisposeAsync in a real application since we're using 'await using'
        await pb.RealtimeSse.DisposeAsync();

        // Verify disconnection after disposal
        pb.RealtimeSse.IsConnected.Should().BeFalse();
    }
}
