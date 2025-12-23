namespace PocketBase.Blazor.Requests
{
    public class QueryOptionsRequest
    {
        public string? Filter { get; set; }
        public string? Sort { get; set; }
        public string? Expand { get; set; }
        public int? Page { get; set; }
        public int? PerPage { get; set; }
    }
}
