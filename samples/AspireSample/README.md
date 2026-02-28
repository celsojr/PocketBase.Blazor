# Aspire Sample

This sample runs three resources:

- `AspireSample.AppHost` (orchestrator)
- `AspireSample.Web` (Blazor Web app)
- `AspireSample.ApiService` (weather API)
- `pocketbase` container (`ghcr.io/muchobien/pocketbase:latest`)

## Prerequisites

- .NET SDK 8.0+
- Aspire workload:
  - `dotnet workload install aspire`
- One container runtime:
  - Docker Desktop (`docker`)
  - Podman standalone (`podman`)

## Run With Aspire AppHost

From `samples/AspireSample`:

```powershell
# Make sure docker or podman is ready, then
dotnet run --project .\AspireSample.AppHost\AspireSample.AppHost.csproj
```

### Force Docker Runtime

```powershell
$env:ASPIRE_CONTAINER_RUNTIME = "docker"
dotnet run --project .\AspireSample.AppHost\AspireSample.AppHost.csproj
```

### Force Podman Runtime (standalone)

```powershell
$env:ASPIRE_CONTAINER_RUNTIME = "podman"
dotnet run --project .\AspireSample.AppHost\AspireSample.AppHost.csproj
```

If Podman is remote/non-default, configure `DOCKER_HOST` for the Podman socket before running AppHost.

## PocketBase Standalone (No AppHost)

If you want to run PocketBase container manually and run the .NET apps separately:

```powershell
cd .\AspireSample.AppHost\
docker run --rm -it `
  --name aspire_sample_pocketbase `
  -p 8090:8090 `
  -v "${PWD}\data\pb_data:/pb_data" `
  -v "${PWD}\data\pb_migrations:/pb_migrations" `
  -v "${PWD}\data\pb_public:/pb_public" `
  -v "${PWD}\data\pb_hooks:/pb_hooks" `
  ghcr.io/muchobien/pocketbase:latest
```

Podman equivalent:

```powershell
cd .\AspireSample.AppHost\
podman run --rm -it `
  --name aspire_sample_pocketbase `
  -p 8090:8090 `
  -v "${PWD}\data\pb_data:/pb_data" `
  -v "${PWD}\data\pb_migrations:/pb_migrations" `
  -v "${PWD}\data\pb_public:/pb_public" `
  -v "${PWD}\data\pb_hooks:/pb_hooks" `
  ghcr.io/muchobien/pocketbase:latest
```

Then run API and Web projects in separate terminals.

## Aspire Runtime Version

- Current sample AppHost SDK: `13.1.2`
- Previous value in this repo: `13.0.2`

Recommendation: keep the sample on the latest `13.1.x` patch unless you have a pinned CI/tooling constraint.
