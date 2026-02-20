# Contributing Architecture Guide

This document defines the technical direction and contribution guardrails.

Use this together with `design-notes.md`:

- `design-notes.md`: high-level philosophy
- `contributing-architecture.md`: enforceable contribution expectations

## 1. Beta Scope and Expectations

Current status: beta.

Implications for contributors:

- Project structure/conventions/style are improving but not yet final "state of the art".
- You should improve consistency when touching nearby code.
- Avoid broad refactors unless they are directly tied to shipped behavior.
- Prioritize correctness, API clarity, and test coverage over large stylistic rewrites.

Stable targets:

- stricter organization boundaries
- stronger naming/style consistency
- hardened contributor standards

## 2. Architectural Principles

- .NET-native implementation, no JS interop for core behavior.
- JS-SDK semantic familiarity where practical.
- Explicit transport boundary via `IHttpTransport`.
- Interface-first public surfaces for consumer-side mocking.
- Explicit result modeling for expected API failures.

## 3. Layering and Responsibilities

### 3.1 Root client

`IPocketBase` is the main composition root.

Responsibilities:

- Expose domain clients.
- Coordinate shared auth/realtime state.
- Manage lifecycle/disposal of realtime resources.

### 3.2 Domain clients

Domain clients focus on PocketBase API areas (records, files, settings, etc).

Rules:

- No ad-hoc `HttpClient` plumbing inside domain clients.
- Use transport/query/options abstractions consistently.
- Keep method contracts cancellable and predictable.

### 3.3 Transport

`IHttpTransport` owns:

- request creation and serialization
- auth header propagation
- HTTP-to-result/error mapping
- stream/bytes/SSE transport behaviors

Contribution rule:

- If a new API call needs special behavior, first extend transport intentionally before bypassing it.

## 4. Interface-First Rule (Mockability by Design)

Every public client capability should be interface-backed.

Why:

- Consumer applications can mock dependencies with Moq/NSubstitute/FakeItEasy.
- Integration tests and unit tests remain decoupled from concrete implementations.
- Enables internal refactors with less break risk.

Minimum expectation:

- new public domain client -> new/update interface
- new public method -> interface + implementation + tests

## 5. Result and Error Model

Use explicit result flow for API outcomes.

Guidelines:

- Expected API failures should be represented in result state.
- Throw exceptions for programming errors/invalid arguments.
- Preserve enough failure metadata for diagnostics.

## 6. Hosting and Runtime Policy

Non-WASM scenarios are first-class for runtime hosting support.

### 6.1 Auto-resolve executable

Default host build may resolve/download PocketBase binary.

Use when:

- local dev convenience
- CI onboarding simplicity

### 6.2 Custom executable path

Support deterministic environments that pin binaries.

Use when:

- runtime downloads are disallowed
- custom internal PocketBase builds are required

## 7. Cron Generation Policy

Cron generation/build is optional and explicit.

Guidelines:

- Keep cron integration decoupled from default client usage.
- Treat Go toolchain dependency as opt-in.
- Document cron behavior in examples/tests when modified.

## 8. Testing Contract

### 8.1 Unit tests

Use for:

- pure logic
- transformations
- validations
- boundary behavior with mocked dependencies

### 8.2 Integration tests

Use for:

- real PocketBase behavior
- auth/permission semantics
- realtime/runtime behavior

Rule:

- Public API changes require integration coverage updates.

## 9. Documentation Contract

When public behavior changes, update:

- README usage snippet(s)
- integration tests that validate those snippets
- architecture/design docs if the direction changed

If docs and tests diverge, tests are source-of-truth for behavior until docs are fixed.

## 10. Review Checklist

Before requesting review, verify:

- no unnecessary JS interop introduced
- transport boundary preserved
- interface coverage updated
- cancellation support maintained
- result/error flow remains explicit
- integration coverage updated for public behavior
- docs updated where user-facing behavior changed

## 11. What Should Improve Before Stable

The stable milestone should improve:

- consistent folder/module boundaries
- naming and style conventions across all clients
- stricter contributor rules for API surface evolution
- stronger compatibility policy communication
- tighter docs/test synchronization process
