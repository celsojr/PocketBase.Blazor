using System;
using System.Net;
using System.Text.Json;

namespace PocketBase.Blazor.Exceptions
{
    public class PocketBaseException : Exception
    {
        public int Status { get; }

        /// <summary>
        /// The raw response body exactly as returned by PocketBase.
        /// </summary>
        public string Raw { get; }

        /// <summary>
        /// The deserialized <c>data</c> object of the error response, when present.
        /// Useful for typed access to per-field validation errors.
        /// </summary>
        public JsonElement? Data { get; }

        /// <summary>
        /// The multi-factor authentication session id, when the server requires a second
        /// auth method. PocketBase answers the first auth attempt with
        /// HTTP 401 and a <c>{"mfaId":"..."}</c> body; pass this value to the
        /// second auth method (eg. <c>AuthWithOtpAsync(..., mfaId: mfaId)</c>) to complete the flow.
        /// </summary>
        public string? MfaId { get; }

        public PocketBaseException(HttpStatusCode status, string raw)
            : base(raw)
        {
            Status = (int)status;
            Raw = raw;

            if (string.IsNullOrWhiteSpace(raw))
                return;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(raw);

                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return;

                JsonElement root = doc.RootElement;

                // PocketBase returns the MFA session id at the root of the 401 body,
                // eg. { "mfaId": "zxu3cw7276fwxhw" }
                if (root.TryGetProperty("mfaId", out JsonElement rootMfaId) &&
                    rootMfaId.ValueKind == JsonValueKind.String)
                {
                    MfaId = rootMfaId.GetString();
                }

                if (root.TryGetProperty("data", out JsonElement data) &&
                    data.ValueKind == JsonValueKind.Object)
                {
                    // clone because the underlying JsonDocument is disposed
                    Data = data.Clone();

                    // fall back to the nested location in case the payload shape changes
                    if (MfaId is null &&
                        data.TryGetProperty("mfaId", out JsonElement dataMfaId) &&
                        dataMfaId.ValueKind == JsonValueKind.String)
                    {
                        MfaId = dataMfaId.GetString();
                    }
                }
            }
            catch (JsonException)
            {
                // non-JSON error body (eg. proxy/gateway HTML) - Data and MfaId stay null
            }
        }
    }
}