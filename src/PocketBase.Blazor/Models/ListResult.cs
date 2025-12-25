using System.Collections.Generic;

namespace PocketBase.Blazor.Models
{
    public class ListResult<T>
    {
        public int Page { get; set; }
        public int PerPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public List<T> Items { get; set; } = [];
    }
}
