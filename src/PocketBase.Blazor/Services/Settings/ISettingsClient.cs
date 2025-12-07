using System.Text.Json;
using System.Threading.Tasks;

namespace PocketBase.Blazor.Services.Settings
{
    public interface ISettingsClient
    {
        Task<JsonElement> GetAllAsync();
        Task UpdateAsync(object settings);
    }
}
