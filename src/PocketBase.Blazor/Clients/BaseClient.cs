using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FluentResults;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients
{
    /// <inheritdoc />
    public abstract class BaseClient : IBaseClient
    {
        /// <inheritdoc />
        public IHttpTransport Http { get; }

        /// <inheritdoc />
        protected abstract string BasePath { get; }

        /// <inheritdoc />
        protected BaseClient(IHttpTransport http)
        {
            Http = http ?? throw new ArgumentNullException(nameof(http));
        }

        /// <inheritdoc />
        public string UrlEncode(string? param)
        {
            return HttpUtility.UrlEncode(param) ?? "";
        }

        /// <inheritdoc />
        public virtual Task<Result<ListResult<T>>> GetListAsync<T>(int page = 1, int perPage = 30, ListOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= new ListOptions();

            options.Query ??= new Dictionary<string, object?>();
            options.Query["page"] = page;
            options.Query["perPage"] = perPage;

            return Http.SendAsync<ListResult<T>>(HttpMethod.Get, BasePath, query: options.Query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        private async Task<Result<List<T>>> GetFullListInternalAsync<T>(int batch, ListOptions? options, CancellationToken cancellationToken)
        {
            var result = new List<T>();
            var page = 1;

            while (true)
            {
                var pageResult = await GetListAsync<T>(
                    page: page,
                    perPage: batch,
                    options: options,
                    cancellationToken: cancellationToken
                );

                if (pageResult.IsFailed)
                    return Result.Fail(pageResult.Errors);

                result.AddRange(pageResult.Value.Items);

                if (page >= pageResult.Value.TotalPages)
                    break;

                page++;
            }

            return Result.Ok(result);
        }

        /// <inheritdoc />
        public Task<Result<List<T>>> GetFullListAsync<T>(CancellationToken cancellationToken = default)
        {
            return GetFullListInternalAsync<T>(
                batch: 500,
                options: null,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<Result<List<T>>> GetFullListAsync<T>(int batch, CancellationToken cancellationToken = default)
        {
            return GetFullListInternalAsync<T>(
                batch,
                options: null,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<Result<List<T>>> GetFullListAsync<T>(FullListOptions options, CancellationToken cancellationToken = default)
        {
            var batch = options.Batch ?? 500;
            return GetFullListInternalAsync<T>(
                batch,
                options,
                cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<Result<T>> GetOneAsync<T>(string id, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Task.FromResult(
                    Result.Fail<T>(
                        new Error("Missing required record id.")
                            .WithMetadata("status", 404)
                    )
                );
            }

            var url = $"{BasePath}/{UrlEncode(id)}";

            return Http.SendAsync<T>(HttpMethod.Get, url, query: options?.Query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<Result<T>> CreateAsync<T>(object? body = null, CommonOptions? options = null, CancellationToken cancellationToken = default)
            where T : BaseModel
        {
            return Http.SendAsync<T>(
                HttpMethod.Post,
                BasePath,
                body: body ?? options?.Body,
                query: options?.Query,
                cancellationToken: cancellationToken);
        }

    }
}

