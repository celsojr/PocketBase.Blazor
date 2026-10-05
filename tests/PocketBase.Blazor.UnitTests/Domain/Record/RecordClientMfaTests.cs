namespace PocketBase.Blazor.UnitTests.Domain.Record;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Clients.Record;
using Blazor.Clients.Realtime;
using Blazor.Events;
using Blazor.Exceptions;
using Blazor.Extensions;
using Blazor.Http;
using Blazor.Options;
using Blazor.Requests.Auth;
using Blazor.Responses.Auth;
using Blazor.Store;
using FluentAssertions;
using FluentResults;

[Trait("Category", "Unit")]
public class RecordClientMfaTests
{
    [Fact]
    public async Task AuthWithPasswordAsync_WithMfaId_ShouldSendMfaIdInBody()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        RecordClient client = new RecordClient("users", transport, CreateStore());

        // Act
        Result<AuthResponse> result = await client.AuthWithPasswordAsync(
            "user@example.com",
            "secret123",
            mfaId: "MFA_SESSION_ID");

        // Assert
        result.IsSuccess.Should().BeTrue();
        transport.Requests.Should().ContainSingle();
        transport.Requests[0].Body.Should().BeOfType<Dictionary<string, object?>>();
        Dictionary<string, object?> body = (Dictionary<string, object?>)transport.Requests[0].Body!;
        body["mfaId"].Should().Be("MFA_SESSION_ID");
        body["identity"].Should().Be("user@example.com");
        body["password"].Should().Be("secret123");
    }

    [Fact]
    public async Task AuthWithPasswordAsync_WithoutMfaId_ShouldNotSendMfaId()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        RecordClient client = new RecordClient("users", transport, CreateStore());

        // Act
        await client.AuthWithPasswordAsync("user@example.com", "secret123");

        // Assert
        Dictionary<string, object?> body = (Dictionary<string, object?>)transport.Requests[0].Body!;
        body.Should().NotContainKey("mfaId");
    }

    [Fact]
    public async Task AuthWithOtpAsync_WithMfaId_ShouldSendMfaIdInBody()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        RecordClient client = new RecordClient("users", transport, CreateStore());

        // Act
        Result<AuthResponse> result = await client.AuthWithOtpAsync(
            "otp-id",
            "123456",
            mfaId: "MFA_SESSION_ID");

        // Assert
        result.IsSuccess.Should().BeTrue();
        Dictionary<string, object?> body = (Dictionary<string, object?>)transport.Requests[0].Body!;
        body["otpId"].Should().Be("otp-id");
        body["password"].Should().Be("123456");
        body["mfaId"].Should().Be("MFA_SESSION_ID");
    }

    [Fact]
    public async Task AuthWithOtpAsync_WithoutMfaId_ShouldNotSendMfaId()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        RecordClient client = new RecordClient("users", transport, CreateStore());

        // Act
        await client.AuthWithOtpAsync("otp-id", "123456");

        // Assert
        Dictionary<string, object?> body = (Dictionary<string, object?>)transport.Requests[0].Body!;
        body.Should().NotContainKey("mfaId");
    }

    [Fact]
    public async Task AuthWithOAuth2CodeAsync_WithMfaId_ShouldSendMfaIdAlongsideRequest()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        RecordClient client = new RecordClient("users", transport, CreateStore());

        // Act
        Result<AuthRecordResponse> result = await client.AuthWithOAuth2CodeAsync(
            new AuthWithOAuth2Request
            {
                Provider = "github",
                Code = "auth-code",
            },
            mfaId: "MFA_SESSION_ID");

        // Assert
        result.IsSuccess.Should().BeTrue();
        transport.Requests[0].Body.Should().BeOfType<Dictionary<string, object?>>();
        Dictionary<string, object?> body = (Dictionary<string, object?>)transport.Requests[0].Body!;
        body["mfaId"].Should().Be("MFA_SESSION_ID");
        body["provider"].Should().Be("github");
        body["code"].Should().Be("auth-code");
        body["redirectURL"].Should().BeNull();
    }

    [Fact]
    public async Task AuthWithOAuth2CodeAsync_WithoutMfaId_ShouldSendRequestUnchanged()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        RecordClient client = new RecordClient("users", transport, CreateStore());

        // Act
        await client.AuthWithOAuth2CodeAsync(new AuthWithOAuth2Request
        {
            Provider = "github",
            Code = "auth-code",
        });

        // Assert - request object is forwarded as-is to preserve existing serialization
        transport.Requests[0].Body.Should().BeOfType<AuthWithOAuth2Request>();
    }

    [Fact]
    public async Task AuthRefreshAsync_WithMfaId_ShouldSendMfaIdInBody()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        RecordClient client = new RecordClient("users", transport, CreateStore());

        // Act
        Result<AuthResponse> result = await client.AuthRefreshAsync(mfaId: "MFA_SESSION_ID");

        // Assert
        result.IsSuccess.Should().BeTrue();
        Dictionary<string, object?> body = (Dictionary<string, object?>)transport.Requests[0].Body!;
        body["mfaId"].Should().Be("MFA_SESSION_ID");
    }

    [Fact]
    public async Task AuthRefreshAsync_WithoutMfaId_ShouldSendNoBody()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        RecordClient client = new RecordClient("users", transport, CreateStore());

        // Act
        await client.AuthRefreshAsync();

        // Assert
        transport.Requests[0].Body.Should().BeNull();
    }

    [Fact]
    public void ResultExtensions_GetMfaId_ShouldReturnMfaIdFromRootPayload()
    {
        // Arrange - the real HTTP 401 body PocketBase returns when MFA needs a 2nd method
        const string raw = "{\"mfaId\":\"zxu3cw7276fwxhw\"}";
        PocketBaseException ex = new PocketBaseException(HttpStatusCode.Unauthorized, raw);
        Result<AuthResponse> result = Result.Fail<AuthResponse>(new ExceptionalError(ex));

        // Assert
        result.GetMfaId().Should().Be("zxu3cw7276fwxhw");
        result.GetStatusCode().Should().Be(401);
        result.GetErrorData().Should().BeNull();
    }

    [Fact]
    public void ResultExtensions_GetMfaId_ShouldReturnMfaIdFromErrorPayload()
    {
        // Arrange - mirrors the HTTP 401 body PocketBase returns when MFA needs a 2nd method
        const string raw = "{\"status\":401,\"message\":\"Auth method already completed.\",\"data\":{\"mfaId\":\"LhLQG3py5uMwz0k\"}}";
        PocketBaseException ex = new PocketBaseException(HttpStatusCode.Unauthorized, raw);
        Result<AuthResponse> result = Result.Fail<AuthResponse>(new ExceptionalError(ex));

        // Assert
        result.GetMfaId().Should().Be("LhLQG3py5uMwz0k");
        result.GetStatusCode().Should().Be(401);
        result.GetErrorData().Should().NotBeNull();
        result.GetErrorData()!.Value.TryGetProperty("mfaId", out _).Should().BeTrue();
    }

    [Fact]
    public void ResultExtensions_GetMfaId_ShouldReturnNull_WhenNoMfaPayload()
    {
        // Arrange
        const string raw = "{\"status\":400,\"message\":\"Failed to authenticate.\",\"data\":{\"identity\":{\"code\":\"validation_required\"}}}";
        PocketBaseException ex = new PocketBaseException(HttpStatusCode.BadRequest, raw);
        Result<AuthResponse> result = Result.Fail<AuthResponse>(new ExceptionalError(ex));

        // Assert
        result.GetMfaId().Should().BeNull();
        result.GetStatusCode().Should().Be(400);
    }

    [Fact]
    public void PocketBaseException_WithNonJsonBody_ShouldNotThrow()
    {
        // Arrange & Act
        PocketBaseException ex = new PocketBaseException(
            HttpStatusCode.BadGateway, "<html>502 Bad Gateway</html>");

        // Assert
        ex.Status.Should().Be(502);
        ex.Raw.Should().Be("<html>502 Bad Gateway</html>");
        ex.MfaId.Should().BeNull();
        ex.Data.Should().BeNull();
    }

    [Fact]
    public void PocketBaseException_ShouldPreserveRawMessage()
    {
        // Arrange & Act - the exception message must keep the raw body for backwards compatibility
        const string raw = "{\"status\":404,\"message\":\"The requested resource wasn't found.\",\"data\":{}}";
        PocketBaseException ex = new PocketBaseException(HttpStatusCode.NotFound, raw);

        // Assert
        ex.Message.Should().Be(raw);
        ex.Message.Should().Contain("404");
    }

    private static PocketBaseStore CreateStore()
    {
        return new PocketBaseStore(new AuthStore(), new NoopRealtimeClient(), new NoopRealtimeStreamClient());
    }

    private sealed class FakeHttpTransport : IHttpTransport
    {
        public List<(HttpMethod Method, string Path, object? Body)> Requests { get; } = [];

        public string BaseUrl => "http://localhost";

        public string BuildUrl(string endpoint) => $"{BaseUrl}/{endpoint.TrimStart('/')}";

        public Task<Result<T>> SendAsync<T>(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            Requests.Add((method, path, body));

            if (typeof(T) == typeof(AuthResponse))
            {
                return Task.FromResult((Result<T>)(object)Result.Ok(new AuthResponse { Token = "token" }));
            }

            if (typeof(T) == typeof(AuthRecordResponse))
            {
                return Task.FromResult((Result<T>)(object)Result.Ok(new AuthRecordResponse { Token = "token" }));
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

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

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

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public IAsyncEnumerable<RealtimeRecordEvent> SubscribeAsync(string collection, string recordId,
            CommonOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}