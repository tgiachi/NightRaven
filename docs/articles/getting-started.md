# Getting started

## Prerequisites

- **.NET 10 SDK** (the repo pins the version in `global.json`).

## Build

```bash
# Full solution (Release)
dotnet build -c Release

# Or a single project
dotnet build src/NightRaven.Core/NightRaven.Core.csproj
```

## Run the server

```bash
dotnet run --project src/NightRaven.Server
```

On first run the server:

1. Resolves its **root directory** (see below) and creates the standard sub-folders
   (`config`, `save`, `scripts`, `plugins`, `logs`, …).
2. Creates `config/nightraven.toml` with defaults and **self-heals** any missing section on
   later boots.
3. Writes a `nightraven.pid` lock file to prevent a second instance from starting.
4. Seeds a default **`admin` / `admin`** administrator account when no users exist —
   change this password immediately.

### Command-line options

| Option | Description |
|---|---|
| `--root-directory <path>` | Override the runtime root directory. |
| `--debug` | Run with the `Development` environment (enables the API docs UI). |
| `--header false` | Suppress the ASCII banner on startup. |

### Root directory resolution

The runtime root is resolved in this order:

1. `--root-directory` argument
2. `NIGHTRAVEN_ROOT` environment variable (legacy `NIGHTHEAVEN_ROOT` is still honored)
3. `./night_raven` under the current working directory (default)

The config file is read from `<root>/config/nightraven.toml` (legacy `nightheaven.toml` is used
only when the new name is absent).

## Default endpoints

| Surface | Endpoint |
|---|---|
| UO game server (TCP) | port `2593` |
| UDP ping/echo server | port `12000` |
| HTTP host | `http://localhost:5265` (and `https://localhost:7043` in dev) |
| Version | `GET /api/version` |
| Metrics (OpenMetrics) | `GET /metrics` |
| API docs (dev only) | `GET /api/docs` |

## Run the tests

```bash
dotnet test tests/NightRaven.Tests/NightRaven.Tests.csproj
```

## Configuration

Configuration lives in a single TOML file with one section per subsystem. Example
(`config/nightraven.toml`):

```toml
[logger]
level = 3
log_packets = true

[network]
port = 2593
ping_server_enabled = true
ping_server_port = 12000

[persistence]
autosave_interval = "00:05:00"

[timing]
tick_duration = "00:00:00.0080000"
wheel_size = 512
```

Plugins can declare their own sections, which are bound at boot alongside the core ones.
