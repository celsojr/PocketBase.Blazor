using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Exceptions;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Models.Collection;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Collections
{
    /// <summary>
    /// Represents a client for a single PocketBase collection.
    /// Provides authentication, CRUD operations, file uploads, and realtime subscriptions.
    /// </summary>
    public interface ICollectionClient : IBaseClient
    {
        /// <summary>
        /// Imports the provided collections.
        /// </summary>
        /// <param name="collections">The collections to import.</param>
        /// <param name="deleteMissing">Whether to delete missing fields. If <c>deleteMissing</c> is <c>true</c>, all local collections and their fields,
        /// that are not present in the imported configuration, WILL BE DELETED
        /// (including their related records data)!</param>
        /// <param name="options">Additional options for the import.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A result indicating the success or failure of the import.</returns>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result> ImportAsync(IReadOnlyList<CollectionCreateModel> collections, bool deleteMissing = false, CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the scaffold for the collection.
        /// </summary>
        /// <param name="options">Additional options for the request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Returns type indexed map with scaffolded collection models populated with their default field values.</returns>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<CollectionModel>> GetScaffoldsAsync(CommonOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all records associated with the specified collection.
        /// </summary>
        /// <param name="collectionNameOrId">The name or ID of the collection to truncate.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A result indicating the success or failure of the operation.</returns>
        /// <exception cref="ClientResponseError">Thrown when the client receives an invalid response.</exception>
        Task<Result> TruncateAsync(string collectionNameOrId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new collection.
        /// </summary>
        /// <param name="model">The model representing the collection to create.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task representing the asynchronous operation, with a result of the created collection.</returns>
        /// <exception cref="ClientResponseError">
        /// Thrown when the client receives an invalid response.
        /// </exception>
        Task<Result<CollectionModel>> CreateAsync(CollectionCreateModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing collection.
        /// </summary>
        /// <param name="collectionIdOrName">The ID or name of the collection to update.</param>
        /// <param name="model">The updated collection data.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A task representing the asynchronous operation, with a result of the updated collection.</returns>
        Task<Result<CollectionModel>> UpdateAsync(string collectionIdOrName, CollectionUpdateModel model, CancellationToken cancellationToken = default);
    }
}

