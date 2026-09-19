# Pathfinder FR DB visual refresh and deterministic navigation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Give role-players a warmer, reader-first homepage and deterministic paginated navigation for spell schools/classes/sources and feat types/sources.

**Architecture:** Keep the existing Razor Pages application and immutable `DataSnapshot` architecture. Add the logo as a static web asset, move homepage technical diagnostics below reader navigation, and extend snapshot indexes plus `CatalogService` with one dimension per route family. Each new route reuses the existing `CatalogPage<T>`, cache headers, Output Cache policy, and 404 behavior.

**Tech Stack:** ASP.NET Core 10 Razor Pages, C# records and immutable snapshot indexes, xUnit, static CSS, `System.Text.Json` source-generated data contracts.

**Spec:** `docs/superpowers/specs/2026-09-19-rpg-navigation-and-visual-refresh-design.md`

## Global Constraints

- Do not modify the legacy `src/PathfinderDb.WebExplorer` application except to read the existing logo asset.
- Keep the modern application server-rendered with no frontend framework or client-side catalog rendering.
- Keep fixed pagination at `CatalogService.PageSize` (50 items).
- Keep unknown buckets and invalid pages as HTTP 404; never return an empty successful catalog.
- Derive spell class/list, spell school, feat type, and source buckets from loaded JSON data; do not hard-code class or source enumerations.
- Preserve immutable snapshot publication, cache headers, Output Cache tagging, degraded startup, and existing routes.
- Use invariant/canonical route values where a bucket is numeric; textual buckets remain case-insensitive with snapshot display casing.
- Add the required co-author trailer to every commit.

---

## File map

- `src/PathfinderDb.Modern/PathfinderDb.Web/wwwroot/images/pf-fr-db.png`: copied legacy logo used by the modern layout.
- `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Index.cshtml`: reader-first homepage structure.
- `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Shared/_Layout.cshtml`: logo link/alt text and shared visual shell.
- `src/PathfinderDb.Modern/PathfinderDb.Web/wwwroot/css/site.css`: warm palette, serif headings, hero/cards, and responsive rules.
- `src/PathfinderDb.Modern/PathfinderDb.Data/Domain/DataSnapshot.cs`: immutable spell/feat/source indexes.
- `src/PathfinderDb.Modern/PathfinderDb.Data/Domain/CatalogService.cs`: bucket lists and paginated dimension queries.
- `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/Index.cshtml(.cs)`: spell dimension landing page.
- `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/BySchool.cshtml(.cs)`: school route.
- `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/ByClass.cshtml(.cs)`: class/list route.
- `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/BySource.cshtml(.cs)`: source route.
- `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/Index.cshtml(.cs)`: feat dimension landing page.
- `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/ByType.cshtml(.cs)`: type route.
- `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/BySource.cshtml(.cs)`: source route.
- `tests/PathfinderDb.Modern.Tests/DomainSnapshotTests.cs`: index construction tests.
- `tests/PathfinderDb.Modern.Tests/CatalogServiceTests.cs`: bucket/pagination/404 tests.
- `tests/PathfinderDb.Modern.Tests/HomepageRenderingTests.cs`: homepage ordering/content test.
- `tests/PathfinderDb.Modern.IntegrationTests/RealPf1DataTests.cs`: real-clone bucket smoke checks.
- `docs/modern-data-pipeline.md`: operational route and UI documentation.

### Task 1: Add the reader-first homepage and visual identity

**Files:**
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/wwwroot/images/pf-fr-db.png`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Index.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Shared/_Layout.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/wwwroot/css/site.css`
- Test: `tests/PathfinderDb.Modern.Tests/HomepageRenderingTests.cs`

**Interfaces:**
- The homepage continues to use `IndexModel.Status`; no status model changes are required.
- The layout serves `/images/pf-fr-db.png` with meaningful alt text and keeps the brand link at `/`.

- [ ] **Step 1: Write the failing homepage structure test**

Create a Razor-oriented content test that reads `Pages/Index.cshtml` and asserts reader navigation appears before the technical status marker:

```csharp
[Fact]
public void Homepage_places_reader_navigation_before_technical_status()
{
    var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
    var markup = File.ReadAllText(Path.Combine(
        repositoryRoot,
        "src", "PathfinderDb.Modern", "PathfinderDb.Web", "Pages", "Index.cshtml"));

    Assert.True(markup.IndexOf("Parcourir les dons", StringComparison.Ordinal) <
                markup.IndexOf("État des données", StringComparison.Ordinal));
    Assert.Contains("pf-fr-db", markup);
}
```

