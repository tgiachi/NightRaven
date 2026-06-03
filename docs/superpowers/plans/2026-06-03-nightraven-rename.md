# NightRaven Project Rename Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rename the project identity from `NightRaven` to `NightRaven` across the .NET solution, C# namespaces/types, plugin API, frontend branding, Docker/runtime settings, and documentation.

**Architecture:** Treat this as a staged refactor, not a single global replace. First establish a clean baseline and safety branch, then move project directories/files, then rewrite source identifiers and text, then add runtime compatibility for legacy root/config names, and finally verify .NET, web, Docker, and residual text scans.

**Tech Stack:** .NET 10, C#, MSBuild `.slnx`, xUnit, React + Vite + TypeScript, Docker, Git.

---

## Current Context

- Current checkout is `main`.
- The worktree is dirty with unrelated pending changes. Do not execute this plan until those changes are committed, stashed, or the user explicitly chooses to include them in the rename branch.
- `dotnet build NightRaven.slnx -c Debug -nologo` passed before this plan was written.
- `NightRaven.slnx` currently uses solution folders and includes runtime projects, test projects, plugin fixture projects, and the new `src/NightRaven.UO.Domain` project.
- Text scan found hundreds of tracked references to `NightRaven`, `nightraven`, `NIGHTRAVEN`, `night_raven`, and related API names.

## Rename Policy

Canonical new names:

- Product/brand: `NightRaven`
- .NET namespace prefix: `NightRaven`
- Project files: `NightRaven.*.csproj`
- Solution file: `NightRaven.slnx`
- Lowercase IDs/filenames: `nightraven`
- Snake case runtime directory: `night_raven`
- Kebab case identifiers: `night-raven`
- Environment variable: `NIGHTRAVEN_ROOT`
- Runtime config file: `nightraven.toml`
- Docker image smoke tag: `nightraven-server-rename-check`

Compatibility retained for one release:

- `NIGHTRAVEN_ROOT` remains a fallback if `NIGHTRAVEN_ROOT` is not set.
- `nightraven.toml` remains readable if `nightraven.toml` does not exist.
- Plugin metadata IDs are renamed to `nightraven.*`; this is a hard API rename because plugins are still pre-release.

Generated and local files excluded from manual edits:

- `**/bin/**`
- `**/obj/**`
- `web/node_modules/**`
- `web/dist/**`
- `.idea/**`
- `*.user`

## File Structure

Renamed solution and projects:

- Rename: `NightRaven.slnx` -> `NightRaven.slnx`
- Rename: `src/NightRaven.Core/` -> `src/NightRaven.Core/`
- Rename: `src/NightRaven.Hosting/` -> `src/NightRaven.Hosting/`
- Rename: `src/NightRaven.Network/` -> `src/NightRaven.Network/`
- Rename: `src/NightRaven.Network.UO/` -> `src/NightRaven.Network.UO/`
- Rename: `src/NightRaven.Persistence/` -> `src/NightRaven.Persistence/`
- Rename: `src/NightRaven.Plugins/` -> `src/NightRaven.Plugins/`
- Rename: `src/NightRaven.Scripting.Lua/` -> `src/NightRaven.Scripting.Lua/`
- Rename: `src/NightRaven.Server/` -> `src/NightRaven.Server/`
- Rename: `src/NightRaven.UO.Domain/` -> `src/NightRaven.UO.Domain/`
- Rename: `tests/NightRaven.Tests/` -> `tests/NightRaven.Tests/`
- Rename: `tests/NightRaven.PluginFixtures.Basic/` -> `tests/NightRaven.PluginFixtures.Basic/`
- Rename: `tests/NightRaven.PluginFixtures.Empty/` -> `tests/NightRaven.PluginFixtures.Empty/`
- Rename: `tests/NightRaven.PluginFixtures.Multiple/` -> `tests/NightRaven.PluginFixtures.Multiple/`

Renamed representative C# symbols and files:

