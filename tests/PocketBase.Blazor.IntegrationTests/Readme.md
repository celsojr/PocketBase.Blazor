# PocketBase.Blazor Integration Tests (No Docker)

This document describes how to set up and run the **PocketBase.Blazor integration tests** locally **without Docker or docker-compose**.  
The goal is to keep the environment **simple, explicit, and debuggable**, relying only on native executables.

---

## Prerequisites

Ensure the following tools are installed and available on your PATH:

- **.NET SDK** (matching the solution target, e.g. .NET 8)
- **PocketBase** executable
- **Caddy** web server
- **SQLite** (optional, for inspection/debugging)

---

## Folder Structure (Relevant Parts)

```
PocketBase.Blazor/
├── tests/
│   └── PocketBase.Blazor.IntegrationTests/
│       ├── Data/
│       │   ├── pb_data/
│       │   └── pb_migrations/
│       ├── ReverseProxy/
│       │   └── Caddyfile
│       ├── Fixtures/
│       │   └── PocketBaseTestFixture.cs
│       ├── Auth/
│       ├── Collections/
│       └── PocketBase.Blazor.IntegrationTests.csproj
```

---

## Step 1: Start PocketBase in Dev Mode

From the repository root (or wherever `pocketbase.exe` is located):

```
.\pocketbase.exe --dev serve `
  --dir "PocketBase.Blazor\tests\PocketBase.Blazor.IntegrationTests\Data\pb_data\" `
  --migrationsDir "PocketBase.Blazor\tests\PocketBase.Blazor.IntegrationTests\Data\pb_migrations\"

./pocketbase --dev serve \
  --dir "../PocketBase.Blazor/tests/PocketBase.Blazor.IntegrationTests/Data/pb_data" \
  --migrationsDir "../PocketBase.Blazor/tests/PocketBase.Blazor.IntegrationTests/Data/pb_migrations"
```

Expected behavior:

- PocketBase starts on **http://127.0.0.1:8090**
- Uses test-specific data and migrations
- Safe to reset/delete between test runs

---

## Step 2: Start the Reverse Proxy (Caddy)

The integration tests expect PocketBase to be accessible via **port 8092**.

Navigate to the reverse proxy folder:

```
cd "PocketBase.Blazor\tests\PocketBase.Blazor.IntegrationTests\ReverseProxy"
```

Run Caddy:

```
caddy run --config .\Caddyfile
```

Expected behavior:

- Caddy listens on **http://127.0.0.1:8092**
- Proxies all traffic to **http://127.0.0.1:8090**
- Keeps test URLs stable and isolated

---

## Step 3: Verify Services

Open the following URLs in a browser or via curl:

PocketBase API (direct):
```
curl -i http://127.0.0.1:8090/api/health
```

PocketBase API (via proxy):
```
curl -i http://127.0.0.1:8092/api/health
```

Both should return a healthy status.

---

## Test Credentials

The test database already contains seeded users.

### Superuser

```
email: admin_tester@email.com
password: gDxTCmnq7K8xyKn
```

### Regular User

```
email: user_tester@email.com
password: Nyp9wiGaAC4qGWz
```

These credentials are used by the integration tests for authentication scenarios.

### Curl Example:
```powershell
curl -X POST "http://127.0.0.1:8092/api/collections/_superusers/auth-with-password" `
	-H "Content-Type: application/json" `
	-d '{ "identity": "", "password": "" }'

curl "http://127.0.0.1:8092/api/collections/_superusers" `
	-H "Content-Type: application/json" `
	-H "Authorization: {token}"
```
---

## Step 4: Run Integration Tests

From the integration tests folder:

```
cd PocketBase.Blazor\tests\PocketBase.Blazor.IntegrationTests

# To run tests that must match all:
dotnet test --filter "Category=Integration&Requires!=SMTP&Requires!=Playwright"
```

From the unit tests folder:

```
cd PocketBase.Blazor\tests\PocketBase.Blazor.UnitTests

# To run tests that match any:
dotnet test --filter "Category=Unit|Requires!=Pocketbase"
```

Notes:

- Tests are **real integration tests** (not mocks)
- They assume PocketBase and Caddy are running
- Tests will interact with the local test database
- It is safe to reset/delete `pb_data` between runs to start fresh

---

## Additional Notes

- **No Docker required** – all dependencies are native
- **Migrations**: Ensure `pb_migrations` contains the correct schema for your tests
- **Caddy**: Used to simulate production-like environment and isolate the test server from the default port
- **Cleanup**: Simply stop PocketBase and Caddy; delete `pb_data` contents if needed