The five `..` segments resolve from `tests/PathfinderDb.Modern.Tests/bin/{Configuration}/net10.0`
to the repository root. Do not launch a browser for this unit-level ordering
assertion.

- [ ] **Step 2: Run the focused test and verify it fails**

Run:

```powershell
dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --no-restore --filter FullyQualifiedName~HomepageRenderingTests
```

Expected: FAIL because the current homepage does not reference the logo and renders technical status before the navigation buttons.

- [ ] **Step 3: Add the logo asset and reader-first markup**

Copy the existing binary asset from `src/PathfinderDb.WebExplorer/Content/images/pf-fr-db.png` into the modern `wwwroot/images` directory. Change the homepage to render:

```html
<section class="hero page-card">
  <img class="brand-mark" src="/images/pf-fr-db.png" alt="Pathfinder FR DB" />
  <p class="eyebrow">Référence Pathfinder en français</p>
  <h1>Les règles à portée de dés</h1>
  <p class="intro">Trouvez rapidement un don, un sort ou un monstre pour préparer vos parties.</p>
  <div class="portal-grid">
    <a class="portal-card" href="/dons"><strong>Dons</strong><span>Talents, prérequis et avantages</span></a>
    <a class="portal-card" href="/sorts"><strong>Sorts</strong><span>Écoles, classes et composantes</span></a>
    <a class="portal-card" href="/monstres"><strong>Monstres</strong><span>CR, types et sources</span></a>
  </div>
</section>
```

Keep the existing links to `/dons`, `/sorts`, and `/monstres`. Move state/version/counts/errors into a later `<section class="technical-status">` with the exact secondary heading `Informations techniques`.

- [ ] **Step 4: Apply the warm visual system**

Update `site.css` with CSS variables for ivory/parchment surfaces, brown ink, muted gold, and brick accent. Add:

```css
:root {
    --ink: #38291f;
    --muted: #766457;
    --line: #dfd0bd;
    --surface: #fffdf8;
    --surface-alt: #f4ecdf;
    --accent: #9b452e;
    --accent-dark: #713021;
    --gold: #c39a52;
    --heading-font: Georgia, Cambria, "Times New Roman", serif;
}
h1, h2, h3 { font-family: var(--heading-font); }
.brand-mark { display: block; max-width: min(100%, 22rem); height: auto; }
.portal-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr)); gap: 1rem; }
```

Keep visible keyboard focus, readable contrast, responsive stacking, and no external font download.

- [ ] **Step 5: Run the focused test and verify it passes**

Run the same focused test. Expected: PASS.

- [ ] **Step 6: Commit the visual increment**

