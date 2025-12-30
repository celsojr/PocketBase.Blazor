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
        public virtual Task<Result<ListResult<T>>> GetListAsync<T>(int page = 1, int perPage = 30, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= new CommonOptions();

            options.Query ??= new Dictionary<string, object?>();
            options.Query["page"] = page;
            options.Query["perPage"] = perPage;

            return Http.SendAsync<ListResult<T>>(HttpMethod.Get, BasePath, query: options.Query, cancellationToken: cancellationToken);
        }
    }
}