- `INightRavenPlugin` -> `INightRavenPlugin`
- `INightRavenService` -> `INightRavenService`
- `INightRavenEvent` -> `INightRavenEvent`
- `NightRavenServiceDescriptor` -> `NightRavenServiceDescriptor`
- `NightRavenServiceOrchestrator` -> `NightRavenServiceOrchestrator`
- `NightRavenTCPClient` -> `NightRavenTCPClient`
- `NightRavenTCPServer` -> `NightRavenTCPServer`
- `NightRavenUDPServer` -> `NightRavenUDPServer`
- `NightRavenScriptJsonContext` -> `NightRavenScriptJsonContext`
- `NightRavenContainerExtensions` -> `NightRavenContainerExtensions`
- `NightRavenCrest.tsx` -> `NightRavenCrest.tsx`

New runtime compatibility helper:

- Create: `src/NightRaven.Server/Data/RuntimePaths.cs`
- Test: `tests/NightRaven.Tests/Server/RuntimePathsTests.cs`

## Task 1: Prepare a Safe Baseline

**Files:**

- No code changes.
- Validates repo state before rename.

- [ ] **Step 1: Check whether the worktree is clean**

Run:

```bash
git status --short
```

Expected for isolated rename execution: no output.

If there is output, stop and ask the user to choose one of these two options:

```text
1. Commit or stash the current pending work, then run the rename on a clean branch.
2. Include the current pending work in the rename branch and accept mixed rename diffs.
```

Do not continue silently in a dirty tree.

- [ ] **Step 2: Create a dedicated branch**

Run:

```bash
git switch -c refactor/night-raven-rename
```

Expected: branch changes from `main` to `refactor/night-raven-rename`.

- [ ] **Step 3: Verify baseline build**

Run:

```bash
dotnet build NightRaven.slnx -c Debug -nologo
```

Expected: `Build succeeded.` with `0 Error(s)`.

- [ ] **Step 4: Commit only if baseline cleanup was needed**

If Step 1 required committing a pre-existing clean baseline, use the user's normal commit workflow and do not mix it with rename work. If no files changed in this task, do not commit.

## Task 2: Rename Solution, Project Directories, Project Files, and Named Source Files

**Files:**

- Rename all tracked paths containing `NightRaven`.
- Do not rename `bin`, `obj`, `node_modules`, `dist`, `.idea`, or `*.user`.

- [ ] **Step 1: Move solution file, project directories, and named files**

Run:

```bash
git mv NightRaven.slnx NightRaven.slnx

for path in \
  src/NightRaven.Core \
  src/NightRaven.Hosting \
  src/NightRaven.Network \
  src/NightRaven.Network.UO \
  src/NightRaven.Persistence \
  src/NightRaven.Plugins \
  src/NightRaven.Scripting.Lua \
  src/NightRaven.Server \
  src/NightRaven.UO.Domain \
  tests/NightRaven.Tests \
  tests/NightRaven.PluginFixtures.Basic \
  tests/NightRaven.PluginFixtures.Empty \
  tests/NightRaven.PluginFixtures.Multiple
do
  git mv "$path" "${path/NightRaven/NightRaven}"
done

while IFS= read -r -d '' path; do
  new_path="${path//NightRaven/NightRaven}"
  if [ "$path" != "$new_path" ]; then
    mkdir -p "$(dirname "$new_path")"
    git mv "$path" "$new_path"
  fi
done < <(git ls-files -z)
```

Expected: `git status --short` shows renames from `NightRaven` paths to `NightRaven` paths.

- [ ] **Step 2: Verify no tracked file path still contains old project names**

Run:

```bash
git ls-files | rg 'NightRaven|nightraven|NIGHTRAVEN|night_raven|night-raven'
```

Expected: either no output, or only intentionally excluded local files that are not tracked by Git. If tracked source paths remain, rename them before continuing.

- [ ] **Step 3: Commit path-only rename**

Run:

```bash
git add -A
git commit -m "refactor: rename NightRaven paths to NightRaven"
```

Expected: commit contains mostly rename entries and no large content rewrites yet.

