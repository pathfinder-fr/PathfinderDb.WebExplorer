# Catalog Usability Improvements Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make Pathfinder 1e catalog browsing more informative and clear while retaining readable item detail pages.

**Architecture:** Add spell-level grouping to the existing immutable `DataSnapshot`/`CatalogService` read model. Render shared compact item partials across catalog dimensions, centralize display formatting and Wiki URL filtering, and keep layout/home status based on the existing snapshot provider.

**Tech Stack:** .NET 10, ASP.NET Core Razor Pages, C# records, xUnit, source-generated `System.Text.Json`.

**Spec:** `docs/superpowers/specs/2026-09-27-catalog-feedback-design.md`

## Global Constraints

- Keep the current page size of 50 and catalog cache policy.
- Detail pages remain the place for complete descriptions and other long-form content.
- Existing detail URLs remain valid and continue to be linked from catalog entries.
- Only display item-level references whose parsed URI host is `www.pathfinder-fr.org` or a subdomain of `pathfinder-fr.org`.
- Keep the imported references in the domain model; Wiki-only is a presentation rule.
- Match known component values case-insensitively and preserve unknown values unchanged.
- Preserve the current 404 behavior for missing buckets and invalid pages.
- Do not replace the current snapshot or invalidate catalog caches when data loading or validation fails.
- Identify the current content as **Pathfinder première édition**; do not add second-edition data or edition-selection architecture.
- Do not add a client-side framework or dependency.

## Review Focus

- A level route with a non-integer, negative, unknown, or out-of-range value must return 404 — cover malformed and negative route values in `SpellLevelRouteTests`, and unknown buckets/pages in `CatalogServiceTests`.
- A spell with multiple classes at the same level must occur only once in that level bucket but display all applicable class assignments; negative source levels must not become browseable buckets — cover in `DomainSnapshotTests` and the by-level rendering test.
- Unknown component codes must remain visible and unknown localization keys must not leak in place of the English-name label — cover in `CatalogTextTests`.
- A malformed, relative, or spoofed host such as `pathfinder-fr.org.attacker.test` must not become an origin link — cover in `CatalogTextTests`.
- A missing optional catalog field or unavailable initial snapshot must not produce misleading blank values or ready-looking counts — cover in catalog partial tests and `HomepageRenderingTests`.

---

### Task 1: Add spell-level catalog indexing and route

