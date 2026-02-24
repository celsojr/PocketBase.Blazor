# PocketBase.Blazor (Beta) [![NuGet Version](https://img.shields.io/nuget/v/PocketBase.Blazor.svg)](https://www.nuget.org/packages/PocketBase.Blazor/0.1.1-beta.2) [![NuGet Downloads](https://img.shields.io/nuget/dt/PocketBase.Blazor.svg)](https://www.nuget.org/packages/PocketBase.Blazor/0.1.1-beta.2) ![GitHub Packages](https://img.shields.io/badge/GitHub%20Packages-0.1.1-blue)

PocketBase.Blazor is a PocketBase .NET REST client wrapper (with some extra powers) for Blazor and .NET apps.

## Why Beta

[PocketBase](https://pocketbase.io/) itself is still evolving toward a final stable v1 API. `PocketBase.Blazor` is also evolving slightly and is currently in beta for the same reason.

## JS-SDK Parity (No JS Interop)

This client is intentionally close to PocketBase JS-SDK semantics and method naming, but implemented in pure .NET (no JavaScript interop required).

- If you already know the JS-SDK mental model, most REST APIs should feel familiar.
- Result handling is .NET-native (`FluentResults`) with explicit `IsSuccess`, `Value`, and `Errors`.

## Features

- Typed domain clients (`Admins`, `Collections`, `Record`, `Files`, `Settings`, `Batch`, `Backup`, `Crons`, `Realtime`)
- DI setup with `AddPocketBase(...)`
- Auth/session store support (`AuthStore`)
- Realtime callback API + SSE stream API
- Server-hosting utilities (auto PocketBase binary resolve/download when applicable)
- Cron server generator and optional Go binary build flow

## Install

```bash
dotnet add package PocketBase.Blazor --version 0.1.1-beta.2
```

## Architecture Notes

- High-level philosophy: `design-notes.md`
- Contributor guardrails/checklist: `contributing-architecture.md`

## Quick Start (Blazor)

```csharp
using PocketBase.Blazor;
using PocketBase.Blazor.Extensions;

builder.Services.AddPocketBase(options =>
{
    options.BaseUrl = "http://127.0.0.1:8090";
    // options.ApiKey = "pbp_xxx"; // optional
});
```

## Usage Examples (From Integration Tests)

Source-of-truth examples live under `tests/PocketBase.Blazor.IntegrationTests/Clients`.

### Authenticate with password

Source: `tests/PocketBase.Blazor.IntegrationTests/Clients/Record/AuthWithPasswordRecordTests.cs`

```csharp
var authResult = await pb.Collection("users")
    .AuthWithPasswordAsync(email, password);

if (!authResult.IsSuccess)
{
    // inspect authResult.Errors
}
```

### Create + list records with typed models

Source: `tests/PocketBase.Blazor.IntegrationTests/Clients/Record/CreateRecordTests.cs` and `tests/PocketBase.Blazor.IntegrationTests/Clients/Record/ListRecordsTests.cs`

```csharp
await pb.Collection("posts")
    .CreateAsync<PostResponse>(new PostCreateRequest
    {
        Title = "Eleven Post",
        Slug = "eleven-post",
        IsPublished = true
    });

var list = await pb.Collection("posts")
    .GetListAsync<PostResponse>(
        page: 1,
        perPage: 10,
        options: new ListOptions
        {
            Expand = "category",
            Sort = "-created",
            SkipTotal = true
        });
```

### Batch processing (more advanced than simple CRUD)

Source: `tests/PocketBase.Blazor.IntegrationTests/Clients/Batch/CreateTests.cs`

```csharp
var batch = pb.CreateBatch();

batch.Collection("posts").Create(new { title = "First" });
batch.Collection("posts").Create(new { title = "Second" });

var result = await batch.SendAsync();

if (result.IsSuccess)
{
    // result.Value contains per-operation status/data
}
```

### Realtime subscription

Source: `tests/PocketBase.Blazor.IntegrationTests/Clients/Realtime/SubscribeTests.cs`

```csharp
using (await pb.Collection("categories").SubscribeAsync("*", evt =>
{
    Console.WriteLine($"{evt.Action}: {evt.RecordId}");
}))
{
    await pb.Collection("categories").CreateAsync<RecordResponse>(new
    {
        name = "Test Category",
        slug = "test-category"
    });
}
```

## Hosting, Auto-Binary Download, and Custom Cron Build

This is server-side only (not WASM).

### Auto-resolve PocketBase executable

`PocketBaseHostBuilder` tries to resolve a local executable; if missing, it downloads PocketBase automatically for the current platform/arch.

```csharp
var host = await PocketBaseHostBuilder.CreateDefault()
    .UseOptions(o =>
    {
        o.Host = "127.0.0.1";
        o.Port = 8090;
        o.Dir = "./pb_data";
        o.Dev = true;
    })
    .BuildAsync();
```

### Use your own executable path

```csharp
var host = await PocketBaseHostBuilder.CreateDefault()
    .UseExecutable(@"C:\tools\pocketbase.exe")
    .UseOptions(o => o.Port = 8090)
    .BuildAsync();
```

### Generate custom cron handlers and build a custom binary

Source references: `src/PocketBase.Blazor/Clients/Crons/CronGenerator.cs`, `src/PocketBase.Blazor/Options/CronGenerationOptions.cs`

```csharp
var generator = new CronGenerator();

var host = await PocketBaseHostBuilder.CreateDefault()
    .UseCrons(
        generator,
        new CronManifest
        {
            Crons =
            [
                new CronDefinition
                {
                    Id = "hello",
                    Description = "Hello cron",
                    HandlerBody = "log.Println(\"cron hello executed\", payload)"
                }
            ]
        },
        new CronGenerationOptions
        {
            ProjectDirectory = "cron-server",
            BuildBinary = true
        })
    .BuildAsync();
```

## Samples

You have two separate runnable examples:

- Standalone Blazor WASM sample: `samples/BlazorWasmSample`
- Aspire sample: `samples/AspireSample`

## Usage Enforcement Policy

Public API usage examples must be validated in integration tests.

- If README snippets and tests diverge, tests win.
- Every public API change should include integration test updates in the same PR.

## Run Tests

Unit tests:

```bash
dotnet test ./tests/PocketBase.Blazor.UnitTests/PocketBase.Blazor.UnitTests.csproj
```

Integration tests:

```bash
dotnet test ./tests/PocketBase.Blazor.IntegrationTests/PocketBase.Blazor.IntegrationTests.csproj --filter "Category=Integration&Requires!=SMTP&Requires!=Playwright"
```

Integration setup guide:

- `tests/PocketBase.Blazor.IntegrationTests/Readme.md`

## Release

Release workflow: `.github/workflows/release.yml`

- Trigger: push tag matching `v*`
- Pipeline: restore, build, smoke test, pack, publish

## License

Licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).
