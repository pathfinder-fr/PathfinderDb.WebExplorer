# Repository guidance

This repository contains two Pathfinder data-browsing applications. Keep the
legacy application working while making most new application changes in the
modern .NET 10 implementation.

## Build and test

Run commands from the repository root in PowerShell.

Build the modern application and its data library:

```powershell
dotnet build src\PathfinderDb.Modern\PathfinderDb.Modern.sln --configuration Release
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

The legacy ASP.NET MVC 4 application targets .NET Framework 4.6 and uses the
Windows/MSBuild build wrapper:

```powershell
.\build.cmd
```

There is no repository-defined lint command.

## Architecture

- `src\PathfinderDb.WebExplorer` is the legacy ASP.NET MVC 4 app. Its
  `MemoryDataSet` loads selected XML files from `App_Data` into the
  `PathfinderDb.Schema` in-memory model; controllers and Razor views provide
  feat and spell browsing.
- `src\PathfinderDb.Modern` is the .NET 10 replacement developed alongside the
  legacy app. `PathfinderDb.Data` owns JSON import, domain models, validation,
  immutable snapshots, and catalog indexes. `PathfinderDb.Web` is an ASP.NET
  Core Razor Pages host that reads those catalogs through `CatalogService`.
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
