using System;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Clients.Record
{
    public interface IRecordClient
    {
        Task<Result<RecordModel>> GetRecordAsync(string collectionIdOrName, string recordId);
    }
}