**Files:**
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Data/Domain/DataSnapshot.cs`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Data/Domain/CatalogService.cs`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/ByLevel.cshtml`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/ByLevel.cshtml.cs`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/Index.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/Index.cshtml.cs`
- Test: `tests/PathfinderDb.Modern.Tests/DomainSnapshotTests.cs`
- Test: `tests/PathfinderDb.Modern.Tests/CatalogServiceTests.cs`
- Test: `tests/PathfinderDb.Modern.Tests/CatalogRouteTests.cs`
- Test: `tests/PathfinderDb.Modern.Tests/SpellLevelRouteTests.cs`

**Interfaces:**
- Consumes: `Spell.Levels : IReadOnlyList<SpellLevel>`, `CatalogPage<T>`, and the existing output-cache / ETag pattern on catalog pages.
- Produces: `DataSnapshot.SpellsByLevel : IReadOnlyDictionary<int, IReadOnlyList<Spell>>`; `CatalogService.SpellLevelBuckets : IReadOnlyList<int>`; `CatalogService.GetSpellsByLevel(int level, int page = 1) : CatalogPage<Spell>?`; `ByLevelModel.OnGet(string level, [FromQuery] int page = 1)`; GET `/sorts/niveau/{level}`.

- [ ] **Step 1: Write failing index and catalog tests**

Add tests proving that level buckets are sorted and discovered from the snapshot, a spell with multiple class assignments at the same level appears once in that bucket, different levels place the spell in each applicable bucket, negative levels are not indexed, and catalog results retain class-level data and page at 50 items. Cover unknown levels, page 0, and pages beyond the final page returning `null`. In `SpellLevelRouteTests`, call `ByLevelModel.OnGet` with `"not-a-level"` and `"-1"` and assert `NotFoundResult`.

- [ ] **Step 2: Run the focused tests and verify failure**

Run: `dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --filter "FullyQualifiedName~DomainSnapshotTests|FullyQualifiedName~CatalogServiceTests|FullyQualifiedName~CatalogRouteTests|FullyQualifiedName~SpellLevelRouteTests"`
Expected: FAIL because level grouping, the service query, and the route do not exist.

- [ ] **Step 3: Implement snapshot and service interfaces**

Build `SpellsByLevel` from each spell’s distinct non-negative numeric `SpellLevel.Level` values, preserving the already sorted `Spells` order. Add sorted `SpellLevelBuckets` and `GetSpellsByLevel(int level, int page = 1)` using the existing `GetPage` helper so unknown buckets and invalid pages follow the same `null` behavior.

- [ ] **Step 4: Add the level selector and result route**

Expose `SpellLevelBuckets` from `Spells.IndexModel`, add a “Par niveau” selector to `Pages/Spells/Index.cshtml`, and implement `ByLevelModel.OnGet(string level, [FromQuery] int page = 1)`. Parse `level` with `int.TryParse` using `NumberStyles.None` and `CultureInfo.InvariantCulture`; reject malformed and negative values with 404 before querying the service. Return 404 when the service returns `null`, apply the existing `"Catalog"` output-cache policy and `CatalogCacheHeaders` with the request path/query, and initially render linked spell names. Task 2 replaces that loop with the shared spell partial.

- [ ] **Step 5: Run focused tests and commit**

Run: `dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --filter "FullyQualifiedName~DomainSnapshotTests|FullyQualifiedName~CatalogServiceTests|FullyQualifiedName~CatalogRouteTests|FullyQualifiedName~SpellLevelRouteTests"`
Expected: PASS, including the new level route and invalid-bucket/page cases.

```bash
git add src/PathfinderDb.Modern/PathfinderDb.Data/Domain/DataSnapshot.cs src/PathfinderDb.Modern/PathfinderDb.Data/Domain/CatalogService.cs src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells tests/PathfinderDb.Modern.Tests/DomainSnapshotTests.cs tests/PathfinderDb.Modern.Tests/CatalogServiceTests.cs tests/PathfinderDb.Modern.Tests/CatalogRouteTests.cs tests/PathfinderDb.Modern.Tests/SpellLevelRouteTests.cs
git commit -m "feat: browse spells by level"
```

### Task 2: Render shared catalog summaries and French spell labels

**Files:**
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Shared/_FeatCatalogItem.cshtml`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Shared/_SpellCatalogItem.cshtml`
- Create: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Shared/_MonsterCatalogItem.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/CatalogText.cs`
- Modify: all result views under `Pages/Feats`, `Pages/Spells`, and `Pages/Monsters`, including `Pages/Spells/ByLevel.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/Detail.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/Detail.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/wwwroot/css/site.css`
- Test: `tests/PathfinderDb.Modern.Tests/CatalogTextTests.cs`
- Test: `tests/PathfinderDb.Modern.Tests/CatalogRouteTests.cs`
- Test: `tests/PathfinderDb.Modern.Tests/CatalogSummaryRenderingTests.cs`

**Interfaces:**
- Consumes: `Feat`, `Spell`, `Monster`, `Reference`, `CatalogText`, and Task 1’s `CatalogPage<Spell>` level results.
- Produces: three Razor partials accepting their respective domain records; `CatalogText.FormatSpellComponent(string) : string`; `CatalogText.IsPathfinderWikiReference(Reference) : bool`.

- [ ] **Step 1: Write failing formatting and rendering tests**

