using System;
using System.Collections.Generic;
using System.Linq;
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
        public virtual async Task<Result<ListResult<T>>> GetListAsync<T>(int page = 1, int perPage = 30, ListOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= new ListOptions();
            options.Query = options.BuildQuery(page, perPage);

            return await Http.SendAsync<ListResult<T>>(HttpMethod.Get, BasePath, query: options.Query, cancellationToken: cancellationToken);
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
        public virtual async Task<Result<List<T>>> GetFullListAsync<T>(CancellationToken cancellationToken = default)
        {
            return await GetFullListInternalAsync<T>(
                batch: 500,
                options: null,
                cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<Result<List<T>>> GetFullListAsync<T>(int batch, CancellationToken cancellationToken = default)
        {
            return await GetFullListInternalAsync<T>(
                batch,
                options: null,
                cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<Result<List<T>>> GetFullListAsync<T>(FullListOptions options, CancellationToken cancellationToken = default)
        {
            var batch = options.Batch ?? 500;
            return await GetFullListInternalAsync<T>(
                batch,
                options,
                cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<Result<T>> GetOneAsync<T>(string? id, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return await Task.FromResult(
                    Result.Fail<T>(
                        new Error("Missing required item id.")
                            .WithMetadata("status", 404)
                    )
                );
            }

            options ??= new CommonOptions();
            options.Query = options.BuildQuery();

            var url = $"{BasePath}/{UrlEncode(id)}";

            return await Http.SendAsync<T>(HttpMethod.Get, url, query: options?.Query, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<Result<T>> CreateAsync<T>(object? body = null, CommonOptions? options = null, CancellationToken cancellationToken = default)
            where T : BaseModel
        {
            var mergedBody = MergeBodies(body, options?.Body as Dictionary<string, object?>);
            return await Http.SendAsync<T>(
                HttpMethod.Post,
                BasePath,
                mergedBody,
                options?.Query,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<Result<T>> UpdateAsync<T>(string? id, object? body = null, CommonOptions? options = null, CancellationToken cancellationToken = default)
            where T : BaseModel
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Collection id or name is required.", nameof(id));

            var mergedBody = MergeBodies(body, options?.Body as Dictionary<string, object?>);
            return await Http.SendAsync<T>(
                HttpMethod.Patch,
                $"{BasePath}/{UrlEncode(id)}",
                mergedBody,
                query: options?.Query,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result> DeleteAsync(string? id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return await Task.FromResult(
                    Result.Fail(
                        new Error("Missing required item id.")
                            .WithMetadata("status", 404)
                    )
                );
            }

            return await Http.SendAsync(HttpMethod.Delete, $"{BasePath}/{UrlEncode(id)}", cancellationToken: cancellationToken);
        }

        private static Dictionary<string, object?> ToDictionary(object obj)
        {
            return obj.GetType()
                      .GetProperties()
                      .Where(p => p.CanRead)
                      .ToDictionary(
                          p => p.Name,
                          p => (object?)p.GetValue(obj)
                      );
        }

        private static object? MergeBodies(object? body, Dictionary<string, object?>? optionsBody)
        {
            if (body == null) return optionsBody;
            if (optionsBody == null && body is not IDictionary<string, object?>) return body;

            // convert main body to dictionary if needed
            var mainDict = body is IDictionary<string, object?> d ? d : ToDictionary(body);
            var result = new Dictionary<string, object?>(optionsBody ?? new Dictionary<string, object?>());

            // main body overrides options.Body
            foreach (var kv in mainDict)
                result[kv.Key] = kv.Value;

            return result;
        }

    }
}

