using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Events;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Requests;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Collections
{
    /// <summary>
    /// Represents a client for a single PocketBase collection.
    /// Provides authentication, CRUD operations, file uploads, and realtime subscriptions.
    /// </summary>
    public interface ICollectionClient
    {
        /// <summary>
        /// Gets the name of the collection this client is bound to.
        /// </summary>
        string Name { get; }

        #region Authentication

        /// <summary>
        /// Authenticates a record in an auth-enabled collection using an identity (username/email) and password.
        /// </summary>
        /// <param name="identity">The user's identity (username or email).</param>
        /// <param name="password">The user's password.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>An <see cref="AuthResponse"/> containing authentication information and tokens.</returns>
        Task<Result<AuthResponse>> AuthWithPasswordAsync(string identity, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes the authentication token for the currently authenticated record.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>An <see cref="AuthResponse"/> with the refreshed token.</returns>
        Task<Result<AuthResponse>> RefreshAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a password reset email for the specified user email.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Retrieves a single record by its ID.
        /// </summary>
        /// <param name="id">The record's ID.</param>
        /// <param name="options">Optional query parameters (e.g., expanded relations).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The requested <see cref="RecordModel"/>.</returns>
        Task<Result<RecordModel>> GetOneAsync(string id, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a paginated list of records.
        /// </summary>
        /// <param name="page">The page number (default: 1).</param>
        /// <param name="perPage">The number of records per page (default: 30).</param>
        /// <param name="options">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A <see cref="ListResult{RecordModel}"/> containing the page of records.</returns>
        Task<Result<ListResult<T>>> GetListAsync<T>(int page = 1, int perPage = 30, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the first record matching the specified filter.
        /// </summary>
        /// <param name="filter">A filter string (e.g., "is_published = true").</param>
        /// <param name="options">Optional query parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The first matching <see cref="RecordModel"/>.</returns>
        Task<Result<RecordModel>> GetFirstAsync(string filter, QueryOptionsRequest? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new record in the collection.
        /// </summary>
        /// <param name="data">An object containing the record fields and values.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The created <see cref="RecordModel"/>.</returns>
        Task<Result<RecordModel>> CreateAsync(object data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing record by its ID.
        /// </summary>
        /// <param name="id">The ID of the record to update.</param>
        /// <param name="data">An object containing the updated fields.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The updated <see cref="RecordModel"/>.</returns>
        Task<Result<RecordModel>> UpdateAsync(string id, object data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a record by its ID.
        /// </summary>
        /// <param name="id">The ID of the record to delete.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);

        #endregion

        #region File Operations

        /// <summary>
        /// Uploads a file to a specific record field.
        /// </summary>
        /// <param name="id">The record ID to upload the file to.</param>
        /// <param name="field">The field name to upload the file to.</param>
        /// <param name="file">The <see cref="Stream"/> containing the file data.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The updated <see cref="RecordModel"/> containing the uploaded file.</returns>
        Task<Result<RecordModel>> UploadFileAsync(string id, string field, Stream file, string fileName, CancellationToken cancellationToken = default);

        #endregion

        #region Realtime

        /// <summary>
        /// Subscribes to a realtime topic related to this collection.
        /// </summary>
        /// <param name="topic">The topic to subscribe to (e.g., "collectionName.records").</param>
        /// <param name="handler">A callback invoked for each event received.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if subscription was successful; otherwise false.</returns>
        Task<Result<bool>> SubscribeAsync(string topic, Action<RealtimeEvent> handler, CancellationToken cancellationToken = default);

        #endregion
    }

}
