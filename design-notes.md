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

## Evolution Intent

Before stable releases, expect improvements in:

- folder/module organization
- naming and coding conventions
- style consistency and contributor guardrails
- doc accuracy and coverage
