namespace PocketBase.Blazor.Options
{
    public class ListOptions : CommonOptions
    {
        public int? Page { get; set; }
        public int? PerPage { get; set; }
        public string? Sort { get; set; }
        public string? Filter { get; set; }
        public bool SkipTotal { get; set; }
    }
}
