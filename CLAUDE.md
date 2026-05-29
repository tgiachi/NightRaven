# NightHeaven

Ultima Online server emulator written in modern C#. Conceptual successor to Moongate, redesigned from scratch around an event-driven, modular architecture.

## Project goal

Do UO right. Not by adding features, but by addressing the structural problems no emulator has seriously tackled:

- **True modularity** — a shard operator can add or remove systems without touching the core.
- **Modern DX** — fast setup, hot-reload, clear errors, real documentation, decent tooling.
- **Testability as an architectural requirement**, not an afterthought — combat, skills and crafting must be testable in isolation.
- **Gameplay-agnostic core** — all UO-specific logic comes through modules.

The differentiator is not the language or a paradigm. It's that a new shard operator can take the server, start it, and within an hour understand how it works and how to customize it.

## Stack

- **Language**: C# on **.NET 10**.
- **Compilation**: standard JIT. **No NativeAOT** — the constraints (no reflection, no dynamic code, trimming warnings everywhere) don't pay off for this project. Do not add `PublishAot`, `IsAotCompatible`, or AOT-related warnings/analyzers.
- **Architecture**: event-driven and modular. Core stays ignorant of gameplay.
- **Static data definitions**: TOML.
- **Behavior scripting**: TBD.

## Architectural rules

- **`NightHeaven.Core` is gameplay-agnostic.** No combat, skills, items, packets, or UO concepts. Only primitives (buffers, collections, strings, encoding, RNG, logging, event bus, etc.).
- **UO-specific logic lives in modules** that opt-in through the event bus and plugin system.
- **Event bus first.** Cross-subsystem communication goes through `IEventBus`; do not inject services directly across module boundaries.
- **Testability is non-negotiable.** New services come with an interface, DI registration, and a test suite in `tests/NightHeaven.Tests/` mirroring the production folder layout.
- **KISS.** When in doubt, pick the simpler design. Avoid premature abstractions and frameworky patterns.

## Conventions

The full convention spec lives in [`CODE_CONVENTION.md`](./CODE_CONVENTION.md). Highlights that override defaults:

- One primary type per `.cs` file. Match file name to type name.
- File-scoped namespaces, namespace mirrors folder path.
- Enums always under `Types/`, with the domain in the name (e.g. `LogLevelType`, `DirectoryType`).
- DTOs/records under `Data/`. Internal-only data under `Data.Internal.*`.
- Interfaces under `Interfaces/`, every interface has `///` XML docs.
- No primary constructors. No expression-bodied constructors — always `{ }`.
- Class member order: `const` → `private readonly _x` → fields → properties → ctor → public → protected → private → `Dispose` (always last).
- Logging via Serilog static `Log.ForContext<T>()`. No `ILogger<T>` DI.
- Always `""` instead of `string.Empty`.
- Tests under `tests/NightHeaven.Tests/<Domain>/<Subdomain>/<Subject>Tests.cs`, one test class per subject, methods named `Method_Scenario_ExpectedResult`. Shared fakes/builders under `tests/.../Support/`.

## Commits

- **Conventional Commits** (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`, `perf:`). Add a scope when it helps (`feat(buffers):`, `fix(strings):`).
- Title and body **always in English**, regardless of conversation language.
- One bullet per logically distinct change in the body.
- **Never** add `Co-Authored-By: Claude`.
- Never bypass hooks (`--no-verify`). Never amend. Never force-push.

## Documentation

- Plan docs and design specs are **not committed** to the repo. They live in `~/docs/plans/NightHeaven/`.
- `docs/plans/`, `docs/superpowers/` and similar are explicitly excluded from commits.

## Useful commands

```bash
# Build core
dotnet build src/NightHeaven.Core/NightHeaven.Core.csproj

# Run full test suite
dotnet test tests/NightHeaven.Tests/NightHeaven.Tests.csproj

# Full solution build (Release)
dotnet build -c Release
```
