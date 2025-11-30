# PocketBase.Blazor

**PocketBase.Blazor** is a lightweight, Blazor-friendly client wrapper for the [PocketBase REST API](https://pocketbase.io/). It simplifies working with PocketBase in both **Blazor WebAssembly** and **Blazor Server** projects by providing a minimal, strongly-typed API with built-in DI support. 

## Features

- Minimalistic `IPocketBaseClient` + `PocketBaseClient` implementation using `HttpClient`.  
- Dependency Injection (DI) extension: `AddPocketBase(options => { ... })`.  
- `PocketBaseOptions` with pre-configured `JsonSerializerOptions` tailored for Blazor.  
- Strongly-typed API with automatic error handling for API responses.  
- Sample Blazor WebAssembly application to get you started quickly.  
- Unit test project skeleton using **XUnit** and **Moq** for HTTP client mocking.  
- GitHub Actions workflow for automated NuGet package publishing.  
- Local build and pack scripts for easy packaging and distribution.

## Quickstart

Install from NuGet:

```bash
dotnet add package PocketBase.Blazor
```

Register services in `Program.cs`:

```csharp
builder.Services.AddPocketBase(options =>
{
    options.BaseUrl = "https://your-pocketbase.example";
    options.ApiKey = "pb_pk_xxx"; // optional

    // Optional: tune JSON options (defaults are camelCase and ignore nulls)
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
```
Inject and use the client in your components:

```
@inject IPocketBaseClient Client

@code {
    private List<Post>? posts;

    protected override async Task OnInitializedAsync()
    {
        var (items, error) = await Client.GetListAsync<Post>("posts");
        if (error != null)
        {
            // Handle error (e.g., log or show a message)
        }
        else
        {
            posts = items;
        }
    }

    public class Post
    {
        public string Id { get; set; } = "";
        public string? Title { get; set; }
        public string? Slug { get; set; }
    }
}

```

## Running Unit Tests

PocketBase.Blazor includes a test project using **XUnit** and **Moq**. To run the tests:
```
dotnet test ./tests/PocketBase.Blazor.Tests/PocketBase.Blazor.Tests.csproj

```

## Deployment
Build & pack locally
```
./build-local.ps1

# macOs/Linux

dotnet clean ./src/PocketBase.Blazor/PocketBase.Blazor.csproj
dotnet build ./src/PocketBase.Blazor/PocketBase.Blazor.csproj -c Release
dotnet pack ./src/PocketBase.Blazor/PocketBase.Blazor.csproj -c Release -o ./nupkg

```

## CI / Release
A GitHub Actions workflow is included to pack and publish to NuGet when a tag `v*` is pushed and `NUGET_API_KEY` is present in repository secrets.

## Roadmap

- Auth helpers (login/logout, token refresh)
- Realtime support (if PocketBase offers a WASM-friendly realtime API)
- Aspire integration hooks
- More complete, generated models

## Contributing

Contributions and issues are welcome. Please follow standard GitHub PR workflow.

## License

This work is licensed under the **Creative Commons Attribution 4.0 International License (CC BY 4.0)**.  

You are free to:

- **Share** — copy and redistribute the material in any medium or format  
- **Adapt** — remix, transform, and build upon the material for any purpose, including commercial  

**Under the following terms:**

- **Attribution** — You must give appropriate credit, provide a link to the license, and indicate if changes were made. You may do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use.

**To view a copy of this license:** [https://creativecommons.org/licenses/by/4.0/](https://creativecommons.org/licenses/by/4.0/)
