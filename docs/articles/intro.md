# Introduction

NightRaven is an event-driven, modular **Ultima Online** server emulator built on **.NET 10**
(standard JIT — no NativeAOT). It is rebuilt from scratch to keep the core ignorant of gameplay
so that shard operators can add or remove systems without touching it.

## Design principles

- **Gameplay-agnostic core.** `NightRaven.Core` contains only primitives (buffers, collections,
  strings, geometry, IDs, RNG, encoding). No combat, items, or packets.
- **Transport-agnostic network.** `NightRaven.Network` transforms bytes only; protocol framing and
  parsing live in dedicated modules.
- **Event bus first.** Cross-subsystem communication goes through `IEventBusService`, never by
  injecting services across module boundaries.
- **Testability is non-negotiable.** Every service ships with an interface, DI registration, and a
  test suite that mirrors the production folder layout.
- **KISS.** Prefer the simpler design; avoid premature abstractions.

## Project layout

| Project | Responsibility |
|---|---|
| `NightRaven.Core` | Gameplay-agnostic primitives: pooled buffers, `Serial`/`AutoInt32/64`, geometry, RNG, TOML/JSON helpers. |
| `NightRaven.Abstractions` | Cross-cutting contracts: config system, event bus, metrics, hosted-service orchestration, packet handlers, timers. |
| `NightRaven.Network` | TCP/UDP servers, middleware pipeline, framing, encryption (Twofish/login/game), UO Huffman compression, span reader/writer. |
| `NightRaven.Network.UO` | UO protocol: attribute-driven packet registry and incoming packet definitions. |
| `NightRaven.Persistence` | World snapshot (MessagePack) + append-only binary journal, data access, recovery. |
| `NightRaven.Plugins` | Assembly-isolated trusted .NET plugins with dependency ordering. |
| `NightRaven.Scripting.Lua` | MoonSharp Lua engine: modules, user data, bootstrap scripts, hot-reload. |
| `NightRaven.UO.Domain` | UO domain types (users, accounts) consumed by the server and plugins. |
| `NightRaven.Server` | The host: composes every subsystem via DryIoc and exposes the HTTP surface. |

## Technology stack

- **Language / runtime:** C# on .NET 10.
- **DI container:** DryIoc (ASP.NET Core rides on top of it).
- **Static data / config:** TOML (Tomlyn), with self-healing of missing sections at boot.
- **Persistence:** MessagePack snapshots + framed binary journal.
- **Scripting:** Lua via MoonSharp.

Continue with **[Getting started](getting-started.md)** to run the server, or jump to the
**[Architecture](architecture.md)** for the full picture.
