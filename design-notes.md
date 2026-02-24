# PocketBase.Blazor Design Notes

This is the high-level engineering philosophy for the project.

For contributor rules and review checklist, see `contributing-architecture.md`.

## Beta Reality (Important)

The project is currently in beta.

That means:

- API direction is intentional, but not final.
- Some project organization, naming conventions, and code style are still transitional.
- Documentation and examples are continuously being hardened.
- Stable versions are expected to raise consistency and polish standards.

Beta is not an excuse for random changes; it is a controlled phase for converging to stable architecture.

## Core Direction

- Keep PocketBase JS-SDK mental model familiar, but with a .NET-native implementation.
- Avoid JS interop for core client behavior.
- Keep transport/auth/hosting/cron concerns separated.
- Keep public surfaces interface-first to support consumer-side mocking and testability.
- Prefer explicit result-based flows for API outcomes.

## Why No JS Interop

- Predictable behavior in server and worker contexts.
- Easier diagnostics in .NET stack traces/logging.
- Better typed tooling and XML-doc discoverability.
- No browser runtime coupling for core SDK behavior.

## Hosting Direction (Non-WASM Focus)

- If no executable path is provided, host builder resolves/downloads PocketBase binary.
- If teams need deterministic binaries, explicit custom executable path is supported.
- Cron generation/build exists for teams that need custom runtime logic near PocketBase.

## Versioning Direction

- Package versioning is independent from PocketBase upstream version numbers.
- Compatibility is documented explicitly (for example, validated PocketBase ranges).

## Testing Direction

- Unit tests protect pure logic and boundaries.
- Integration tests are source-of-truth for public API behavior and docs examples.
- Prefer integration coverage over mocking frameworks for API/client behavior.
- For unit tests, prefer real objects and simple in-project fakes/stubs instead of mock libraries.
- Integration tests should not be run simultaneously, either through test runners or manually, due to the lack of isolation of the authorization context.

## Coding Conventions

- Private instance fields use `_camelCase`.
- Avoid `this.` qualification unless required for disambiguation.
- Prefer explicit local types in public-library code for readability.
- Use `var` only when the type is obvious from the right-hand side (for example, anonymous types or object creation with repetitive generic types).
- Keep DTO intent explicit: `*Request` for outbound payloads, `*Response` for inbound API payloads, `*Model` for reusable domain/client structures.
- Keep transport/auth/hosting/cron boundaries separated; do not move cross-cutting concerns into client DTOs.

## Module Organization Plan

Target structure (feature-first, boundary-aware):

- `Clients/<Feature>`: transport-facing client contracts and implementations.
- `Requests/<Feature>`: payloads sent to PocketBase.
- `Responses/<Feature>`: payloads received from PocketBase.
- `Models/<Feature>`: reusable SDK domain models not tied to a single HTTP request/response shape.
- `Options/<Feature>`: request option builders and query/body configuration objects.
- `Events/<Feature>`: event payload contracts used by realtime/event-driven flows.
- `Infrastructure/Http|Store|Hosting|Realtime`: runtime plumbing and cross-cutting integrations.
- `Abstractions`: top-level interfaces shared across features when needed.

## Evolution Intent

Before stable releases, expect improvements in:

- folder/module organization
- naming and coding conventions
- style consistency and contributor guardrails
- doc accuracy and coverage
