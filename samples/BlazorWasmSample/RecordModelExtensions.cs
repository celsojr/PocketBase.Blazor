using System.Text.Json;
using PocketBase.Blazor.Models;

namespace BlazorWasmSample
{
    public static class RecordModelExtensions
    {
        public static T ToModel<T>(this RecordModel record)
        {
            var json = JsonSerializer.Serialize(record.Data);
            return JsonSerializer.Deserialize<T>(json)!;
        }
    }
}
