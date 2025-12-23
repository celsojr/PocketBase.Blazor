using System;
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

        public Task<AuthResponse> AuthWithPasswordAsync(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }

        public Task<AuthResponse> RefreshAsync()
        {
            throw new NotImplementedException();
        }
    }
}
