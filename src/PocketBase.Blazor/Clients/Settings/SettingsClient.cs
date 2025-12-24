using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Extensions;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Settings
{
    /// <inheritdoc />
    public class SettingsClient : ISettingsClient
    {
        private readonly IHttpTransport _http;

        /// <inheritdoc />
        public SettingsClient(IHttpTransport http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        /// <inheritdoc />
        public Task<JsonElement> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _http.SendAsync<JsonElement>(HttpMethod.Get, "/api/settings", cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(object settings, CancellationToken cancellationToken = default)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            return _http.SendAsync(HttpMethod.Patch, "/api/settings", body: settings, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> TestS3(CommonOptions? options, string fileSystem = "storage", CancellationToken cancellationToken = default)
        {
            var query = options?.ToDictionary();
            await _http.SendAsync(HttpMethod.Post, $"/api/settings/test-s3/{fileSystem}", query: query, cancellationToken: cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> TestEmailAsync(string collectionIdOrName, string toEmail, string emailTemplate, CommonOptions? options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionIdOrName)) throw new ArgumentException("Collection id or name is required.", nameof(collectionIdOrName));
            if (string.IsNullOrWhiteSpace(toEmail)) throw new ArgumentException("Recipient email is required.", nameof(toEmail));
            if (string.IsNullOrWhiteSpace(emailTemplate)) throw new ArgumentException("Email template is required.", nameof(emailTemplate));

            var body = new
            {
                collection = collectionIdOrName,
                to = toEmail,
                template = emailTemplate
            };

            var query = options?.ToDictionary();
            await _http.SendAsync(HttpMethod.Post, "/api/settings/test-email", body: body, query: query, cancellationToken: cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public Task<AppleClientSecretResponse> GenerateAppleClientSecretAsync(string clientId, string teamId, string keyId, string privateKey, int duration, CommonOptions? options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentException("ClientId is required.", nameof(clientId));
            if (string.IsNullOrWhiteSpace(teamId)) throw new ArgumentException("TeamId is required.", nameof(teamId));
            if (string.IsNullOrWhiteSpace(keyId)) throw new ArgumentException("KeyId is required.", nameof(keyId));
            if (string.IsNullOrWhiteSpace(privateKey)) throw new ArgumentException("PrivateKey is required.", nameof(privateKey));
            if (duration <= 0) throw new ArgumentException("Duration must be positive.", nameof(duration));

            var body = new
            {
                clientId,
                teamId,
                keyId,
                privateKey,
                duration
            };

            var query = options?.ToDictionary();
            return _http.SendAsync<AppleClientSecretResponse>(HttpMethod.Post, "/api/settings/apple/client-secret", body: body, query: query, cancellationToken: cancellationToken);
        }
    }
}

