using System.Threading.Tasks;

namespace PocketBase.Blazor.Clients.Health
{
    public interface IHealthClient
    {
        Task<bool> CheckAsync();
    }
}
