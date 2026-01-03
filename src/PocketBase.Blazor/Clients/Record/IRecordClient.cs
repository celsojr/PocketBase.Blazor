using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Clients.Admin;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses;
using PocketBase.Blazor.Store;

namespace PocketBase.Blazor.Clients.Record
{
    /// <summary>
    /// Provides access to PocketBase collection record CRUD APIs.
    /// Mirrors the behavior of the JS SDK RecordService.
    /// </summary>
    public interface IRecordClient
    {
        /// <summary>
        /// Gets the collection name associated with this client.
        /// </summary>
        public string CollectionName { get; }

        /// <summary>
        /// Authenticates an admin using email and password.
        /// </summary>
        /// <param name="email">Admin email or identity.</param>
        /// <param name="password">Admin password.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The authentication response containing the session token.</returns>
        public Task<Result<AuthResponse>> AuthWithPasswordAsync(string email, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes the currently authenticated admin session.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The updated authentication response.</returns>
        public Task<Result<AuthResponse>> RefreshAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Logs out the currently authenticated admin.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        public Task<Result> LogoutAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the PocketBase store for managing authentication state.
        /// </summary>
        /// <param name="store">The PocketBase store instance.</param>
        public void SetStore(PocketBaseStore store);

        ///// <summary>Gets a paginated list of records.</summary>
        ///// <param name="collection">The collection name.</param>
        ///// <param name="page">The page number.</param>
        ///// <param name="perPage">The number of records per page.</param>
        ///// <param name="options">Additional options for the request.</param>
        ///// <param name="cancellationToken">A cancellation token for the request.</param>
        //Task<Result<ListResult<RecordResponse>>> GetListAsync(string collection, int page = 1, int perPage = 30, CommonOptions? options = null, CancellationToken cancellationToken = default);

        ///// <summary>Gets all records across pages (no pagination).</summary>
        ///// <param name="collection">The collection name.</param>
        ///// <param name="options">Additional options for the request.</param>
        ///// <param name="cancellationToken">A cancellation token for the request.</param>
        //Task<Result<List<RecordResponse>>> GetFullListAsync(string collection, CommonOptions? options = null, CancellationToken cancellationToken = default);

        ///// <summary>Retrieves the first record matching a filter.</summary>
        ///// <param name="collection">The collection name.</param>
        ///// <param name="filter">The filter to apply.</param>
        ///// <param name="options">Additional options for the request.</param>
        ///// <param name="cancellationToken">A cancellation token for the request.</param>
        //Task<Result<RecordResponse>> GetFirstListItemAsync(string collection, string filter, CommonOptions? options = null, CancellationToken cancellationToken = default);

        ///// <summary>Retrieves a single record by its ID.</summary>
        ///// <param name="collection">The collection name.</param>
        ///// <param name="recordId">The record ID.</param>
        ///// <param name="options">Additional options for the request.</param>
        ///// <param name="cancellationToken">A cancellation token for the request.</param>
        //Task<Result<RecordResponse>> GetOneAsync(string collection, string recordId, CommonOptions? options = null, CancellationToken cancellationToken = default);

        ///// <summary>Creates a new record in the collection.</summary>
        ///// <param name="collection">The collection name.</param>
        ///// <param name="bodyParams">The request body parameters.</param>
        ///// <param name="options">Additional options for the request.</param>
        ///// <param name="cancellationToken">A cancellation token for the request.</param>
        //Task<Result<RecordResponse>> CreateAsync(string collection, object bodyParams, CommonOptions? options = null, CancellationToken cancellationToken = default);

        ///// <summary>Updates an existing record by its ID.</summary>
        ///// <param name="collection">The collection name.</param>
        ///// <param name="recordId">The record ID.</param>
        ///// <param name="bodyParams">The request body parameters.</param>
        ///// <param name="options">Additional options for the request.</param>
        ///// <param name="cancellationToken">A cancellation token for the request.</param>
        //Task<Result<RecordResponse>> UpdateAsync(string collection, string recordId, object bodyParams, CommonOptions? options = null, CancellationToken cancellationToken = default);

        ///// <summary>Deletes a record by its ID.</summary>
        ///// <param name="collection">The collection name.</param>
        ///// <param name="recordId">The record ID.</param>
        ///// <param name="options">Additional options for the request.</param>
        ///// <param name="cancellationToken">A cancellation token for the request.</param>
        //Task<Result<bool>> DeleteAsync(string collection, string recordId, CommonOptions? options = null, CancellationToken cancellationToken = default);
    }
}
