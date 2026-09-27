# Catalog usability feedback design

## Context

The .NET 10 Razor Pages application in `src/PathfinderDb.Modern` is the active
replacement for the legacy MVC application. Its catalog landing pages let
readers choose alphabetical, type, school, class, or source buckets. Result
pages currently show only item names and link to per-item detail routes.

The accepted user feedback asks for more useful list results, clearer return
navigation, spell-level browsing, better French labels and source links, more
prominent Pathfinder first-edition identification, and less prominent
technical status on the homepage.

The real `pf1-data` exports contain the three catalogs and `labels.json`.
Spell source references include the Pathfinder-fr.org Wiki, Paizo PRD, and DRP
Black-Book-Éditions; only the Wiki link should be presented. Spell component
values include `Verbal`, `Somatic`, `Material`, `Focus`, `DivineFocus`, and
`FocusOrDivineFocus`. English spell names are represented by localization keys
such as `en-US:name`.

## Goals

- Make catalog lists informative without replacing the readable detail pages.
- Keep list presentation consistent regardless of how the user selected a
  catalog bucket.
- Provide spell-level navigation backed by actual imported data.
- Make navigation, edition identity, and source/localization labels clear to
  French readers.
- Preserve existing pagination, cache behavior, server-rendered Razor Pages,
  and explicit degraded-data reporting.

## Design

### Catalog result entries

Use shared Razor partials for feat, spell, and monster list entries. Render the
same partials on alphabetical result pages and every filtered result page.
Entries are compact summaries, not copies of the full detail pages:

- **Feat:** name, types, prerequisites, source, and an item-level Wiki link
  when present.
- **Spell:** name, French school label, level by class, translated components,
  range, target, casting time, source, English name labeled `Nom VO`, and an
  item-level Wiki link when present.
- **Monster:** name, challenge rating, type, climate, environment, and source.
  The current export does not contain full monster stat blocks; do not imply
  that the summary is a complete stat block.

The item name remains a link to its existing `/dons/detail/{slug}`,
`/sorts/detail/{slug}`, or `/monstres/detail/{slug}` route. Detail pages remain
the place for complete descriptions and other long-form content. Continue to
omit missing optional fields rather than render empty labels.

Keep the current page size of 50 and pagination behavior. Style summary entries
as readable, vertically flowing cards rather than retaining the current
three-column name-only grid.

### Spell-level catalog

Extend `DataSnapshot` with a spell index grouped by the numeric spell level.
Each spell appears once in each level bucket where one or more class/list
assignments use that level. Keep the assignments attached to the spell so the
result entry can show which classes use the selected level.

Expose discovered level buckets from `CatalogService` and add a paginated
Razor Page at `/sorts/niveau/{level}`. The `/sorts` selector gains a “Par
niveau” section. Unknown level values and invalid or out-of-range page numbers
return 404, consistent with existing catalog dimensions. Keep spell-level
formatting independent of the current UI culture.

### Return navigation

Add a visible link from alphabetical result pages for feats, spells, and
monsters back to the corresponding catalog selector. Filtered result pages
already link to their selector; retain those links. Detail pages keep their
current catalog return links, while every list entry remains directly linked
to its detail page.

### Wiki-only references

Only display item-level references whose parsed URI host is
`www.pathfinder-fr.org` or a subdomain of `pathfinder-fr.org`. Use the
reference URL supplied by the export; do not synthesize a URL. Do not display
Paizo PRD, DRP, or other external item references. Keep optional external-link
attributes (`target="_blank"` and `rel="noopener noreferrer"`) on Wiki links.

This is a presentation rule: retain imported references in the domain model so
the data contract does not discard information.

### French spell labels

Add a `CatalogText` formatter for known component values:

| Export value | Display label |
| --- | --- |
| `Verbal` | Verbale |
| `Somatic` | Somatique |
| `Material` | Matérielle |
| `Focus` | Focaliseur |
| `DivineFocus` | Focaliseur divin |
| `FocusOrDivineFocus` | Focaliseur ou focaliseur divin |

Match known values case-insensitively and preserve unknown values unchanged.
Render English spell-name localizations such as `en-US:name` with the display
label `Nom VO`, rather than exposing the technical localization key.

### Homepage and edition identity

Show feat, spell, and monster counts in the main navigation alongside their
catalog links. Read counts from the shared data-load status; do not present
empty initial-load counts as if they were a successfully loaded catalog.

Remove the separate prominent “État du catalogue” panel from the homepage.
Display the data version in a compact technical footer on the homepage.
Preserve a visible error/warning signal when data is unavailable or loading
failed; moving status information must not hide a degraded state.

Identify the current content as **Pathfinder première édition** in the shared
site header and homepage. This change is a clear label only; it does not add
second-edition data, edition selection, or multi-edition data architecture.

## Data flow and component boundaries

`PathfinderDataLoader` continues to import and validate source data without
discarding references or changing JSON contracts. `DataSnapshot` creates the
level index alongside existing immutable indexes. `CatalogService` exposes
level buckets and paginated level queries. Razor Page models provide those
queries to the selector and result page. Shared Razor partials format each
domain item using `CatalogText`; the detail routes continue to use their
complete existing item models.

The shared layout reads catalog counts from the existing snapshot provider.
The homepage continues to read `DataLoadStatus` for the data version and
degraded-state message.

## Error handling and compatibility

- Preserve the current 404 behavior for missing buckets and invalid pages.
- Do not replace the current snapshot or invalidate catalog caches when data
  loading or validation fails.
- A missing Wiki reference means no origin link is rendered; no fallback or
  generated URL is shown.
- Unknown component values and labels remain visible using their original
  value.
- Existing detail URLs remain valid and continue to be linked from catalog
  entries.
- Keep the existing 50-item page size and catalog cache policy.

## Validation

- Unit-test `DataSnapshot` and `CatalogService` for level discovery, grouping,
  duplicate assignments, pagination, and invalid level/page behavior.
- Unit-test component and localization formatting, including unknown-value
  fallback.
- Verify list partials show the agreed fields and link to the detail route;
  verify only Pathfinder-fr.org references are rendered.
- Update route/rendering tests for alphabetical return links, the level
  selector and route, header counts, first-edition identity, homepage version
  placement, and visible degraded-state reporting.
- Run the modern build and unit-test suite. Keep the real-clone integration
  test opt-in and use it when the local `pf1-data` clone is available.
- Update `docs/modern-data-pipeline.md` to describe spell-level browsing,
  list summaries, and Wiki-only presentation of item-level links.
