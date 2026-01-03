using System;
using PocketBase.Blazor.Clients.Admin;
using PocketBase.Blazor.Responses;

namespace PocketBase.Blazor.Store
{
    public class AuthStore
    {
        public string? Token { get; private set; }
        public UserResponse? User { get; private set; }

        private readonly IAdminsClient _admins;
        private AuthResponse? _currentSession;

        public AuthResponse? CurrentSession => _currentSession;

        public AuthStore(IAdminsClient adminsClient)
        {
            _admins = adminsClient ?? throw new ArgumentNullException(nameof(adminsClient));
        }

        public void Save(AuthResponse auth)
        {
            Token = auth.Token;
            User = auth.Record;
        }

        public void Clear()
        {
            Token = null;
            User = null;
        }
    }
}