```powershell
git add src\PathfinderDb.Modern\PathfinderDb.Web\wwwroot\images\pf-fr-db.png src\PathfinderDb.Modern\PathfinderDb.Web\Pages\Index.cshtml src\PathfinderDb.Modern\PathfinderDb.Web\Pages\Shared\_Layout.cshtml src\PathfinderDb.Modern\PathfinderDb.Web\wwwroot\css\site.css tests\PathfinderDb.Modern.Tests\HomepageRenderingTests.cs
git commit -m "feat: refresh roleplayer homepage and theme" -m "Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

### Task 2: Add immutable spell and feat dimension indexes

**Files:**
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Data/Domain/DataSnapshot.cs`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Data/Domain/CatalogService.cs`
- Test: `tests/PathfinderDb.Modern.Tests/DomainSnapshotTests.cs`
- Test: `tests/PathfinderDb.Modern.Tests/CatalogServiceTests.cs`

**Interfaces:**
- Add snapshot properties:
  - `SpellsBySchool: IReadOnlyDictionary<string, IReadOnlyList<Spell>>`
  - `SpellsByList: IReadOnlyDictionary<string, IReadOnlyList<Spell>>`
  - `SpellsBySource: IReadOnlyDictionary<string, IReadOnlyList<Spell>>`
  - `FeatsByType: IReadOnlyDictionary<string, IReadOnlyList<Feat>>`
  - `FeatsBySource: IReadOnlyDictionary<string, IReadOnlyList<Feat>>`
- Add service methods:
  - `GetSpellsBySchool(string? school, int page = 1)`
  - `GetSpellsByList(string? list, int page = 1)`
  - `GetSpellsBySource(string? source, int page = 1)`
  - `GetFeatsByType(string? type, int page = 1)`
  - `GetFeatsBySource(string? source, int page = 1)`
- Add sorted bucket properties:
  - `SpellSchoolBuckets`, `SpellListBuckets`, `SpellSourceBuckets`
  - `FeatTypeBuckets`, `FeatSourceBuckets`

- [ ] **Step 1: Write failing snapshot tests**

Extend `DomainSnapshotTests` with data containing two spells sharing a school, spells in lists `wizard` and `psychiste`, two feat types, and source IDs. Assert every index contains the expected immutable bucket and does not contain an empty key.

```csharp
Assert.Equal(["ray"], snapshot.SpellsBySchool["Evocation"].Select(x => x.Id));
Assert.Contains("psychiste", snapshot.SpellsByList.Keys);
Assert.Equal(["combat-feat"], snapshot.FeatsByType["Combat"].Select(x => x.Id));
```

- [ ] **Step 2: Write failing service tests**

Add tests proving each accessor returns a 50-item `CatalogPage<T>`, preserves canonical bucket casing, and returns `null` for unknown buckets and page numbers. Include a case-insensitive request such as `GetSpellsBySchool("evocation")`.

- [ ] **Step 3: Run focused tests and verify they fail**

```powershell
dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --no-restore --filter "FullyQualifiedName~DomainSnapshotTests|FullyQualifiedName~CatalogServiceTests"
```

Expected: FAIL because the new properties and accessors do not exist.

- [ ] **Step 4: Implement the immutable indexes**

Reuse the existing `BuildMonsterStringIndex` pattern with selectors:

```csharp
SpellsBySchool = BuildStringIndex(Spells, spell => spell.School);
SpellsByList = BuildStringIndex(Spells, spell => spell.Levels.Select(level => level.List));
SpellsBySource = BuildStringIndex(Spells, spell => spell.Source?.Id);
FeatsByType = BuildStringIndex(Feats, feat => feat.Types);
FeatsBySource = BuildStringIndex(Feats, feat => feat.Source?.Id);
```

Implement a generalized helper accepting `Func<T, IEnumerable<string?>>`, trimming values, skipping null/blank values, grouping case-insensitively, and materializing arrays before publication. Do not mutate existing indexes.

- [ ] **Step 5: Implement service accessors**

Route each method through the existing generic `GetPage` helper so pagination, case-insensitive matching, and 404 translation remain consistent. Bucket properties must sort with `StringComparer.OrdinalIgnoreCase`.

- [ ] **Step 6: Run focused tests and verify they pass**

Run the focused test command again. Expected: all snapshot and catalog tests PASS.

- [ ] **Step 7: Commit the index increment**

```powershell
git add src\PathfinderDb.Modern\PathfinderDb.Data\Domain\DataSnapshot.cs src\PathfinderDb.Modern\PathfinderDb.Data\Domain\CatalogService.cs tests\PathfinderDb.Modern.Tests\DomainSnapshotTests.cs tests\PathfinderDb.Modern.Tests\CatalogServiceTests.cs
git commit -m "feat: index feats and spells by deterministic dimensions" -m "Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

### Task 3: Add deterministic feat and spell pages

**Files:**
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/Index.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/Index.cshtml.cs`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/ByType.cshtml`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/ByType.cshtml.cs`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/BySource.cshtml`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/BySource.cshtml.cs`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/Index.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/Index.cshtml.cs`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/BySchool.cshtml`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/BySchool.cshtml.cs`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/ByClass.cshtml`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/ByClass.cshtml.cs`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/BySource.cshtml`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/BySource.cshtml.cs`
- Test: `tests/PathfinderDb.Modern.Tests/CatalogRouteTests.cs`

**Interfaces:**
- Each page model injects `CatalogService`, stores a nullable `CatalogPage<T>`, applies `CatalogCacheHeaders`, returns `NotFound()` when the service returns null, and uses `[OutputCache(PolicyName = "Catalog")]`.
- Routes are exactly `/dons/type/{type}`, `/dons/source/{source}`, `/sorts/ecole/{school}`, `/sorts/classe/{class}`, and `/sorts/source/{source}`.

- [ ] **Step 1: Write failing route tests**

Add route/page tests against the endpoint metadata or Razor route declarations asserting the five exact templates and the landing pages' dimension links. Include unknown bucket/page cases through the service-backed page model.

- [ ] **Step 2: Run route tests and verify they fail**

```powershell
dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --no-restore --filter FullyQualifiedName~CatalogRouteTests
```

Expected: FAIL because the route pages and landing links do not exist.

- [ ] **Step 3: Implement page models**

Follow `Pages/Monsters/ByType.cshtml.cs` and `BySource.cshtml.cs`. For example:

