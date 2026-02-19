using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Exceptions;
using PocketBase.Blazor.Requests.Batch;
using PocketBase.Blazor.Responses.Backup;

namespace PocketBase.Blazor.Clients.Batch
{
    /// <summary>
    /// Interface for batch operations on PocketBase collections.
    /// </summary>
    public interface IBatchClient
    {
        /// <summary>
        /// Starts constructing a batch request entry for the specified collection.
        /// </summary>
        /// <param name="collectionName"></param>
        IBatchClient Collection(string collectionName);

        /// <summary>
        /// Registers a record create request into the current batch queue.
        /// </summary>
        /// <param name="body">The record data to create.</param>
        /// <param name="files">The files to attach to the record.</param>
        IBatchClient Create(object body, List<BatchFile>? files = null);

        /// <summary>
        /// Registers a record update request into the current batch queue.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the record to update.
        /// </param>
        /// <param name="body">The updated record data.</param>
        /// <param name="files">The files to attach to the record.</param>
        IBatchClient Update(string id, object body, List<BatchFile>? files = null);

        /// <summary>
        /// Registers a record delete request into the current batch queue.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the record to delete.
        /// </param>
        IBatchClient Delete(string id);

        /// <summary>
        /// Registers a record upsert request into the current batch queue.
        /// The request will be executed as update if `bodyParams` have a valid existing record `id` value, otherwise - create.
        /// </summary>
        /// <param name="body">
        /// The record data to upsert.
        /// </param>
        /// <returns></returns>
        IBatchClient Upsert(object body);

        /// <summary>
        /// Sends the batch requests.
        /// </summary>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<List<BatchResponse>>> SendAsync(CancellationToken cancellationToken = default);
    }
}
