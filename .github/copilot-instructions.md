# Repository guidance

This repository contains the current .NET 10 Pathfinder site and its archived
ASP.NET MVC 4 predecessor. Make all current application, build, test, and
deployment changes against the modern implementation; legacy code lives under
`archive\legacy` and is retained for reference.

## Build and test

Run commands from the repository root in PowerShell.

Build the current application and its data library from the root solution:

```powershell
.\build.cmd
# Equivalent:
dotnet build PathfinderDb.sln --configuration Release
```

Run the modern unit tests, or a single test class:

```powershell
dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj
dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --filter FullyQualifiedName~LocalizedLabelTests
```

The real-data integration test is opt-in and requires a local `pf1-data` clone:

```powershell
$env:PATHFINDER_DATA_ROOT = 'D:\code\perso\pf\pf1-data'
dotnet test tests\PathfinderDb.Modern.IntegrationTests\PathfinderDb.Modern.IntegrationTests.csproj
```

Run the current site locally:

```powershell
dotnet run --project src\PathfinderDb.Web\PathfinderDb.Web.csproj
```

There is no repository-defined lint command.

## Architecture

- `src\PathfinderDb.Web` is the current ASP.NET Core Razor Pages host.
- `src\PathfinderDb.Data` owns JSON import, domain models, validation,
  immutable snapshots, and catalog indexes. The web host reads those catalogs
  through `CatalogService`.
- `archive\legacy\src\PathfinderDb.WebExplorer` is the archived ASP.NET MVC 4
  app. Its `MemoryDataSet` loads selected XML files from `App_Data`; do not
  target it for current feature work.
- The modern app reads `feats.json`, `spells.json`, and `monsters.json` from an
  external `pf1-data` clone configured with `PathfinderData:RootPath` or
  `PathfinderData__RootPath`; the clone is not part of this repository or
  application output. `labels.json` supplies generated display labels when
  available. See `docs\modern-data-pipeline.md` for the data and deployment
  contract.
- On startup, `PathfinderDataLoader` deserializes with the source-generated
  `PathfinderJsonContext`, validates and maps the exports, then builds a
  versioned `DataSnapshot`. `DataSnapshotProvider` publishes a complete new
  snapshot atomically: failed initial loads leave the app explicitly
  unavailable, and failed later loads retain the last valid snapshot.
- The web host registers the loader, snapshot provider, catalog service, and
  optional refresh/cache services through dependency injection. The
  `DataRefreshService` is disabled by default; when enabled it fast-forward
  pulls the external clone and reloads data. Successful snapshot changes
  invalidate catalog output-cache entries.

## Repository-specific conventions

- Keep source data outside this repository. Configure its root with
  `PathfinderData:RootPath` / `PathfinderData__RootPath`; do not copy exports
  into the app or publish directory.
- Add JSON DTOs to `PathfinderJsonContext` and use its generated `JsonTypeInfo`
  for deserialization. The loader validates and maps transport data into
  domain models before the web layer consumes it.
- Treat `DataSnapshot` as the shared read model: it owns sorted catalogs and
  case-insensitive lookup/index dictionaries. Add catalog behavior to
  `CatalogService` and the snapshot indexes instead of scanning data in page
  handlers or hard-coding values that are discoverable from the exports.
- Catalog routes use deterministic buckets, 50-item pages, and return not
  found for unknown buckets or out-of-range pages. Keep challenge-rating
  route formatting/parsing culture-invariant.
- Preserve technical category values in the data and routes; translate labels
  for display through `LabelCatalog` / `CatalogText`, with the original value
  as the fallback for missing labels.
- Keep data reloads atomic. Do not replace a valid snapshot or invalidate its
  cache when loading or validation fails.
- The root `publish.bat` performs a direct publish to the IIS output directory.
  The scheduled `job.cmd` is the server entry point: it requires branch
  `master`, runs `git reset --hard`, then `git pull --ff-only origin master`,
  and publishes only if Git succeeds. It never runs `git clean`, so untracked
  files remain.
- Do not run `job.cmd` against a developer checkout: it intentionally discards
  local changes to tracked files before pulling.
