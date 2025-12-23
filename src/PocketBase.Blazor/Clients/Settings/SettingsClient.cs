using System;
using System.Text.Json;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;

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
        public Task<JsonElement> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task UpdateAsync(object settings)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> TestS3(CommonOptions? options, string fileSystem = "storage")
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> TestEmailAsync(string collectionIdOrName, string toEmail, string emailTemplate, CommonOptions? options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<AppleClientSecret> GenerateAppleClientSecretAsync(string clientId, string teamId, string keyId, string privateKey, int duration, CommonOptions? options)
        {
            throw new NotImplementedException();
        }
    }
}
