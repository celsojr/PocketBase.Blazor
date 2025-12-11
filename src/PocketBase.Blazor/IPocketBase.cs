using PocketBase.Blazor.Clients.Collections;
using PocketBase.Blazor.Clients.Admin;
using PocketBase.Blazor.Clients.Files;
using PocketBase.Blazor.Clients.Health;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Clients.Settings;

namespace PocketBase.Blazor;

public interface IPocketBase
{
    string BaseUrl { get; }

    // Collections
    ICollectionClient Collection(string name);

    // Services
    IAdminsClient Admins { get; }
    IFilesClient Files { get; }
    IRealtimeClient Realtime { get; }
    IHealthClient Health { get; }
    ISettingsClient Settings { get; }

    // behavior
    void EnableAutoCancellation(bool enabled);
}