## Task 3: Rewrite Namespaces, Types, Project References, and Branding Text

**Files:**

- Modify all tracked text files after path rename.
- This task updates content references, not path names.

- [ ] **Step 1: Run the mechanical tracked-file rewrite**

Run:

```bash
git ls-files -z \
  | grep -z -v -E '(^|/)(bin|obj|node_modules|dist)(/|$)' \
  | grep -z -v -E '(^|/)\.idea(/|$)' \
  | grep -z -v -E '\.user$' \
  | xargs -0 perl -pi -e '
      s/NIGHTRAVEN/NIGHTRAVEN/g;
      s/Night Raven/Night Raven/g;
      s/NightRaven/NightRaven/g;
      s/night-raven/night-raven/g;
      s/night_raven/night_raven/g;
      s/nightraven/nightraven/g;
    '
```

Expected: namespaces, project references, `InternalsVisibleTo`, solution paths, Docker commands, README text, web text, plugin IDs, and test expectations now use `NightRaven` names.

- [ ] **Step 2: Verify solution can list projects**

Run:

```bash
dotnet sln NightRaven.slnx list
```

Expected: all runtime projects, test project, and plugin fixture projects are listed under `NightRaven.*` paths.

- [ ] **Step 3: Verify compile errors are only ordinary rename misses**

Run:

```bash
dotnet build NightRaven.slnx -c Debug -nologo
```

Expected: likely FAIL on remaining symbols or file-generated names. Fix all compiler errors before committing.

- [ ] **Step 4: Commit mechanical content rewrite**

Run after build errors from Step 3 are fixed:

```bash
git add -A
git commit -m "refactor: rename NightRaven symbols to NightRaven"
```

Expected: commit contains namespace/type/content rewrites and project reference updates.

## Task 4: Add Runtime Root and Config Compatibility

**Files:**

- Create: `src/NightRaven.Server/Data/RuntimePaths.cs`
- Modify: `src/NightRaven.Server/Program.cs`
- Test: `tests/NightRaven.Tests/Server/RuntimePathsTests.cs`

- [ ] **Step 1: Add failing tests for new root/config behavior**

Create `tests/NightRaven.Tests/Server/RuntimePathsTests.cs`:

```csharp
using NightRaven.Core.Data.Directories;
using NightRaven.Core.Types;
using NightRaven.Server.Data;

namespace NightRaven.Tests.Server;

public sealed class RuntimePathsTests : IDisposable
{
    private readonly string? _oldPrimary = Environment.GetEnvironmentVariable("NIGHTRAVEN_ROOT");
    private readonly string? _oldLegacy = Environment.GetEnvironmentVariable("NIGHTRAVEN_ROOT");
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"nr-runtime-paths-{Guid.NewGuid():N}");

    [Fact]
    public void ResolveRootDirectory_CommandLineRoot_WinsOverEnvironment()
    {
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", Path.Combine(_root, "env"));
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", Path.Combine(_root, "legacy"));

        var root = RuntimePaths.ResolveRootDirectory(Path.Combine(_root, "cli"));

        Assert.Equal(Path.Combine(_root, "cli"), root);
    }

    [Fact]
    public void ResolveRootDirectory_PrimaryEnvironment_WinsOverLegacy()
    {
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", Path.Combine(_root, "new"));
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", Path.Combine(_root, "old"));

        var root = RuntimePaths.ResolveRootDirectory(null);

        Assert.Equal(Path.Combine(_root, "new"), root);
    }

    [Fact]
    public void ResolveRootDirectory_LegacyEnvironment_RemainsFallback()
    {
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", null);
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", Path.Combine(_root, "old"));

        var root = RuntimePaths.ResolveRootDirectory(null);

        Assert.Equal(Path.Combine(_root, "old"), root);
    }

    [Fact]
    public void ResolveConfigPath_UsesNewConfigNameByDefault()
    {
        var directories = Directories();

        var configPath = RuntimePaths.ResolveConfigPath(directories);

        Assert.Equal(Path.Combine(directories[DirectoryType.Config], "nightraven.toml"), configPath);
    }

    [Fact]
    public void ResolveConfigPath_UsesLegacyConfigOnlyWhenNewConfigIsMissing()
    {
        var directories = Directories();
        Directory.CreateDirectory(directories[DirectoryType.Config]);
        File.WriteAllText(Path.Combine(directories[DirectoryType.Config], "nightraven.toml"), string.Empty);

        var configPath = RuntimePaths.ResolveConfigPath(directories);

        Assert.Equal(Path.Combine(directories[DirectoryType.Config], "nightraven.toml"), configPath);
    }

    [Fact]
    public void ResolveConfigPath_NewConfigWinsOverLegacyConfig()
    {
        var directories = Directories();
        Directory.CreateDirectory(directories[DirectoryType.Config]);
        File.WriteAllText(Path.Combine(directories[DirectoryType.Config], "nightraven.toml"), string.Empty);
        File.WriteAllText(Path.Combine(directories[DirectoryType.Config], "nightraven.toml"), string.Empty);

        var configPath = RuntimePaths.ResolveConfigPath(directories);

        Assert.Equal(Path.Combine(directories[DirectoryType.Config], "nightraven.toml"), configPath);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", _oldPrimary);
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", _oldLegacy);

        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    private DirectoriesConfig Directories()
        => new(_root, Enum.GetNames<DirectoryType>());
}
```

