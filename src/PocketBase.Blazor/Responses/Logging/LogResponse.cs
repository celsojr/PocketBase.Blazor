using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PocketBase.Blazor.Enums;

namespace PocketBase.Blazor.Responses.Logging
{
    public sealed class LogResponse
    {
        public string? Id { get; init; }
        public LogLevel Level { get; init; }
        public string? Message { get; init; }
        public DateTime? Created { get; init; }

        [JsonConverter(typeof(LogDataConverter))]
        public LogDataResponse? Data { get; init; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public class LogDataConverter : JsonConverter<LogDataResponse>
    {
        public override LogDataResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            return new LogDataResponse
            {
                Auth = TryGetString(root, "auth"),
                AuthId = TryGetString(root, "authId"),
                ExecTime = TryGetDouble(root, "execTime"),
                Method = TryGetString(root, "method"),
                Referer = TryGetString(root, "referer"),
                RemoteIP = TryGetString(root, "remoteIP"),
                Status = TryGetInt(root, "status"),
                Type = TryGetString(root, "type"),
                Url = TryGetString(root, "url"),
                UserAgent = TryGetString(root, "userAgent"),
                UserIP = TryGetString(root, "userIP"),
                Error = TryGetString(root, "error"),
                Details = TryGetString(root, "details")
            };
        }

        private static string? TryGetString(JsonElement element, string propertyName)
            => element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;

        private static double TryGetDouble(JsonElement element, string propertyName)
            => element.TryGetProperty(propertyName, out var prop) ? prop.GetDouble() : 0;

        private static int TryGetInt(JsonElement element, string propertyName)
            => element.TryGetProperty(propertyName, out var prop) ? prop.GetInt32() : 0;

        public override void Write(Utf8JsonWriter writer, LogDataResponse value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

