# PocketBase.Blazor Integration Tests

This guide is for running **integration tests only** in `tests/PocketBase.Blazor.IntegrationTests`.

## Scope

- Real PocketBase process
- Real HTTP calls
- Real seeded test data/migrations
- No Docker requirement

## Prerequisites

- .NET 8 SDK
- PocketBase executable on PATH (or local binary path)
- Caddy on PATH
- Optional: SQLite CLI for local DB inspection

## Test Data Layout

- Data dir: `tests/PocketBase.Blazor.IntegrationTests/Data/pb_data`
- Migrations: `tests/PocketBase.Blazor.IntegrationTests/Data/pb_migrations`
- Reverse proxy config: `tests/PocketBase.Blazor.IntegrationTests/ReverseProxy/Caddyfile`

## Start PocketBase (dev mode)

Windows PowerShell:

```powershell
.\pocketbase.exe --dev serve `
  --dir "tests\PocketBase.Blazor.IntegrationTests\Data\pb_data" `
  --migrationsDir "tests\PocketBase.Blazor.IntegrationTests\Data\pb_migrations"
```

macOS/Linux:

```bash
./pocketbase --dev serve \
  --dir "tests/PocketBase.Blazor.IntegrationTests/Data/pb_data" \
  --migrationsDir "tests/PocketBase.Blazor.IntegrationTests/Data/pb_migrations"
```

Expected direct URL: `http://127.0.0.1:8090`

## Start Caddy reverse proxy

```powershell
cd tests/PocketBase.Blazor.IntegrationTests/ReverseProxy
caddy run --config ./Caddyfile
```

Expected proxied URL: `http://127.0.0.1:8092`

## Verify services

```bash
curl -i http://127.0.0.1:8090/api/health
curl -i http://127.0.0.1:8092/api/health
```

## Run Integration Tests

From repository root:

```bash
dotnet test ./tests/PocketBase.Blazor.IntegrationTests/PocketBase.Blazor.IntegrationTests.csproj --filter "Category=Integration&Requires!=SMTP&Requires!=Playwright"
```

Lower-noise subset (skip filesystem-dependent integration tests too):

```bash
dotnet test ./tests/PocketBase.Blazor.IntegrationTests/PocketBase.Blazor.IntegrationTests.csproj --filter "Category=Integration&Requires!=SMTP&Requires!=Playwright&Requires!=FileSystem"
```

## Traits Used

Common trait filters in this project:

- `Category=Integration`
- `Requires=SMTP`
- `Requires=Playwright`
- `Requires=FileSystem`
- `Requires=GoRuntime`

## Notes

- Tests mutate test data; do not point to production data.
- If tests fail after schema changes, reset/reseed `pb_data` and re-run migrations.
- Keep integration examples in sync with README snippets.
