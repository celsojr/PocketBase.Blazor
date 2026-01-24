using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.IdentityModel.Tokens;
using PocketBase.Blazor.Extensions;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Requests.Settings;
using PocketBase.Blazor.Responses.Auth;
using PocketBase.Blazor.Responses.Settings;

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
        public Task<Result<SettingsResponse>> GetListAsync(CancellationToken cancellationToken = default)
        {
            return _http.SendAsync<SettingsResponse>(HttpMethod.Get, "api/settings", cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<Result> UpdateAsync(object settings, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(settings);
            return _http.SendAsync(HttpMethod.Patch, "api/settings", body: settings, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result<bool>> TestS3Async(string fileSystem = "storage", CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            var query = options?.ToDictionary();
            var response = await _http.SendAsync(HttpMethod.Post, $"api/settings/test-s3/{fileSystem}", query: query, cancellationToken: cancellationToken);
            return response.IsSuccess ? Result.Ok(true) : Result.Fail(response.Errors);
        }

        /// <inheritdoc />
        public async Task<Result<bool>> TestEmailAsync(string collectionIdOrName, string toEmail, string emailTemplate, CommonOptions? options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) throw new ArgumentException("Recipient email is required.", nameof(toEmail));
            if (string.IsNullOrWhiteSpace(emailTemplate)) throw new ArgumentException("Email template is required.", nameof(emailTemplate));

            var body = new
            {
                collection = collectionIdOrName,
                email = toEmail,
                template = emailTemplate
            };

            var query = options?.ToDictionary();
            var response = await _http.SendAsync(HttpMethod.Post, "api/settings/test/email", body: body, query: query, cancellationToken: cancellationToken);
            return response.IsSuccess ? Result.Ok(true) : Result.Fail(response.Errors);
        }

        /// <inheritdoc />
        public Task<Result<AppleClientSecretResponse>> GenerateAppleClientSecretAsync(
            ClientSecretConfigRequest request, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.ClientId)) throw new ArgumentException("ClientId is required.", nameof(request.ClientId));
            if (string.IsNullOrWhiteSpace(request.TeamId)) throw new ArgumentException("TeamId is required.", nameof(request.TeamId));
            if (string.IsNullOrWhiteSpace(request.KeyId)) throw new ArgumentException("KeyId is required.", nameof(request.KeyId));
            if (string.IsNullOrWhiteSpace(request.PrivateKey)) throw new ArgumentException("PrivateKey is required.", nameof(request.PrivateKey));
            if (request.Duration <= 0) throw new ArgumentException("Duration must be positive.", nameof(request.Duration));

            var body = new
            {
                clientId = request.ClientId,
                teamId = request.TeamId,
                keyId = request.KeyId,
                privateKey = request.PrivateKey,
                duration = request.Duration
            };

            var query = options?.ToDictionary();
            return _http.SendAsync<AppleClientSecretResponse>(HttpMethod.Post, "api/settings/apple/generate-client-secret", body: body, query: query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public string CreateClientSecret(ClientSecretConfigRequest config)
        {
            ArgumentNullException.ThrowIfNull(config);
            if (string.IsNullOrWhiteSpace(config.ClientId)) throw new ArgumentException("ClientId is required");
            if (string.IsNullOrWhiteSpace(config.TeamId)) throw new ArgumentException("TeamId is required");
            if (string.IsNullOrWhiteSpace(config.KeyId)) throw new ArgumentException("KeyId is required");
            if (string.IsNullOrWhiteSpace(config.PrivateKey)) throw new ArgumentException("PrivateKey is required");
            if (config.Duration <= 0) throw new ArgumentException("ExpiresIn must be positive");
            if (config.Duration > 15777000) throw new ArgumentException("ExpiresIn cannot exceed 15777000 seconds (~6 months)");

            var privateKeyBytes = Convert.FromBase64String(config.PrivateKey);
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportECPrivateKey(privateKeyBytes, out _);

            var securityKey = new ECDsaSecurityKey(ecdsa)
            {
                KeyId = config.KeyId
            };

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = "https://appleid.apple.com",
                Issuer = config.TeamId,
                Subject = new ClaimsIdentity(
                [
                    new Claim("sub", config.ClientId)
                ]),
                Expires = DateTime.UtcNow.AddSeconds(config.Duration),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
        
            return tokenHandler.WriteToken(token);
        }
    }
}

