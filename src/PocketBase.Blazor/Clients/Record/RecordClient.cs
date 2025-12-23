using System;
using FluentResults;
using System.Threading.Tasks;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Http;
using System.Threading;

namespace PocketBase.Blazor.Clients.Record
{
    public class RecordClient : IRecordClient
    {
        private IHttpTransport _http;

        public RecordClient(IHttpTransport http)
        {
            _http = http;
        }

        public Task<Result<RecordModel>> GetRecordAsync(string collectionIdOrName, string recordId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
