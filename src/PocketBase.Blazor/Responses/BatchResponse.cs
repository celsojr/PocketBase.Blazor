using System.Collections.Generic;

namespace PocketBase.Blazor.Responses
{
    public class BatchResponse
    {
        public int Status { get; set; }
        public Dictionary<string, object>? Body { get; set; }
    }
}

