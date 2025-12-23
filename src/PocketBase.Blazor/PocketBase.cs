using System.Net.Http;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Clients.Admin;
using PocketBase.Blazor.Clients.Files;
using PocketBase.Blazor.Clients.Health;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Clients.Settings;
using PocketBase.Blazor.Clients.Collections;
using PocketBase.Blazor.Clients.Backup;
using PocketBase.Blazor.Clients.Batch;
using PocketBase.Blazor.Clients.CronJob;
using PocketBase.Blazor.Clients.Logging;
using PocketBase.Blazor.Clients.Record;

namespace PocketBase.Blazor;

/// <inheritdoc />
public class PocketBase : IPocketBase
{
    public string BaseUrl { get; }

    public IAdminsClient Admins { get; }
    public IBackupClient Backup { get; }
    public IBatchClient Batch { get; }
    public ICronJobClient CronJob { get; }
    public IFilesClient Files { get; }
    public IHealthClient Health { get; }
    public ILogClient Log { get; }
    public IRealtimeClient Realtime { get; }
    public IRecordClient Record { get; }
    public ISettingsClient Settings { get; }

    readonly IHttpTransport _http;

    /// <inheritdoc />
    public PocketBase(string baseUrl, HttpClient? client = null)
    {
        BaseUrl = baseUrl.TrimEnd('/');
        _http = new HttpTransport(BaseUrl, client);

        Admins = new AdminsClient(_http);
        Backup = new BackupClient(_http);
        Batch = new BatchClient(_http);
        CronJob = new CronJobClient(_http);
        Log = new LogClient(_http);
        Files = new FilesClient(_http);
        Realtime = new RealtimeClient(_http);
        Record = new RecordClient(_http);
        Health = new HealthClient(_http);
        Settings = new SettingsClient(_http);
    }

    /// <inheritdoc />
    public ICollectionClient Collection(string name)
        => new CollectionClient(name, _http);

    /// <inheritdoc />
    public void EnableAutoCancellation(bool enabled)
    {
    }
}

