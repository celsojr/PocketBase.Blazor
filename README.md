# PocketBase.Blazor [![NuGet Version](https://img.shields.io/nuget/v/PocketBase.Blazor.svg)](https://www.nuget.org/packages/PocketBase.Blazor) [![NuGet Downloads](https://img.shields.io/nuget/dt/PocketBase.Blazor.svg)](https://www.nuget.org/packages/PocketBase.Blazor/) ![GitHub Packages](https://img.shields.io/badge/GitHub%20Packages-0.0.3-blue)


**PocketBase.Blazor** is a lightweight, Blazor-friendly client wrapper for the [PocketBase REST API](https://pocketbase.io/). It simplifies working with PocketBase in both **Blazor WebAssembly** and **Blazor Server** projects by providing a minimal, strongly-typed API with built-in DI support. 

> [!WARNING]  
> **Disclaimer:**  
> This repository is an experimental work in progress. It is primarily used in another project and is maintained only for personal use or on-demand updates. The goal is to keep it simple while providing tooling that might be helpful to others. _Not all **PocketBase** functionalities are covered yet_.  
> 
> Feel free to use, fork, or modify this project as needed. Pull requests are welcome, and you may contact me with any questions.  
> 
> **Important:** This project is not an official Microsoft repository and is not endorsed, sponsored, or affiliated with Microsoft or any of its subsidiaries. Additionally, this is not an official PocketBase SDK; the official PocketBase SDKs are available in JavaScript and Dart and you can find them on their repository.

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
dotnet add package PocketBase.Blazor --version 0.0.3
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
# Windows
./build-local.ps1

# macOs/Linux
chmod +x build.sh
./build.sh
```

## CI / Release
A GitHub Actions workflow is included to pack and publish to NuGet when a tag `v*` is pushed and `NUGET_API_KEY` is present in repository secrets.

## Running the Aspire Sample Application

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download) (as specified in global.json)
- [Docker](https://www.docker.com/get-started) (or [Podman](https://podman.io/getting-started/installation)) installed and running
- Optional: [Aspire CLI](https://aspire.dev/get-started/install-cli/) if you want to use command-line tooling
> **Note**: The sample uses a containerized PocketBase instance, so Docker (or Podman) must be running.

### 2. Setting Up the Aspire Environment
To run the sample application, follow these steps:

1. Ensure you have the .NET SDK installed.

1. Open a terminal and navigate to the `samples/AspireSample` directory.

1. Run the following command to start the application:

   ```
   dotnet run --project AspireSample.AppHost
   ```

1. Check the terminal output for the super user creation URL. Replace the internal container address and port with the one exposed on your host machine by the Aspire dashboard.

### 3. Access PocketBase in the container
To run PocketBase commands inside the container, use the following command with Docker:
```
docker (or podman) exec aspire_sample_pocketbase /usr/local/bin/pocketbase --version
```

or, you can execute an interactive shell session. Best for interactive commands when you have to answer questions:
```
docker (or podman) exec -it aspire_sample_pocketbase /bin/sh
pocketbase --version
```

## Roadmap

- **Authentication helpers**  
  Streamlined utilities for login/logout, session handling, and automatic token refresh.

- **Realtime support**  
  Investigate realtime capabilities using the new Server-Sent Events (SSE) features introduced in ASP.NET Core 10.  
  This includes leveraging `IAsyncEnumerable<T>`, `SseItem<T>`, and `TypedResults.ServerSentEvents` to enable efficient, one-way realtime updates without requiring external dependencies.

- **Aspire integration**  
  Provide extensions or hooks to integrate PocketBase.Blazor into .NET Aspire applications.

- **Expanded model generation**  
  Generate more complete, strongly-typed models based on PocketBase schemas.

- **Possible full SDK implementation**  
  Explore evolving this project into a more complete PocketBase SDK for .NET/Blazor, depending on demand and community interest.

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
