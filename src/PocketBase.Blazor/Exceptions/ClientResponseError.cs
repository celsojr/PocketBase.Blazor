using System;

namespace PocketBase.Blazor.Exceptions
{
    public class ClientResponseError : Exception
    {
        public ClientResponseError(string message) : base(message) { }
    }
}