```csharp
[OutputCache(PolicyName = "Catalog")]
public sealed class BySchoolModel(CatalogService catalogs) : PageModel
{
    public CatalogPage<Spell>? CatalogPage { get; private set; }

    public IActionResult OnGet(string school, [FromQuery] int page = 1)
    {
        CatalogPage = catalogs.GetSpellsBySchool(school, page);
        if (CatalogPage is not null && catalogs.SnapshotVersion is { } version &&
            CatalogCacheHeaders.Apply(Response, version, Request.Path + Request.QueryString))
            return StatusCode(StatusCodes.Status304NotModified);
        return CatalogPage is null ? NotFound() : Page();
    }
}
```

Use the corresponding service method for each dimension.

- [ ] **Step 4: Implement pages and landing selectors**

Each dimension page renders its bucket title, a 50-item list linking to existing detail routes, and the standard previous/next pagination links. Change `/dons` and `/sorts` landing pages to show selector sections instead of opening the default `A` bucket. Include source/type/school/class links using canonical bucket values.

- [ ] **Step 5: Run route tests and verify they pass**

Run the focused route test command. Expected: PASS.

- [ ] **Step 6: Commit the route increment**

```powershell
git add src\PathfinderDb.Modern\PathfinderDb.Web\Pages\Feats src\PathfinderDb.Modern\PathfinderDb.Web\Pages\Spells tests\PathfinderDb.Modern.Tests\CatalogRouteTests.cs
git commit -m "feat: add deterministic feat and spell navigation" -m "Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

### Task 4: Integrate, document, and verify against the real clone

**Files:**
- Modify: `tests/PathfinderDb.Modern.IntegrationTests/RealPf1DataTests.cs`
- Modify: `docs/modern-data-pipeline.md`
- Modify: `C:\Users\tbolon\.copilot\session-state\9bcf264a-8a5d-4fd4-a7dc-1e1fde73a434\plan.md`

- [ ] **Step 1: Add real-clone assertions**

Load the real snapshot and assert that school, list, source, and feat-type bucket collections are non-empty. Assert at least one real spell list and source can be passed to the corresponding `CatalogService` method and returns a non-null first page.

- [ ] **Step 2: Update operational documentation**

Document the new landing-page behavior, all five route templates, fixed 50-item pagination, dynamic class/list discovery, the logo/theme, and the fact that technical status is secondary on the homepage.

- [ ] **Step 3: Run the complete validation**

Stop the local site before commands that copy web binaries, then run:

```powershell
dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --no-restore
$env:PATHFINDER_DATA_ROOT = 'D:\code\perso\pf\pf1-data'
dotnet test tests\PathfinderDb.Modern.IntegrationTests\PathfinderDb.Modern.IntegrationTests.csproj --no-restore
dotnet build src\PathfinderDb.Modern\PathfinderDb.Modern.sln --configuration Release --no-restore
git diff --check
```

Expected: all tests pass, the Release build succeeds, and only intended files are changed.

- [ ] **Step 4: Run the site and verify representative URLs**

Start with:

```powershell
$env:PathfinderData__RootPath = 'D:\code\perso\pf\pf1-data'
dotnet run --project src\PathfinderDb.Modern\PathfinderDb.Web\PathfinderDb.Web.csproj --no-launch-profile --urls http://127.0.0.1:5200
```

Verify `/`, `/dons`, `/sorts`, `/monstres`, one feat type/source page, one spell school/class/source page, and the static logo. Confirm unknown buckets return 404 and detail links still return 200.

- [ ] **Step 5: Update the session plan and commit the integration increment**

Mark the visual refresh and deterministic feat/spell navigation complete and record the next remaining work as HTTP regression coverage for degraded startup plus optional climate/environment monster indexes.

```powershell
git add tests\PathfinderDb.Modern.IntegrationTests\RealPf1DataTests.cs docs\modern-data-pipeline.md
git commit -m "test: verify visual and deterministic navigation surfaces" -m "Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
```

## Final self-review

- Spec coverage: homepage hierarchy and theme are Task 1; immutable indexes are Task 2; routes and paginated selectors are Task 3; error/cache compatibility is preserved in Tasks 2-3; testing and documentation are Task 4.
- Placeholder scan: no TBD/TODO/FIXME steps are used; every task names concrete files, commands, and expected outcomes.
- Type consistency: all page models consume `CatalogPage<T>` and the exact `CatalogService` methods declared in Task 2; all routes use the exact templates declared in Task 3.
