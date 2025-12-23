using System.Threading.Tasks;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Admin
{
    public interface IAdminsClient
    {
        Task<AuthResponse> AuthWithPasswordAsync(string email, string password);
        Task<AuthResponse> RefreshAsync();
        Task LogoutAsync();
    }
}
