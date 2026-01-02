using PocketBase.Blazor.Clients.Admin;
using PocketBase.Blazor.Clients.Backup;
using PocketBase.Blazor.Clients.Batch;
using PocketBase.Blazor.Clients.Collections;
using PocketBase.Blazor.Clients.CronJob;
using PocketBase.Blazor.Clients.Files;
using PocketBase.Blazor.Clients.Health;
using PocketBase.Blazor.Clients.Logging;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Clients.Record;
using PocketBase.Blazor.Clients.Settings;

namespace PocketBase.Blazor;

public interface IPocketBase
{
    string BaseUrl { get; }

    // Record client for a specific collection
    IRecordClient Collection(string collectionName);

    // Services
    public IAdminsClient Admins { get; }
    public IBackupClient Backup { get; }
    public IBatchClient Batch { get; }
    public ICollectionClient Collections { get; }
    public ICronJobClient CronJob { get; }
    public IFilesClient Files { get; }
    public IHealthClient Health { get; }
    public ILogClient Log { get; }
    public IRealtimeClient Realtime { get; }
    public ISettingsClient Settings { get; }

    // behavior
    void EnableAutoCancellation(bool enabled);
}

