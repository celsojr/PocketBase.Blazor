namespace PocketBase.Blazor.UnitTests.Domain.Record;

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Blazor.Clients.Record;
using Blazor.Clients.Realtime;
using Blazor.Events;
using Blazor.Http;
using Blazor.Options;
using Blazor.Requests.Auth;
using Blazor.Responses.Auth;
using Blazor.Store;
using FluentResults;
using FluentAssertions;

[Trait("Category", "Unit")]
public class RecordClientAuthBoundaryTests
{
    [Fact]
    public async Task AuthWithPasswordAsync_ShouldThrowForSuperusersCollection()
    {
        RecordClient client = CreateSuperusersClient();

        Func<Task> act = async () =>
            await client.AuthWithPasswordAsync("admin@example.com", "secret");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*_superusers*Records endpoint*Admins endpoint*");
    }

    [Fact]
    public async Task AuthWithOAuth2CodeAsync_ShouldThrowForSuperusersCollection()
    {
        RecordClient client = CreateSuperusersClient();

        Func<Task> act = async () =>
            await client.AuthWithOAuth2CodeAsync(new AuthWithOAuth2Request
            {
                Provider = "github",
                Code = "abc",
                CodeVerifier = "verifier",
                RedirectUrl = "http://localhost/callback",
            });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*_superusers*Records endpoint*Admins endpoint*");
    }

    [Fact]
    public async Task RequestOtpAsync_ShouldThrowForSuperusersCollection()
    {
        RecordClient client = CreateSuperusersClient();

        Func<Task> act = async () =>
            await client.RequestOtpAsync("admin@example.com");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*_superusers*Records endpoint*Admins endpoint*");
    }

    [Fact]
    public async Task AuthWithOtpAsync_ShouldThrowForSuperusersCollection()
    {
        RecordClient client = CreateSuperusersClient();

        Func<Task> act = async () =>
            await client.AuthWithOtpAsync("otp_id", "123456", new CommonOptions());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*_superusers*Records endpoint*Admins endpoint*");
    }

    [Fact]
    public async Task AuthRefreshAsync_ShouldThrowForSuperusersCollection()
    {
        RecordClient client = CreateSuperusersClient();

        Func<Task> act = async () =>
            await client.AuthRefreshAsync(new CommonOptions());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*_superusers*Records endpoint*Admins endpoint*");
    }

    [Fact]
    public async Task AuthWithPasswordAsync_ShouldCallTransportForRegularCollection()
    {
        FakeHttpTransport transport = new FakeHttpTransport
        {
            AuthWithPasswordResult = Result.Ok(new AuthResponse
            {
                Token = "token",
                Record = null,
            }),
        };
        PocketBaseStore store = new PocketBaseStore(new AuthStore(), new NoopRealtimeClient(), new NoopRealtimeStreamClient());
        RecordClient client = new RecordClient("users", transport, store);

        Result<AuthResponse> result = await client.AuthWithPasswordAsync("user@example.com", "secret");

        result.IsSuccess.Should().BeTrue();
        transport.AuthWithPasswordCalled.Should().BeTrue();
    }

    private static RecordClient CreateSuperusersClient()
    {
        PocketBaseStore store = new PocketBaseStore(new AuthStore(), new NoopRealtimeClient(), new NoopRealtimeStreamClient());
        return new RecordClient("_superusers", new FakeHttpTransport(), store);
    }

    private sealed class FakeHttpTransport : IHttpTransport
    {
        public string BaseUrl => "http://localhost";

        public bool AuthWithPasswordCalled { get; private set; }

        public Result<AuthResponse>? AuthWithPasswordResult { get; init; }

        public string BuildUrl(string endpoint) => $"{BaseUrl}/{endpoint.TrimStart('/')}";

        public Task<Result<T>> SendAsync<T>(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            if (method == HttpMethod.Post &&
                path == "api/collections/users/auth-with-password" &&
                typeof(T) == typeof(AuthResponse))
            {
                AuthWithPasswordCalled = true;
                Result<AuthResponse> authResult = AuthWithPasswordResult ?? Result.Fail<AuthResponse>("No configured auth result.");
                return Task.FromResult((Result<T>)(object)authResult);
            }

            throw new NotSupportedException($"Unexpected request in test double: {method} {path} ({typeof(T).Name})");
        }

        public Task<Result> SendAsync(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> SendAsync(HttpMethod method, string path, MultipartFile file,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<Stream>> SendForStreamAsync(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<byte[]>> SendForBytesAsync(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<string> SendForSseAsync(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }

    private sealed class NoopRealtimeClient : IRealtimeClient
    {
        public bool IsConnected => false;

        public event Action<IReadOnlyList<string>> OnDisconnect
        {
            add { }
            remove { }
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public Task<IDisposable> SubscribeAsync(string collection, string recordId, Action<RealtimeRecordEvent> onEvent,
            CommonOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class NoopRealtimeStreamClient : IRealtimeStreamClient
    {
        public bool IsConnected => false;

        public event Action<IReadOnlyList<string>> OnDisconnect
        {
            add { }
            remove { }
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public IAsyncEnumerable<RealtimeRecordEvent> SubscribeAsync(string collection, string recordId,
            CommonOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
