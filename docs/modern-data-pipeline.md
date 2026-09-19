# Modern data pipeline

The modern application under `src/PathfinderDb.Modern` runs beside the unchanged
legacy MVC application. It reads only the three root exports from an external
`pf1-data` clone:

* `feats.json`
* `spells.json`
* `monsters.json`
* `labels.json` (generated shared technical-label catalogue)

Configure the clone with `PathfinderData:RootPath` (for example
`D:\code\perso\pf\pf1-data`), or set `PathfinderData__RootPath` in an environment.
The clone is deliberately not copied into the application output or committed to
this repository.

`labels.json` is produced by `pf1-tools` alongside the exports. It contains
domain/key pairs and translations such as `spellSchool/conjuration` →
`Invocation`. The loader includes this file in the snapshot version when it is
present and also accepts the labels embedded in the root exports for
compatibility with older generated clones. `PathfinderDb.Schema` 2.1.0 is used
for the official `DataSet.GetLabel` fallback behavior.

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

The homepage is reader-first: it presents direct entry points for dons, sorts,
and monstres before the technical snapshot status. The shared shell uses the
legacy `pf-fr-db.png` logo on a warm parchment/ivory palette with serif
headings; the logo is copied into the modern static assets but the legacy
application remains unchanged.

The `/dons` and `/sorts` pages are selectors and no longer open an arbitrary
first alphabetical bucket. They expose the following deterministic, paginated
catalog routes:

* `/dons/type/{type}`
* `/dons/source/{source}`
* `/sorts/ecole/{school}`
* `/sorts/classe/{class}`
* `/sorts/source/{source}`

Each route uses the immutable snapshot indexes, keeps the fixed 50-item page
size, preserves the existing cache headers and output-cache policy, and returns
404 for an unknown bucket or page. Spell classes/lists, schools, feat types,
and sources are discovered from the loaded JSON rather than maintained as a
hard-coded enumeration; this keeps values such as `psychiste` available when
present in the data.

Monster navigation is available alphabetically, by challenge rating, type, and
source. Alphabetical pages use the same deterministic bucket and pagination
rules as feats and spells:

* `/monstres/{initial}`
* `/monstres/cr/{challengeRating}`
* `/monstres/type/{type}`
* `/monstres/source/{source}`
* `/monstres/detail/{slug}`

Initial, type, and source indexes are built in the immutable data snapshot, so
these routes keep the same bounded 50-item pagination and 404 behavior as the
existing catalog routes. Monster details use the explicit `/detail/` segment
to avoid ambiguity with the optional alphabetical route.

Feat and spell detail pages now render the normalized fields available in the
JSON exports, including prerequisite choices, normal text, source, spell
components, target, casting time, and localization values. Missing optional
fields remain omitted rather than rendered as empty placeholders.

Item-level origin references are preserved for feats and spells. Their detail
pages expose the URLs supplied by the export, such as the Pathfinder-fr.org
Wiki and DRP Black-Book-Éditions pages, without reconstructing URLs from names.
The current monster exports contain no item-level origin references, so monster
pages intentionally do not display invented per-creature links.

Technical category values remain canonical in the snapshot and URLs, but the
Razor presentation layer translates them for readers through the generated
label catalogue. For example, `Conjuration` is displayed as `Invocation` and
`Fey` as `Fées`; sources and spell-list identifiers without a generated label
retain the existing French terminology mappings. Missing or new values fall
back to the original value, so dynamic catalog discovery is preserved.

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
