using System;
using System.Net;

namespace PocketBase.Blazor.Exceptions
{
    public class PocketBaseException : Exception
    {
        public int Status { get; }
        public string Raw { get; }

        public PocketBaseException(HttpStatusCode status, string raw)
            : base(raw)
        {
            Status = (int)status;
            Raw = raw;
        }
    }
}
