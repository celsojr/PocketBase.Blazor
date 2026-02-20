# PocketBase.Blazor Design Notes

This document explains the architectural direction of the project for contributors and reviewers.
It focuses on the why behind decisions, not just the what.

## 1. Project Positioning

PocketBase.Blazor is a .NET-native SDK-style client inspired by PocketBase JS-SDK semantics.

Primary goals:

- Keep API ergonomics familiar to JS-SDK users.
- Stay fully .NET-native (no JS interop dependency).
- Be testable by downstream apps through interface-first design.
- Keep transport, auth state, hosting, and cron concerns separated.

Non-goals:

- Port every JS-SDK convenience feature immediately.
- Bind package version 1:1 to PocketBase versioning.
- Optimize for WASM-hosted PocketBase process scenarios.

## 2. Why No JS Interop

We intentionally avoid JS interop for core client behavior.

Why:

- Predictable runtime behavior in server and worker contexts.
- Easier diagnostics in .NET stacks (exceptions, logging, cancellation flow).
- Strong typing and XML-doc discoverability in C# tooling.
- No hidden dependency on browser runtime APIs.
- Cleaner unit/integration test surface for CI.

Consequence:

- We implement HTTP/realtime semantics directly in .NET.
- We accept extra maintenance effort to keep JS-SDK parity where practical.

## 3. API Shape and Parity Strategy

Design intent is semantic parity with PocketBase JS-SDK mental model, while remaining idiomatic in C#.

Current API principles:

- `IPocketBase` is the main entry point (`Collection(...)`, `Admins`, `Files`, `Settings`, etc).
- Domain-specific clients model PocketBase API areas.
- Methods are async and cancellation-friendly.
- Responses use `FluentResults` (`IsSuccess`, `Value`, `Errors`) rather than implicit exceptions-only flow.

Why `FluentResults`:

- Makes error handling explicit and composable.
- Avoids forcing exception-driven control flow for common API failures.
- Works well in UI and background jobs where failure is expected behavior.

## 4. Interface-First Design (Mock-Friendly by Intent)

Every major client has an interface.

Examples:

- `IPocketBase`
- `IRecordClient`, `IAdminsClient`, `ISettingsClient`, etc.
- `IHttpTransport`
- Hosting abstractions (`IPocketBaseHost`, `IPocketBaseHostBuilder`)

Why:

- Consumer apps can mock SDK boundaries using Moq/NSubstitute/FakeItEasy.
- Tests can focus on app behavior without spinning up PocketBase.
- Easier extension points and internal refactoring with less API breakage.

Guideline:

- New public client capability should have interface coverage at introduction time.

## 5. Transport Abstraction (`IHttpTransport`)

All clients depend on `IHttpTransport` instead of custom HTTP code.

Why:

- Enforces one place for auth header updates and request building.
- Centralizes JSON serialization behavior and API error mapping.
- Reduces duplicated HTTP plumbing across clients.
- Simplifies test doubles for low-level HTTP behavior.

Contributor rule:

- Do not add ad-hoc `HttpClient` logic inside domain clients when transport can own it.

## 6. Auth State and Lifecycle

`PocketBase` composes `PocketBaseStore` with auth/realtime integration.

Why:

- Auth token lifecycle is cross-cutting and should not leak into every feature client.
- Realtime subscriptions and auth state need consistent synchronization.

Lifecycle stance:

- `IPocketBase` is `IAsyncDisposable`.
- Realtime clients are disposed with the root client to avoid leaking long-lived connections.

## 7. Hosting Model (Non-WASM Focus)

For server/desktop/integration scenarios, hosting APIs enable managing PocketBase process lifecycle from .NET.

Key types:

- `PocketBaseHostBuilder`
- `IPocketBaseHost`
- `PocketBaseBinaryResolver`

### 7.1 Auto-resolve Executable

If no executable path is provided, builder resolves a local binary and auto-downloads if missing.

Why:

- Low-friction local setup for contributors and CI.
- Reduces onboarding steps for integration scenarios.

### 7.2 Custom Executable Path

`UseExecutable(...)` allows explicit executable control.

Why:

- Teams may pin custom builds or pre-installed binaries.
- Some environments disallow runtime download.
- Supports deterministic release engineering.

## 8. Cron Server Strategy

`CronGenerator` can generate Go cron server files and optionally build a custom binary.

Why this exists:

- PocketBase cron extension path is Go-native.
- Many .NET consumers still need custom cron behavior near their PocketBase instance.
- Generation flow reduces manual boilerplate and standardizes project structure.

Tradeoff:

- Requires Go toolchain for full build flow (`BuildBinary = true`).
- We keep this explicit and opt-in.

## 9. Testing Philosophy

Two layers by design:

- Unit tests for pure logic and boundaries.
- Integration tests as the usage source-of-truth for public API behavior.

Contributor expectation:

- Public API additions/changes should include integration coverage.
- README usage snippets should mirror integration-tested behavior.

Trait-based filtering exists to keep CI practical for optional dependencies (SMTP, Playwright, filesystem, Go runtime, etc).

## 10. Versioning and Compatibility Direction

SDK versioning is independent from PocketBase upstream versioning.

Why:

- PocketBase cadence does not map cleanly to SDK release cadence.
- We need room for SDK-level betas/patches without fake upstream coupling.

Compatibility should be documented explicitly (for example: validated against PocketBase `0.34.x`) rather than inferred from package version.

## 11. Contribution Guardrails

When reviewing or proposing changes, evaluate against this checklist:

- Does this preserve JS-SDK-style semantics where intended?
- Is the change .NET-native and free from unnecessary JS interop?
- Is the public surface interface-backed?
- Does the change bypass `IHttpTransport` (if yes, why)?
- Are failure modes explicit through `Result` where appropriate?
- Is integration coverage added/updated for public behavior?
- Does it keep hosting and cron concerns optional, not mandatory?

## 12. Open Direction (Intentional)

Likely areas for evolution:

- Broader parity coverage with JS-SDK convenience methods.
- Better host configuration providers (YAML/TOML support, etc).
- Stronger compatibility metadata and diagnostics.
- Continued stabilization of API contracts through beta iterations.
