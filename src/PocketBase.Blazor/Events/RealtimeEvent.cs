namespace PocketBase.Blazor.Events
{
    public class RealtimeEvent
    {
        public string? Topic { get; set; }
        public string? Action { get; set; }
        public string? Data { get; set; }
    }
}
