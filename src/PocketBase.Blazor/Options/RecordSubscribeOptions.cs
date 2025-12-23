namespace PocketBase.Blazor.Options
{
    public class RecordSubscribeOptions : SendOptions
    {
        public string? Fields { get; set; }
        public string? Filter { get; set; }
        public string? Expand { get; set; }
    }
}
