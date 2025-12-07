using PocketBase.Blazor.Collections;
using PocketBase.Blazor.Services.Admins;
using PocketBase.Blazor.Services.Files;
using PocketBase.Blazor.Services.Health;
using PocketBase.Blazor.Services.Realtime;
using PocketBase.Blazor.Services.Settings;

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
