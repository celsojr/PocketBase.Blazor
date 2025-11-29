using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BlazorWasmSample;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        // Configure PocketBase with example options
        builder.Services.AddPocketBase(options =>
        {
            options.BaseUrl = "https://demo.pocketbase.io";
            // options.ApiKey = "pb_pk_xxx";
            // You can customize JSON options here if needed
        });

        await builder.Build().RunAsync();
    }
}
