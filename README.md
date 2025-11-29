# PocketBase.Blazor

PocketBase.Blazor is a small, Blazor-friendly client wrapper for the [PocketBase REST API](https://pocketbase.io/).  

## Features

- Minimal `IPocketBaseClient` + `PocketBaseClient` implementation using `HttpClient`.
- DI convenience extension: `AddPocketBase(options => { ... })`.
- `PocketBaseOptions` with a `JsonSerializerOptions` pre-configured similar to Blazored.LocalStorage defaults.
- Sample Blazor WASM app.
- Unit test project skeleton.
- GitHub Actions workflow for publishing a NuGet package on tag push.
- Local build/pack script.

## Quickstart

Install from NuGet (when published):

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

## Deployment
Build & pack locally
```
./build-local.ps1

# macOs/Linux

dotnet clean ./src/PocketBase.Blazor
dotnet build ./src/PocketBase.Blazor -c Release
dotnet pack ./src/PocketBase.Blazor -c Release -o ./nupkg

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