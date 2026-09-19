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

The daily Git refresh service, public catalog routes, output cache, UI work, and
CDN invalidation are intentionally deferred to later increments.
