# Modern data pipeline

The modern application under `src/PathfinderDb.Modern` runs beside the unchanged
legacy MVC application. It reads only the three root exports from an external
`pf1-data` clone:

* `feats.json`
* `spells.json`
* `monsters.json`

Configure the clone with `PathfinderData:RootPath` (for example
`D:\code\perso\pf\pf1-data`), or set `PathfinderData__RootPath` in an environment.
The clone is deliberately not copied into the application output or committed to
this repository.

The data library uses `System.Text.Json` source generation, validates required
identifiers, source references, spell levels, and duplicate identifiers, then
publishes one immutable indexed snapshot. A failed initial load leaves the web
host running in an explicit `Unavailable` degraded state. A failed later load
keeps the previous valid snapshot while reporting the failure.

Run unit tests with:

```powershell
dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj
```

Run the opt-in real-clone test with:

```powershell
$env:PATHFINDER_DATA_ROOT = 'D:\code\perso\pf\pf1-data'
dotnet test tests\PathfinderDb.Modern.IntegrationTests\PathfinderDb.Modern.IntegrationTests.csproj
```

The daily Git refresh service and CDN invalidation are implemented as optional
runtime behaviors. Catalog responses now expose a shared public
cache policy (`max-age=300`, `s-maxage=3600`, and
`stale-while-revalidate=86400`) plus an ETag derived from the immutable
snapshot version and request path. Matching `If-None-Match` requests return
`304 Not Modified`; a new snapshot automatically produces new ETags.

The optional refresh service can pull the configured clone with
`git pull --ff-only` and reload the snapshot after a successful pull. It is
disabled by default to avoid unexpected network or working-tree changes.
Enable it explicitly with:

```json
{
  "PathfinderData": {
    "RefreshEnabled": true,
    "RefreshIntervalMinutes": 1440
  }
}
```

Git failures are logged and do not trigger a reload, while data validation
failures continue to use the snapshot provider's existing atomic/degraded
behavior. When a new snapshot becomes `Ready`, the server purges the
ASP.NET Output Cache entries tagged `catalog`; invalid data never purges the
previously valid cache. Catalog page models use this tag and the same policy
as the CDN-facing headers, so the edge cache remains the external layer and
the output cache is the local VM layer.

The modern Razor Pages UI uses one shared responsive layout with keyboard-focus
styles, accessible navigation labels, and compact catalogue/detail components.
The visual layer is kept static and server-rendered so it does not add a
client-side framework or reduce CDN cacheability.

Monster navigation is available by challenge rating, type, and source:

* `/monstres/cr/{challengeRating}`
* `/monstres/type/{type}`
* `/monstres/source/{source}`

Type and source indexes are built in the immutable data snapshot, so these
routes keep the same bounded 50-item pagination and 404 behavior as the
existing catalog routes.

Feat and spell detail pages now render the normalized fields available in the
JSON exports, including prerequisite choices, normal text, source, spell
components, target, casting time, and localization values. Missing optional
fields remain omitted rather than rendered as empty placeholders.

Monster challenge-rating route values use invariant decimal formatting (for
example `1.5`) for both catalogue links and route buckets, regardless of the
server's current UI culture. Trailing zeroes are removed, so an integer CR is
rendered as `14` rather than `14.0`; the current real export contains integer
CR values.

The current real-clone load and index benchmark must remain below 2 seconds.
The benchmark is covered by `RealPf1DataTests` and includes JSON deserialization,
normalization, validation, version hashing, and snapshot index construction.
ReadyToRun is the preferred deployment optimization to evaluate before full AOT:
the application uses Razor Pages and remains a server-rendered web host, so AOT
compatibility must be proven separately rather than enabled speculatively.

Measured on the local clone:

* load, validation, hashing, and index construction: **199 ms**;
* standard `win-x64` framework-dependent publish: **430,927 bytes**;
* ReadyToRun `win-x64` framework-dependent publish: **652,623 bytes**.

ReadyToRun publishes successfully but increases the application payload by about
51%; startup timing must therefore be measured in the target VM before enabling
it by default. Full Native AOT remains deferred because Razor Pages compatibility
has not been established.
