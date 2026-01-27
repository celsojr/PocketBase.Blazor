namespace PocketBase.Blazor.IntegrationTests.Clients.Realtime;

using Blazor.Events;
using Blazor.Responses;
using Xunit.Abstractions;

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
        var col = _pb.Realtime.SubscribeAsync("<collection_name>", "<record_id>", _ => { });
    }

    //[Fact]
    //public async Task Debug_SseConnectionOnly()
    //{
    //    // Act - Just try to connect and see what we get
    //    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    //    var messages = new List<string>();

    //    try
    //    {
    //        await foreach (var line in _pb.Realtime.SubscribeAsync(
    //            HttpMethod.Get, 
    //            "api/realtime",
    //            cancellationToken: cts.Token))
    //        {
    //            _output.WriteLine($"SSE Line: {line}");
    //            messages.Add(line);

    //            if (messages.Count >= 3) // Get first few messages
    //            {
    //                cts.Cancel();
    //                break;
    //            }
    //        }
    //    }
    //    catch (OperationCanceledException)
    //    {
    //        _output.WriteLine("SSE connection test completed (cancelled)");
    //    }

    //    // Assert
    //    messages.Should().NotBeEmpty();
    //    messages.Should().Contain(line => line.Contains("PB_CONNECT"));
    //    messages.Should().Contain(line => line.Contains("clientId"));
    //}

    //[Fact]
    //public async Task Debug_WhatHappensOnRealtimeGet()
    //{
    //    await _pb.Realtime.ConnectWebSocketAsync(CancellationToken.None);
    //    // Let's debug what actually happens
    //    //var response = await _pb.SendAsync(
    //    //    HttpMethod.Get, 
    //    //    "api/realtime");

    //    //_output.WriteLine($"Status: {response.StatusCode}");
    //    //_output.WriteLine($"Content: {response.Content}");
    //    //_output.WriteLine($"Headers: {string.Join(", ", response.Headers)}");

    //    // If this returns 101 Switching Protocols, we need WebSocket
    //    // If it returns something else, we might be able to parse clientId
    //}

    //[Fact]
    //public async Task SubscribeAsync_ShouldConnectToRealtimeEndpoint()
    //{
    //    // If the connected client doesn't receive any new messages for 5 minutes,
    //    // the server will send a disconnect signal (this is to prevent forgotten/leaked connections)
    //    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    //    await _pb.Realtime.SubscribeAsync(
    //        topic: "*",
    //        callback: evt =>
    //        {
    //            if (evt.Event == "PB_CONNECT")
    //            {
    //                var json = JsonDocument.Parse(evt.Data);
    //                var clientId = json.RootElement.GetProperty("clientId").GetString();
    //                _output.WriteLine($"Connected as {clientId}");
    //            }

    //            _output.WriteLine($"{evt.Event}: {evt.Data}");
    //        },
    //        cancellationToken: cts.Token);
    //}

    //[Fact]
    //public async Task SubscribeAsync_ShouldConnectToRealtimeEndpoint()
    //{
    //    // Arrange
    //    var topic = "_pb_users_auth_";
    //    var callbackInvoked = false;
    //    var callback = new Action<RealtimeEvent>(e =>
    //    {
    //        _output.WriteLine($"Received event: {e.Action} on {e.Topic} data: {e.Data}");
    //        callbackInvoked = true;
    //    });

    //    // Act

    //    // First, ensure we're authenticated (important for realtime)
    //    //await _fixture.AuthenticateAsync();

    //    // Subscribe to topic
    //    var result = await _pb.Realtime.SubscribeAsync(topic, callback);

    //    // Assert
    //    result.Should().BeTrue();
    //    _pb.Realtime.IsConnected.Should().BeTrue();

    //    // Now trigger an auth event to test
    //    await Task.Delay(2000); // Give connection time to establish

    //    // Create a new user to trigger auth event
    //    var userResult = await _pb.Collection("categories")
    //        .CreateAsync<RecordResponse>(new
    //        {
    //            email = "testing-realtime@email.com",
    //            name = "Testing Realtime",
    //        });

    //    userResult.IsSuccess.Should().BeTrue();

    //    // Wait for potential event
    //    await Task.Delay(5000);

    //    // The test might not receive an event immediately, 
    //    // but connection should be established
    //    _output.WriteLine($"Test completed. Callback invoked: {callbackInvoked}");
    //}

    //[Fact]
    //public async Task SubscribeAsync_ShouldReceiveEvents_WhenDataChanges()
    //{
    //    // Arrange
    //    var collectionName = $"test_{Guid.NewGuid():N}";
    //    var topic = collectionName;

    //    // First create a collection via Admin API
    //    await _fixture.CreateCollectionAsync(collectionName);

    //    var receivedEvents = new List<RealtimeEvent>();
    //    var eventReceived = new TaskCompletionSource<bool>();

    //    var callback = new Action<RealtimeEvent>(e =>
    //    {
    //        _output.WriteLine($"Event received: {e.Action} on {e.Collection}");
    //        receivedEvents.Add(e);
    //        if (e.Action == "create")
    //            eventReceived.TrySetResult(true);
    //    });

    //    // Subscribe to collection events
    //    await _client.SubscribeAsync(topic, callback);

    //    // Act - Create a record in the collection
    //    await Task.Delay(500); // Give subscription time to establish
    //    await _fixture.CreateRecordAsync(collectionName, new { name = "Test Item" });

    //    // Wait for event with timeout
    //    var completedTask = await Task.WhenAny(
    //        eventReceived.Task,
    //        Task.Delay(5000)
    //    );

    //    // Assert
    //    completedTask.Should().Be(eventReceived.Task, "Should have received a realtime event");
    //    receivedEvents.Should().Contain(e => 
    //        e.Collection == collectionName && e.Action == "create");
    //}

}
