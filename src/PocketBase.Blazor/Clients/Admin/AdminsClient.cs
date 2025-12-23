using System;
using System.Threading;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Clients.Admin
{
    public class AdminsClient : IAdminsClient
    {
        readonly IHttpTransport _http;

        public AdminsClient(IHttpTransport http)
        {
            _http = http;
        }

        public Task<AuthResponse> AuthWithPasswordAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResponse> RefreshAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