- [ ] **Step 2: Run tests to verify failure**

Run:

```bash
dotnet test tests/NightRaven.Tests/NightRaven.Tests.csproj --filter "FullyQualifiedName~RuntimePathsTests" --nologo
```

Expected: FAIL because `RuntimePaths` does not exist.

- [ ] **Step 3: Implement runtime path helper**

Create `src/NightRaven.Server/Data/RuntimePaths.cs`:

```csharp
using NightRaven.Core.Data.Directories;
using NightRaven.Core.Types;

namespace NightRaven.Server.Data;

internal static class RuntimePaths
{
    public const string RootEnvironmentVariable = "NIGHTRAVEN_ROOT";
    public const string LegacyRootEnvironmentVariable = "NIGHTRAVEN_ROOT";
    public const string DefaultRootDirectoryName = "night_raven";
    public const string ConfigFileName = "nightraven.toml";
    public const string LegacyConfigFileName = "nightraven.toml";

    public static string ResolveRootDirectory(string? commandLineRootDirectory)
    {
        if (!string.IsNullOrWhiteSpace(commandLineRootDirectory))
        {
            return commandLineRootDirectory;
        }

        var primaryRoot = Environment.GetEnvironmentVariable(RootEnvironmentVariable);

        if (!string.IsNullOrWhiteSpace(primaryRoot))
        {
            return primaryRoot;
        }

        var legacyRoot = Environment.GetEnvironmentVariable(LegacyRootEnvironmentVariable);

        if (!string.IsNullOrWhiteSpace(legacyRoot))
        {
            return legacyRoot;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), DefaultRootDirectoryName);
    }

    public static string ResolveConfigPath(DirectoriesConfig directories)
    {
        ArgumentNullException.ThrowIfNull(directories);

        var configDirectory = directories[DirectoryType.Config];
        var configPath = Path.Combine(configDirectory, ConfigFileName);
        var legacyConfigPath = Path.Combine(configDirectory, LegacyConfigFileName);

        return File.Exists(legacyConfigPath) && !File.Exists(configPath)
            ? legacyConfigPath
            : configPath;
    }
}
```

- [ ] **Step 4: Wire `Program.cs` to use compatibility helper**

Modify `src/NightRaven.Server/Program.cs`:

```csharp
rootDirectory = RuntimePaths.ResolveRootDirectory(rootDirectory);
```

Replace direct config path construction:

```csharp
container.AddNightRavenConfig(RuntimePaths.ResolveConfigPath(directoriesConfig));
```

