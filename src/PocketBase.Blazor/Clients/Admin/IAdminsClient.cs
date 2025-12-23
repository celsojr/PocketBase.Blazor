using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Admin
{
    public interface IAdminsClient
    {
        Task<AuthResponse> AuthWithPasswordAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<AuthResponse> RefreshAsync(CancellationToken cancellationToken = default);
        Task LogoutAsync(CancellationToken cancellationToken = default);
    }
}
