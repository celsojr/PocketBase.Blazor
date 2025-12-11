using System;
using FluentResults;
using System.Threading.Tasks;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Clients.Record
{
    public class RecordClient : IRecordClient
    {
        public Task<Result<RecordModel>> GetRecordAsync(string collectionIdOrName, string recordId)
        {
            throw new NotImplementedException();
        }
    }
}