Expected: `NIGHTRAVEN_ROOT` is primary, `NIGHTRAVEN_ROOT` is fallback, default local root is `night_raven`, and config binding uses `nightraven.toml` unless only `nightraven.toml` exists.

- [ ] **Step 5: Run compatibility tests**

Run:

```bash
dotnet test tests/NightRaven.Tests/NightRaven.Tests.csproj --filter "FullyQualifiedName~RuntimePathsTests" --nologo
```

Expected: PASS, 6 tests.

- [ ] **Step 6: Commit runtime compatibility**

Run:

```bash
git add src/NightRaven.Server/Data/RuntimePaths.cs src/NightRaven.Server/Program.cs tests/NightRaven.Tests/Server/RuntimePathsTests.cs
git commit -m "refactor: add NightRaven runtime path compatibility"
```

## Task 5: Frontend Branding and Asset Rename

**Files:**

- Rename: `web/src/features/player/NightRavenCrest.tsx` -> `web/src/features/player/NightRavenCrest.tsx`
- Modify: `web/src/features/auth/FakeLoginPage.tsx`
- Modify: `web/src/features/player/PlayerPortalShell.tsx`
- Modify: `web/src/features/player/PlayerDashboard.tsx`
- Modify: `web/src/shared/layouts/Shell.tsx`
- Modify: `web/src/features/home/HomePage.tsx`
- Modify: `web/src/styles/global.css`
- Modify: `web/index.html`

- [ ] **Step 1: Verify frontend imports use the new component name**

Run:

```bash
rg -n "NightRavenCrest|NightRaven|nightraven" web/src web/index.html
```

Expected after Task 3 mechanical rewrite: no output. If output remains, replace:

```text
NightRavenCrest -> NightRavenCrest
NightRaven -> NightRaven
nightraven -> nightraven
```

- [ ] **Step 2: Run frontend build**

Run:

```bash
npm --prefix web run build
```

Expected: Vite build succeeds and writes `web/dist`.

- [ ] **Step 3: Commit frontend rename fixes if needed**

If Step 1 or Step 2 required additional frontend changes, run:

```bash
git add web/src web/index.html
git commit -m "refactor(web): rename NightRaven branding to NightRaven"
```

If no files changed in this task, do not commit.

## Task 6: Documentation, Docker, and Runtime Smoke

**Files:**

- Modify: `README.md`
- Modify: `Directory.Build.props`
- Modify: `.gitignore`
- Modify: `src/NightRaven.Server/Dockerfile`
- Modify: `docs/superpowers/**/*.md`

- [ ] **Step 1: Verify product metadata is renamed**

Run:

```bash
rg -n "NightRaven|nightraven|NIGHTRAVEN|night_raven|night-raven" README.md Directory.Build.props .gitignore src/NightRaven.Server/Dockerfile docs/superpowers
```

Expected: only intentional legacy compatibility references are allowed:

```text
NIGHTRAVEN_ROOT
nightraven.toml
```

Everything else should say `NightRaven`, `nightraven`, `NIGHTRAVEN`, `night_raven`, or `night-raven`.

- [ ] **Step 2: Run server smoke with new root env var**

Run:

```bash
rm -rf /tmp/nightraven-rename-smoke
timeout 8s env NIGHTRAVEN_ROOT=/tmp/nightraven-rename-smoke ASPNETCORE_URLS=http://127.0.0.1:0 dotnet run --project src/NightRaven.Server/NightRaven.Server.csproj -c Release --no-launch-profile
code=$?
printf 'EXIT_CODE=%s\n' "$code"
test "$code" -eq 124
test -d /tmp/nightraven-rename-smoke/plugins
test -f /tmp/nightraven-rename-smoke/config/nightraven.toml
```

Expected: exit code `124`, server starts and shuts down through timeout, plugin directory exists, and `nightraven.toml` exists.

- [ ] **Step 3: Run server smoke with legacy root env var**

Run:

