# PocketBase.Blazor Clients Design Notes

## 1. General Design Philosophy

- Treat every PocketBase service as a **client**, not a service.  
- Maintain **JS SDK semantics** for all methods:
  - Return types match JS SDK (`Task<bool>` for create/delete actions, `Task<T>` for fetches).  
  - Method behavior mirrors JS SDK for consistency.
- Use **`IHttpTransport`** for HTTP calls; no custom HttpClient logic per client.  
- Include **interfaces with XML documentation** for every client.  
- Method signatures are **single-line** for readability and consistency.

---

## 2. Backup Client

- Interface: `IBackupClient`  
- Concrete: `BackupClient`  
- Features:
  - CRUD operations for backups (`CreateAsync`, `UploadAsync`, `DeleteAsync`, `RestoreAsync`).  
  - `GetDownloadUrl` helper method.  
  - Returns `bool` where JS SDK expects success confirmation.  
- Uses `MultipartFile` for file uploads.

### MultipartFile

- Represents a file to send via multipart/form-data.  
- Factory methods: `FromBytes`, `FromFile`.  
- Compatible with `_http.SendAsync` calls.

---

## 3. CronJob Client

- Interface: `ICronJobClient`  
- Concrete: `CronJobClient`  
- Supports:
  - `GetFullList`
  - `Run`
- Uses `CommonOptions.ToDictionary()` to map query parameters.

---

## 4. Log Client

- Interface: `ILogClient`  
- Concrete: `LogClient`  
- Methods:
  - `GetListAsync` (paged results with `ListOptions`)  
  - `GetOneAsync`  
  - `GetStatsAsync` (hourly statistics)  
- Queries mapped via `ToDictionary()` extensions.  
- Validations added for required parameters.

---

## 5. Settings Client

- Interface: `ISettingsClient`  
- Concrete: `SettingsClient`  
- Methods:
  - `GetAllAsync` returns `JsonElement`  
  - `UpdateAsync` updates settings object  
  - `TestS3` and `TestEmailAsync` return `bool` on success  
  - `GenerateAppleClientSecretAsync` returns `AppleClientSecretResponse`  
- Parameters validated to prevent runtime errors.  

---

## 6. Realtime Client

- Interface: `IRealtimeClient`  
- Concrete: `RealtimeClient`  
- Designed to mimic JS SDK subscription API.  
- Tracks subscriptions locally and calls `_http.SendAsync` to register them.  
- **Note:** No live SSE events are streamed yet; only subscription registration is supported.  

### Standalone SSE Client

- Interface: `IRealtimeSseClient`  
- Concrete: `RealtimeSseClient`  
- Fully separated from HTTP-only clients.  
- Connects to `/realtime` via **Server-Sent Events**.  
- Dispatches live `RealtimeEvent` objects to callbacks.  
- Supports `Subscribe`, `Unsubscribe`, `StartListeningAsync`, and `Stop`.  
- Handles `IsConnected` state and `OnDisconnect` event.

---

## 7. Common Options & ToDictionary

- `CommonOptions` already has `Query` dictionary, `Body`, `Headers`.  
- `ToDictionary()` extensions normalize options for query parameters.  
- `Fields` property included if set.  
- Similar extensions created for `ListOptions` and `LogStatsOptions`.

---

## 8. Interfaces Importance

- **Decoupling:** Consumers depend on interface, not implementation.  
- **Testability:** Mocking and unit testing.  
- **Consistency:** All clients follow the same pattern.  
- **Future-proofing:** Change implementation without changing consumers.  
- **DI Friendly:** Easily inject clients.

---

## 9. Method Signature Rules

- All method signatures should be **single-line**.  
- Constructor parameters validated (`ArgumentNullException` for required parameters).  
- Optional parameters defaulted for convenience (e.g., `CancellationToken = default`).

---

## 10. Pending Enhancements / Notes

- Full SSE / WebSocket integration for `RealtimeClient`.  
- Optional automatic reconnect with exponential backoff.  
- Unified base client class could be introduced in future.  
- Exception messages could be aligned with JS SDK for parity.

## 11. RecordClient vs JS SDK RecordService

### JS SDK Features

The original JS SDK `RecordService` includes a broader set of convenience methods beyond core CRUD, such as:

- `getList(page, perPage, options)` ? paginated list of records.
- `getFullList(options)` ? retrieves all records across pages automatically.
- `getFirstListItem(filter, options)` ? returns the first record matching a filter.
- `getOne(recordId, options)` ? fetches a single record by ID.
- `create(bodyParams, options)` ? create new records.
- `update(recordId, bodyParams, options)` ? update existing records.
- `delete(recordId, options)` ? delete a record.
- `subscribe` ? integrates with the Realtime service for record events.
- Helpers for `expand` and batch operations (create/update/delete multiple records at once).

### C# `RecordClient` Implementation

- Focuses on **core REST CRUD operations** and pagination.
- Provides `GetListAsync`, `GetFullListAsync`, `GetFirstListItemAsync`, `GetOneAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`.
- Uses `IHttpTransport` for all HTTP communication.
- Includes parameter validation and query mapping via `ToDictionary()`.
- Does not include realtime subscription helpers, batch helpers, or certain convenience methods that exist in the JS SDK.
- Optional helper methods (batch operations, expanded relations, subscription integration) can be added later as extensions or additional clients.

### Rationale

- Keeps the API clean and focused on core responsibilities.
- Avoids over-complicating the initial C# port.
- Aligns with the “client-first” design philosophy.
- Makes future extension easier while preserving JS SDK semantics where it matters most.

