using System;
using System.Web;
using PocketBase.Blazor.Http;

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

    }
}