```bash
rm -rf /tmp/nightraven-legacy-smoke
timeout 8s env NIGHTRAVEN_ROOT=/tmp/nightraven-legacy-smoke ASPNETCORE_URLS=http://127.0.0.1:0 dotnet run --project src/NightRaven.Server/NightRaven.Server.csproj -c Release --no-launch-profile
code=$?
printf 'EXIT_CODE=%s\n' "$code"
test "$code" -eq 124
test -d /tmp/nightraven-legacy-smoke/plugins
test -f /tmp/nightraven-legacy-smoke/config/nightraven.toml
```

Expected: exit code `124`, server still honors `NIGHTRAVEN_ROOT`, and writes the new `nightraven.toml` file.

- [ ] **Step 4: Build Docker image**

Run:

```bash
docker build -f src/NightRaven.Server/Dockerfile -t nightraven-server-rename-check .
```

Expected: Docker build succeeds.

- [ ] **Step 5: Commit docs and Docker rename**

Run:

```bash
git add README.md Directory.Build.props .gitignore src/NightRaven.Server/Dockerfile docs/superpowers
git commit -m "docs: rename NightRaven project references to NightRaven"
```

## Task 7: Final Verification and Residual Scan

**Files:**

- No planned new files.
- Verifies all rename tasks.

- [ ] **Step 1: Run full .NET test suite**

Run:

```bash
dotnet test tests/NightRaven.Tests/NightRaven.Tests.csproj -c Release --nologo
```

Expected: all tests pass.

- [ ] **Step 2: Build full renamed solution**

Run:

```bash
dotnet build NightRaven.slnx -c Release -nologo
```

Expected: `Build succeeded.` with `0 Error(s)`.

- [ ] **Step 3: Run residual source scan**

Run:

```bash
rg -n --glob '!**/bin/**' --glob '!**/obj/**' --glob '!web/node_modules/**' --glob '!web/dist/**' --glob '!**/*.user' --glob '!.idea/**' \
  "NightRaven|nightraven|NIGHTRAVEN|night_raven|night-raven" .
```

Expected: only intentional compatibility references:

```text
src/NightRaven.Server/Data/RuntimePaths.cs: LegacyRootEnvironmentVariable = "NIGHTRAVEN_ROOT"
src/NightRaven.Server/Data/RuntimePaths.cs: LegacyConfigFileName = "nightraven.toml"
tests/NightRaven.Tests/Server/RuntimePathsTests.cs: assertions for legacy environment/config fallback
docs/superpowers/plans/2026-06-03-nightraven-to-nightraven-rename.md: documents the old name and compatibility policy
```

- [ ] **Step 4: Verify tracked paths**

Run:

```bash
git ls-files | rg 'NightRaven|nightraven|NIGHTRAVEN|night_raven|night-raven'
```

Expected: no output.

- [ ] **Step 5: Verify Git status is scoped**

Run:

```bash
git status --short
```

Expected: either clean, or only intentional uncommitted final adjustments from this rename. Commit any intentional final adjustments:

```bash
git add -A
git commit -m "refactor: complete NightRaven rename"
```

## Self-Review Checklist

- Spec coverage:
  - Project identity renamed from `NightRaven` to `NightRaven`: Tasks 2-3.
  - `.slnx`, project paths, project references, and namespaces renamed: Tasks 2-3.
  - C# public API names renamed, including plugin and hosting interfaces: Tasks 2-3.
  - Runtime env/config names updated with compatibility: Task 4.
  - Web branding and component names renamed: Task 5.
  - Docker and documentation renamed: Task 6.
  - Residual old-name scan and full verification: Task 7.
- Placeholder scan:
  - No placeholder markers, deferred steps, or open implementation holes.
- Type consistency:
  - `INightRavenPlugin`, `INightRavenService`, `INightRavenEvent`, `NightRavenServiceOrchestrator`, and `AddNightRaven*` are used consistently after mechanical rewrite.
  - `NIGHTRAVEN_ROOT` and `nightraven.toml` are primary runtime names.
  - Legacy names are restricted to compatibility constants and tests.
