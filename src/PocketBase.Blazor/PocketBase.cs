using System.Net.Http;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Clients.Admin;
using PocketBase.Blazor.Clients.Files;
using PocketBase.Blazor.Clients.Health;
using PocketBase.Blazor.Clients.Realtime;
using PocketBase.Blazor.Clients.Settings;
using PocketBase.Blazor.Clients.Collections;

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
