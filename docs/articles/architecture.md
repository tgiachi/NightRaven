# Architecture

NightRaven is composed of small, single-responsibility projects with a strict dependency
direction: `Core` → `Abstractions` → transport/persistence/scripting modules → `Server`. The
server is the only project that knows about every subsystem; it wires them together through
DryIoc.

## Dependency injection & hosting

Every long-lived subsystem implements `INightRavenService` (which extends `IHostedService`) and is
registered on the DryIoc container with a **start priority**. A single
`NightRavenServiceOrchestrator` — surfaced to the .NET generic host as the one `IHostedService` —
starts services in ascending priority order and stops them in reverse.

| Priority | Service |
|---:|---|
| 0 | Event bus |
| 3 | Timer wheel |
| 5 | Metrics |
| 10 | Game loop |
| 15 | Persistence |
| 20 | Network |
| 30 | Lua scripting |

ASP.NET Core registers its own services through `IServiceCollection`; the whole host is backed by
DryIoc (`DryIocServiceProviderFactory`) so the Lua engine can register and resolve script-module
types at runtime.

## Configuration

`ConfigService` loads a single TOML file once at boot. Each subsystem declares a config section
(`RegisterConfigSection`). Missing sections are created from defaults and written back
("self-healing"); malformed TOML or invalid values fail fast. Bound sections are registered as DI
instances.

## Event bus & game loop

The event bus has **two routing paths**:

- **Async events** (`IAsyncEvent`) are dispatched to `IAsyncEventHandler<T>` on the thread pool,
  sequentially per event, in registration order.
- **Tick events** (`ITickEvent`) are queued and drained by the **game-loop thread**, which invokes
  `ITickEventHandler<T>` deterministically in publish order. Tick handlers must be non-blocking.

The `GameLoopService` owns a dedicated background thread that, each iteration, drains tick events
and advances the hashed **timer wheel** (`ITimerService`), then idle-sleeps when there is no work.
This gives gameplay a single, deterministic execution context.

## Network & packet flow

`NightRaven.Network` is transport-only: a middleware pipeline transforms bytes and an optional
`INetFramer` extracts frames. UO protocol concerns live in `NightRaven.Network.UO`, where packets
are declared with `[PacketHandler(opcode, sizing, …)]` and discovered into a `PacketRegistry` by
assembly scanning.

The end-to-end inbound flow:

1. A TCP read raises `NightRavenTCPClient.OnDataReceived`.
2. `NetworkService` enqueues the bytes onto an ingress queue drained by a dedicated thread.
3. `PacketParser` accumulates bytes, validates opcode/length, and builds typed packets.
4. A `PacketReceivedEvent` (**tick** event) is published.
5. The game-loop thread drains it and `PacketDispatchHandler` routes it to the typed
   `IPacketHandler<TPacket>` instances via a `PacketContext` (with `Send`/`Broadcast` helpers).
6. Handlers enqueue outbound packets onto `IOutgoingPacketQueue`, drained by the outbound thread
   and written through `GameSession.SendPacket`.

Outbound and inbound run on separate threads from the deterministic game loop, keeping socket I/O
off the loop while preserving ordered gameplay dispatch.

## Persistence

World state is kept in memory and persisted two ways:

- **Snapshot** — a full MessagePack file written atomically (temp + rename).
- **Journal** — an append-only binary log of mutations, each record length-prefixed and checksummed
  (FNV-1a); a corrupt trailing record is detected and discarded on read.

At boot the service loads the latest snapshot and replays the journal. A timer triggers periodic
autosave, which writes a new snapshot and trims the journal. Entity types register a
`PersistenceEntityDescriptor` (stable type id + key selector); `IDataAccess<TEntity,TKey>` and
`IAutoDataAccess` (auto-increment keys) expose CRUD, reads returning detached clones for snapshot
isolation.

## Plugins

Trusted .NET plugins are loaded from the `plugins` directory, each in an isolated
`AssemblyLoadContext`. Plugins are topologically sorted by their declared dependencies and then
`Configure(container, context)` runs **before** global config binding, so a plugin can declare its
own config sections, services, Lua modules, persisted entities, and packet handlers.

## Scripting

`NightRaven.Scripting.Lua` embeds MoonSharp. .NET types are exposed as modules via
`[ScriptModule]`/`[ScriptFunction]`, user-data types are registered for direct use from Lua, and
bootstrap scripts (`bootstrap.lua`, `init.lua`, `main.lua`) run on start with `on_initialize` /
`on_ready` hooks. A file watcher enables hot-reload, and a `.luarc.json` is generated for editor
tooling.

## Metrics

Subsystems implement `IMetricProvider`. `MetricsService` polls them on a timer into an immutable
snapshot served at `GET /metrics` in **OpenMetrics** text format. Counters automatically gain the
`_total` suffix expected by Prometheus.
