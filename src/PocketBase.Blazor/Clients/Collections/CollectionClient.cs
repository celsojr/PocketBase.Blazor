using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Models.Collection;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses.Scaffolds;

namespace PocketBase.Blazor.Clients.Collections
{
    /// <inheritdoc />
    public class CollectionClient : BaseClient, ICollectionClient
    {
        /// <inheritdoc />
        protected override string BasePath => "api/collections";

        /// <inheritdoc />
        public CollectionClient(IHttpTransport http)
            :base(http)
        {
        }

        /// <inheritdoc />
        public async Task<Result> ImportAsync(IReadOnlyList<CollectionCreateModel> collections, bool deleteMissing = false, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= new CommonOptions();
            options.BuildQuery();

            options.Body = new Dictionary<string, object>
            {
                ["collections"] = collections,
                ["deleteMissing"] = deleteMissing
            };

            return await Http.SendAsync(HttpMethod.Put, $"{BasePath}/import", body: options?.Body, query: options?.Query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result<CollectionScaffoldsResponse>> GetScaffoldsAsync(CommonOptions? options = null,  CancellationToken cancellationToken = default)
        {
            options ??= new CommonOptions();
            options.BuildQuery();

            return await Http.SendAsync<CollectionScaffoldsResponse>(
                HttpMethod.Get,
                $"{BasePath}/meta/scaffolds",
                body: options?.Body,
                query: options?.Query,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public async Task<Result> TruncateAsync(string collectionNameOrId, CancellationToken cancellationToken = default)
        {
            return await Http.SendAsync(HttpMethod.Delete, $"{BasePath}/{UrlEncode(collectionNameOrId)}/truncate", cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<Result<CollectionModel>> CreateAsync(CollectionCreateModel model, CancellationToken cancellationToken = default)
        {
            return await Http.SendAsync<CollectionModel>(
                HttpMethod.Post,
                BasePath,
                body: model,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<Result<CollectionModel>> UpdateAsync(string collectionIdOrName, CollectionUpdateModel model, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(collectionIdOrName))
                throw new ArgumentException("Collection id or name is required.", nameof(collectionIdOrName));

            return await Http.SendAsync<CollectionModel>(
                HttpMethod.Patch,
                $"{BasePath}/{UrlEncode(collectionIdOrName)}",
                body: model,
                cancellationToken: cancellationToken
            );
        }
    }
}

