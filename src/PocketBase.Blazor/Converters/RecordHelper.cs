using System;
using System.Collections.Generic;
using System.Text.Json;
using PocketBase.Blazor.Events;

namespace PocketBase.Blazor.Converters
{
    public static class RecordHelper
    {
        public static RealtimeRecordEvent? ParseRecordEvent(RealtimeEvent evt)
        {
            if (!evt.Data.Contains("\"record\":")) return null;

            try
            {
                var json = JsonDocument.Parse(evt.Data);
                var root = json.RootElement;

                return new RealtimeRecordEvent
                {
                    Action = root.GetProperty("action").GetString()!,
                    Collection = root.GetProperty("record").GetProperty("collectionName").GetString()!,
                    RecordId = root.GetProperty("record").GetProperty("id").GetString(),
                    Record = JsonSerializer.Deserialize<Dictionary<string, object?>>(root.GetProperty("record").GetRawText())!
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
