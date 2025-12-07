using System.Net.Http;
using PocketBase.Blazor.Collections;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Services.Admins;
using PocketBase.Blazor.Services.Files;
using PocketBase.Blazor.Services.Health;
using PocketBase.Blazor.Services.Realtime;
using PocketBase.Blazor.Services.Settings;

namespace PocketBase.Blazor;

public class PocketBase : IPocketBase
{
    public string BaseUrl { get; }

    public IAdminsClient Admins { get; }
    public IFilesClient Files { get; }
    public IRealtimeClient Realtime { get; }
    public IHealthClient Health { get; }
    public ISettingsClient Settings { get; }

    readonly IHttpTransport _http;

    public PocketBase(string baseUrl, HttpClient? client = null)
    {
        BaseUrl = baseUrl.TrimEnd('/');
        _http = new HttpTransport(BaseUrl, client);

        Admins = new AdminsClient(_http);
        Files = new FilesClient(_http);
        Realtime = new RealtimeClient();
        Health = new HealthClient(_http);
        Settings = new SettingsClient(_http);
    }

    public ICollectionClient Collection(string name)
        => new CollectionClient(name, _http);

    public void EnableAutoCancellation(bool enabled)
    {
    }
}
