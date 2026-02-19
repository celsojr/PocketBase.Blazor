using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PocketBase.Blazor.Clients.Admin;
using PocketBase.Blazor.Clients.Backup;
using PocketBase.Blazor.Clients.Batch;
using PocketBase.Blazor.Clients.Collections;
using PocketBase.Blazor.Clients.Crons;
using PocketBase.Blazor.Clients.Files;
using PocketBase.Blazor.Clients.Health;
using PocketBase.Blazor.Clients.Logging;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Clients.Record;
using PocketBase.Blazor.Clients.Settings;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Store;

namespace PocketBase.Blazor;

/// <inheritdoc />
public sealed class PocketBase : IPocketBase
{
    private readonly Dictionary<string, RecordClient> _recordClients = [];

    public string BaseUrl { get; }

    public IAdminsClient Admins { get; }
    public IBackupClient Backup { get; }
    public IBatchClient Batch { get; }
    public ICollectionClient Collections { get; }
    public ICronsClient Crons { get; }
    public IFilesClient Files { get; }
    public IHealthClient Health { get; }
    public ILogClient Log { get; }
    public IRealtimeClient Realtime { get; }
    public IRealtimeStreamClient RealtimeSse { get; }
    public ISettingsClient Settings { get; }
    public PocketBaseStore AuthStore { get; }

    readonly IHttpTransport _http;

    /// <inheritdoc />
    public PocketBase(string baseUrl, HttpClient? client = null, PocketBaseOptions? options = null)
    {
        BaseUrl = baseUrl.TrimEnd('/');
        _http = new HttpTransport(BaseUrl, client, options);

        Admins = new AdminsClient(_http);
        Backup = new BackupClient(_http);
        Batch = new BatchClient(_http);
        Collections = new CollectionClient(_http);
        Crons = new CronsClient(_http);
        Log = new LogClient(_http);
        Files = new FilesClient(_http);
        Realtime = new RealtimeClient(_http);
        RealtimeSse = new RealtimeSseClient(_http);
        Health = new HealthClient(_http);
        Settings = new SettingsClient(_http);

        var authStore = new AuthStore();
        AuthStore = new PocketBaseStore(authStore, Realtime, RealtimeSse);

        if (_http is HttpTransport transport)
        {
            transport.SetStore(AuthStore);
            Admins.SetStore(AuthStore);
        }
    }

    /// <inheritdoc />
    public IRecordClient Collection(string collectionName)
    {
        var encodedName = WebUtility.UrlEncode(collectionName);
        if (_recordClients.TryGetValue(encodedName, out var value))
        {
            return value;
        }
        var newRecordClient = new RecordClient(encodedName, _http, AuthStore);
        _recordClients[encodedName] = newRecordClient;
        return newRecordClient;
    }

    /// <inheritdoc />
    public IBatchClient CreateBatch()
    {
        // We return a new instance every time to ensure a fresh batch
        return new BatchClient(_http);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Realtime is IAsyncDisposable rtDisposable)
            await rtDisposable.DisposeAsync();
        
        if (RealtimeSse is IAsyncDisposable sseDisposable)
            await sseDisposable.DisposeAsync();

        _http.Dispose();
    }
}