Test all six known spell component labels from the spec and case-insensitive lookup; unknown values return unchanged. Test `IsPathfinderWikiReference` for `www.pathfinder-fr.org`, another `*.pathfinder-fr.org` host, the apex host, a Paizo URL, a DRP URL, malformed/relative URLs, and `pathfinder-fr.org.attacker.test`. Test that spell localization key `en-US:name` renders as `Nom VO`. In `CatalogSummaryRenderingTests`, assert each result view invokes its type’s shared partial, including the by-level page; each partial renders the agreed fields and existing detail URL shape, optional values are omitted, and feat/spell detail views filter references through `IsPathfinderWikiReference`.

- [ ] **Step 2: Run focused tests and verify failure**

Run: `dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --filter "FullyQualifiedName~CatalogTextTests|FullyQualifiedName~CatalogRouteTests|FullyQualifiedName~CatalogSummaryRenderingTests"`
Expected: FAIL because the formatter, filter, and shared summary partials are absent.

- [ ] **Step 3: Implement centralized formatting**

Add the component mapping: `Verbal` → `Verbale`, `Somatic` → `Somatique`, `Material` → `Matérielle`, `Focus` → `Focaliseur`, `DivineFocus` → `Focaliseur divin`, and `FocusOrDivineFocus` → `Focaliseur ou focaliseur divin`. Add `IsPathfinderWikiReference(Reference)` using `HrefString ?? Href`, `Uri.TryCreate(..., UriKind.Absolute, ...)`, and an exact host or dot-boundary suffix check for `pathfinder-fr.org`; reject all other values. Preserve unknown component values.

- [ ] **Step 4: Implement the three compact summary partials**

`_FeatCatalogItem.cshtml` displays linked name, translated type labels, prerequisites through `FormatPrerequisite`, source, and Wiki reference when available. `_SpellCatalogItem.cshtml` displays linked name, French school, class/level assignments, translated components, range, target, casting time, source, English name under `Nom VO`, and Wiki reference. Retrieve English name from the localization value whose key matches `en-US:name` case-insensitively; omit it when missing. `_MonsterCatalogItem.cshtml` displays linked name, challenge rating, translated type, climate, environment, and source, omitting missing values. All links to detail routes keep existing URL shapes.

- [ ] **Step 5: Use partials consistently and update visual style**

Replace name-only loops in every alphabetical and dimension result view (feat by type/source, spell by school/class/source/level, monster by CR/type/source) with the matching partial. Render Wiki references only after `IsPathfinderWikiReference` succeeds on both summary and detail pages, with `target="_blank"` and `rel="noopener noreferrer"`. Keep imported domain references intact. Change `.catalog-list` entries from the three-column name-only grid into responsive, vertically flowing summary cards that are readable with up to 50 entries.

- [ ] **Step 6: Run focused tests and commit**

Run: `dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --filter "FullyQualifiedName~CatalogTextTests|FullyQualifiedName~CatalogRouteTests|FullyQualifiedName~CatalogSummaryRenderingTests"`
Expected: PASS; no rendered origin-link condition admits Paizo, DRP, invalid URLs, or a spoofed host.

```bash
git add src/PathfinderDb.Modern/PathfinderDb.Web tests/PathfinderDb.Modern.Tests/CatalogTextTests.cs tests/PathfinderDb.Modern.Tests/CatalogRouteTests.cs
git commit -m "feat: enrich catalog list entries"
```

### Task 3: Improve return navigation, edition identity, and homepage status

