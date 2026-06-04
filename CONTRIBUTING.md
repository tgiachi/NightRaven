# Contributing to NightRaven

## Branching model (GitFlow)

```
feature/*  ──PR──▶  develop  ──PR (release only)──▶  main
```

- **`feature/*`** — all day-to-day work happens on feature branches cut from `develop`.
- **`develop`** — integration branch. Opening a PR into `develop` runs the **CI** pipeline
  (build, test, coverage, ReSharper inspection). The PR can be merged once CI is green.
- **`main`** — release branch. A PR from `develop` to `main` is opened **only for a release**.
  Merging it (the release) triggers the **docs** workflow, which rebuilds and deploys the
  documentation to GitHub Pages.

### Typical flow

1. `git switch develop && git pull`
2. `git switch -c feature/<short-name>`
3. Commit your work (see *Commits* below) and open a PR into **`develop`**.
4. CI runs on the PR; merge once green.
5. When cutting a release, open a PR from **`develop`** into **`main`**. Merging it deploys
   the docs.

## CI/CD

| Workflow | Trigger | What it does |
|---|---|---|
| `ci.yml` | PR to `develop`/`main`, push to `develop` | Restore, build, test with coverage, ReSharper inspection (report-only). |
| `docs.yml` | push to `main`, published release | Builds DocFX docs and deploys to GitHub Pages. |

- **Coverage** is collected with Coverlet and summarized in the run summary + uploaded as an
  artifact.
- **Code style / quality** is checked with the **ReSharper CLI** (`jb inspectcode`), because the
  repository is formatted with ReSharper/Rider (see `.editorconfig`). The inspection runs in
  **report-only** mode: findings are surfaced (job summary / code scanning) but do not block the
  pipeline.

## Code style

The full spec is in [`CODE_CONVENTION.md`](./CODE_CONVENTION.md); rules are encoded in
`.editorconfig` and enforced locally by Rider/ReSharper. Highlights:

- One primary type per file; file-scoped namespaces mirroring the folder path.
- No primary constructors; no expression-bodied constructors.
- Prefer **collection expressions** (`[]`, `[.. source]`) over `Array.Empty<T>()`,
  `new T[0]`, `.ToArray()`, `.ToList()`.
- Prefer **`System.Threading.Lock`** over a plain `object` used as a lock target.
- Use `""` instead of `string.Empty`; static Serilog via `Log.ForContext<T>()`.

## Commits

- **Conventional Commits**: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`, `perf:`,
  `ci:` — add a scope when it helps (`feat(buffers):`).
- Title and body **in English**, one bullet per logically distinct change.
- Never add `Co-Authored-By`. Never bypass hooks, amend, or force-push.

## Local commands

```bash
dotnet build NightRaven.slnx -c Release          # build everything
dotnet test  NightRaven.slnx                      # run the test suite
docfx docs/docfx.json --serve                     # preview the docs locally
```
