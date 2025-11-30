using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWasmSample;
using PocketBase.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure PocketBase with example options
builder.Services.AddPocketBase(options =>
{
    options.BaseUrl = "https://pocketbase.io";
    // options.ApiKey = "pb_pk_xxx";
    // You can customize JSON options here if needed
});

await builder.Build().RunAsync();