**Files:**
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Feats/Index.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Spells/Index.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Monsters/Index.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Shared/_Layout.cshtml`
- Modify: `src/PathfinderDb.Modern/PathfinderDb.Web/Pages/Index.cshtml`
- Test: `tests/PathfinderDb.Modern.Tests/CatalogRouteTests.cs`
- Test: `tests/PathfinderDb.Modern.Tests/HomepageRenderingTests.cs`

**Interfaces:**
- Consumes: `IDataSnapshotProvider.Status`, `DataLoadStatus`, `CatalogText`, and the existing Razor layout.
- Produces: header catalog counts shown only when `DataLoadStatus.State == Ready`; visible return links on all alphabetical results; homepage edition label, compact version footer, and unavailable-state alert.

- [ ] **Step 1: Write failing navigation and homepage tests**

Assert alphabetical result sections on `/dons/{initial}`, `/sorts/{initial}`, and `/monstres/{initial}` link back to their catalog selectors. Assert the shared header and homepage identify “Pathfinder première édition”. Assert layout markup gates all three counts on ready status and does not render count values otherwise. Assert the homepage no longer has the prominent catalogue-status card, shows the version in a compact technical footer when available, and still shows an alert with unavailable errors. Keep and update the existing homepage rendering test’s reader-first and logo assertions.

- [ ] **Step 2: Run focused tests and verify failure**

Run: `dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --filter "FullyQualifiedName~HomepageRenderingTests|FullyQualifiedName~CatalogRouteTests"`
Expected: FAIL on absent return links, edition label, header counts, and status placement.

- [ ] **Step 3: Add return links and shared first-edition identity**

Add “← Tous les dons”, “← Tous les sorts”, and “← Tous les monstres” links to the alphabetical-result branches, matching the existing filtered-page navigation. Add the Pathfinder première édition label to the shared header and homepage without adding edition switching.

- [ ] **Step 4: Move counts and technical status**

Inject `IDataSnapshotProvider` into `_Layout.cshtml` and display the three counts alongside their navigation labels only while status is `Ready`. Remove the separate technical-status panel from `Index.cshtml`. Place the current version in a compact technical footer on the homepage and keep unavailable status/errors visible there as an alert. Leave shared footer identity content in place.

- [ ] **Step 5: Run focused tests and commit**

Run: `dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --filter "FullyQualifiedName~HomepageRenderingTests|FullyQualifiedName~CatalogRouteTests"`
Expected: PASS with no ready-looking counts during loading or unavailable data states.

```bash
git add src/PathfinderDb.Modern/PathfinderDb.Web/Pages tests/PathfinderDb.Modern.Tests/HomepageRenderingTests.cs tests/PathfinderDb.Modern.Tests/CatalogRouteTests.cs
git commit -m "feat: clarify catalog navigation and status"
```

### Task 4: Document catalog behavior and verify the complete change

**Files:**
- Modify: `docs/modern-data-pipeline.md`
- Modify: `docs/superpowers/specs/2026-09-27-catalog-feedback-design.md` only if implementation decisions require a documented spec correction

- [ ] **Step 1: Update the modern pipeline documentation**

Document `/sorts/niveau/{level}`, dynamically discovered numeric levels, compact per-type catalog summaries with links to complete item pages, Pathfinder-fr.org-only origin links as a display rule, and the count/version/degraded-state placement. Keep existing data-source and deployment details accurate.

- [ ] **Step 2: Run full modern build and unit tests**

Run: `dotnet build src\PathfinderDb.Modern\PathfinderDb.Modern.sln --configuration Release`
Expected: build succeeds.

Run: `dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj --configuration Release`
Expected: all modern unit tests pass.

- [ ] **Step 3: Run real-data integration test**

Run: `$env:PATHFINDER_DATA_ROOT = 'D:\code\perso\pf\pf1-data'; dotnet test tests\PathfinderDb.Modern.IntegrationTests\PathfinderDb.Modern.IntegrationTests.csproj --configuration Release`
Expected: integration test passes against the local clone and retains the under-two-second load/index benchmark.

- [ ] **Step 4: Commit documentation and any approved spec correction**

```bash
git add docs/modern-data-pipeline.md docs/superpowers/specs/2026-09-27-catalog-feedback-design.md
git commit -m "docs: describe enriched catalog browsing"
```
