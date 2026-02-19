# PocketBase.Blazor Unit Tests

This project contains fast unit tests for core behavior that does not require a running PocketBase instance.

## Project

- `tests/PocketBase.Blazor.UnitTests/PocketBase.Blazor.UnitTests.csproj`

## Run All Unit Tests

```bash
dotnet test ./tests/PocketBase.Blazor.UnitTests/PocketBase.Blazor.UnitTests.csproj
```

## Run Fast Smoke Subset

Skips environment-dependent tests (PocketBase process, Go runtime, filesystem-heavy paths):

```bash
dotnet test ./tests/PocketBase.Blazor.UnitTests/PocketBase.Blazor.UnitTests.csproj --filter "Category=Unit&Requires!=Pocketbase&Requires!=GoRuntime&Requires!=FileSystem"
```

## Traits Used

- `Category=Unit`
- `Requires=Pocketbase`
- `Requires=GoRuntime`
- `Requires=FileSystem`

## Notes

- Prefer adding unit tests first for pure logic.
- Add integration coverage when behavior depends on real PocketBase HTTP/runtime semantics.
