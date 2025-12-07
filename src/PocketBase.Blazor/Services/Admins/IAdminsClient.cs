using System.Threading.Tasks;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Services.Admins
{
    public interface IAdminsClient
    {
        Task<AuthResult> AuthWithPasswordAsync(string email, string password);
        Task<AuthResult> RefreshAsync();
        Task LogoutAsync();
    }
}
