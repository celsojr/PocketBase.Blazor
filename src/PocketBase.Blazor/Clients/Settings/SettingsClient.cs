using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Settings
{
    /// <inheritdoc />
    public class SettingsClient : ISettingsClient
    {
        readonly IHttpTransport _http;

        /// <inheritdoc />
        public SettingsClient(IHttpTransport http)
        {
            _http = http;
        }

        /// <inheritdoc />
        public Task<JsonElement> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task UpdateAsync(object settings, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> TestS3(CommonOptions? options, string fileSystem = "storage", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> TestEmailAsync(string collectionIdOrName, string toEmail, string emailTemplate, CommonOptions? options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<AppleClientSecretResponse> GenerateAppleClientSecretAsync(string clientId, string teamId, string keyId, string privateKey, int duration, CommonOptions? options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

