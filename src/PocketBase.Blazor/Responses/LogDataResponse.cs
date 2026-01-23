using System.Text.Json.Serialization;

namespace PocketBase.Blazor.Responses
{
    public sealed class LogDataResponse
    {
        public string? Auth { get; init; }
        public string? AuthId { get; init; }
        public double ExecTime { get; init; }
        public string? Method { get; init; }
        public string? Referer { get; init; }
        [JsonPropertyName("remoteIP")] public string? RemoteIP { get; init; }
        public int Status { get; init; }
        public string? Type { get; init; }
        public string? Url { get; init; }
        public string? UserAgent { get; init; }
        [JsonPropertyName("userIP")] public string? UserIP { get; init; }
        public string? Error { get; init; }
        public string? Details { get; init; }
    }
}

