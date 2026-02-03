using System;
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

namespace PocketBase.Blazor
{
    /// <summary>
    /// PocketBase client for interacting with PocketBase backend.
    /// IMPORTANT: This class implements IAsyncDisposable. When manually instantiating,
    /// always use 'await using' statement or call DisposeAsync() explicitly.
    /// When using with Dependency Injection, the container manages disposal automatically.
    /// </summary>
    public interface IPocketBase : IAsyncDisposable
    {
        string BaseUrl { get; }

        IRecordClient Collection(string collectionName);

        // Services
        IAdminsClient Admins { get; }
        IBackupClient Backup { get; }
        IBatchClient Batch { get; }
        ICollectionClient Collections { get; }
        ICronsClient Crons { get; }
        IFilesClient Files { get; }
        IHealthClient Health { get; }
        ILogClient Log { get; }
        IRealtimeClient Realtime { get; }
        IRealtimeStreamClient RealtimeSse { get; }
        ISettingsClient Settings { get; }

        IBatchClient CreateBatch();

        void EnableAutoCancellation(bool enabled);
    }
}
