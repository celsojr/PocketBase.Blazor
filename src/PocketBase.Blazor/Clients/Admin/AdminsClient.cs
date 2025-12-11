using System;
using System.Threading.Tasks;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Models;

namespace PocketBase.Blazor.Clients.Admin
{
    public class AdminsClient : IAdminsClient
    {
        readonly IHttpTransport _http;

        public AdminsClient(IHttpTransport http)
        {
            _http = http;
        }

        public Task<AuthResult> AuthWithPasswordAsync(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }

        public Task<AuthResult> RefreshAsync()
        {
            throw new NotImplementedException();
        }
    }
}
