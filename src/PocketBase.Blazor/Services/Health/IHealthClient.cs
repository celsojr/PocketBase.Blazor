using System.Threading.Tasks;

namespace PocketBase.Blazor.Services.Health
{
    public interface IHealthClient
    {
        Task<bool> CheckAsync();
    }
}
