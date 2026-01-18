using PocketBase.Blazor.Hosting.Interfaces;

namespace PocketBase.Blazor.Hosting.Services
{
    public static class PocketBaseClientFactory
    {
        public static PocketBase CreateClient(IPocketBaseHost host)
        {
            return new PocketBase(host.BaseUrl);
        }

        public static PocketBase CreateClient(string baseUrl)
        {
            return new PocketBase(baseUrl);
        }
    }
}
