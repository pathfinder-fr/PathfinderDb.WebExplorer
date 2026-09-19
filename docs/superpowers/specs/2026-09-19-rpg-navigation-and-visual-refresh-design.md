# Pathfinder FR DB visual refresh and deterministic content navigation

## Context

The modern ASP.NET Core 10 site already loads the JSON exports from
`pf1-data`, exposes paginated catalog routes, and keeps the legacy MVC
application beside it. The current UI is intentionally minimal, but its
homepage presents operational details before reader-oriented navigation and
does not reuse the existing Pathfinder FR DB logo. The current data snapshot
also has indexes for monster metadata but not for the deterministic spell and
feat dimensions requested by role-playing users.

## Goals

- Make the homepage useful to role-players before exposing technical status.
- Establish a warmer Pathfinder visual identity while keeping the site
  server-rendered, lightweight, accessible, and responsive.
- Reuse `src/PathfinderDb.WebExplorer/Content/images/pf-fr-db.png` in the
  modern application.
- Add bounded, paginated navigation for spell school, spell class/list, spell
  source, feat type, and feat source.
- Keep navigation deterministic: no unbounded search or combinatorial filters.
- Preserve existing cache headers, output-cache tagging, snapshot atomicity, and
  degraded startup behavior.

## Non-goals

- No frontend framework or client-side rendering.
- No free-form search or arbitrary combinations of filters in this increment.
- No change to the legacy MVC application.
- No monster stat-block work; the current source export contains metadata only.
- No hard-coded spell-class enumeration; values remain data-derived.

## Homepage and visual design

The homepage becomes a reader-facing portal:

- a branded hero/header using the existing `pf-fr-db.png` image;
- a short role-player-oriented introduction;
- three prominent entry cards for Dons, Sorts, and Monstres;
- secondary navigation cues to the available deterministic dimensions;
- technical snapshot state, version, counts, warnings, and errors moved below
  the reader-oriented content in a visually subdued information section.

The shared CSS keeps system fonts and adds a serif stack for headings only,
such as `Georgia, Cambria, "Times New Roman", serif`. Body text remains a
system sans-serif stack. The palette moves from the current cool dashboard
appearance toward ivory/parchment surfaces, dark brown ink, muted gold, and
the existing brick accent. Existing focus-visible styles and responsive
behavior remain required.

The PNG is copied into the modern application's static assets without editing
the source image. It must have meaningful alternative text and remain
responsive without causing layout overflow.

## Deterministic navigation

The current `/dons` and `/sorts` landing pages stop opening a default first
bucket. They become selection pages listing available dimensions. Existing
detail and paginated routes remain compatible.

New canonical routes:

| Content | Dimension | Route |
|---|---|---|
| Spells | school | `/sorts/ecole/{school}` |
| Spells | class/list | `/sorts/classe/{class}` |
| Spells | source | `/sorts/source/{source}` |
| Feats | type | `/dons/type/{type}` |
| Feats | source | `/dons/source/{source}` |

Every dimension page uses the existing fixed page size of 50, the existing
404 behavior for unknown buckets and pages, and the existing cache policy.
Bucket names are compared case-insensitively while links use a canonical
display value from the snapshot.

`DataSnapshot` adds immutable indexes:

- spells by school;
- spells by level/list value, including dynamically appearing values such as
  `psychiste`;
- spells by source;
- feats by type;
- feats by source.

The service exposes bucket lists and paginated accessors following the
existing monster type/source patterns. Empty or missing optional metadata is
not indexed and does not create an empty-success bucket.

## Error handling and compatibility

Unknown dimensions, buckets, slugs, and page numbers return HTTP 404. Invalid
initial data remains degraded, and an invalid later reload preserves the
previous valid snapshot. No new route may expose an empty catalog as a
successful result.

The shared layout and cache behavior remain unchanged except for the visual
identity and navigation links. Existing monster routes and feat/spell detail
routes remain available.

## Testing and validation

- Add snapshot/service tests for every new index and for unknown buckets/pages.
- Add route/page tests for representative school, class, source, and feat-type
  links and 404 responses.
- Add a homepage rendering test proving technical status appears after the
  reader-facing navigation.
- Verify the copied logo is served as a static asset.
- Run the full unit suite, real-clone integration suite, Release build, and
  `git diff --check`.
- Run the site against the real clone and manually verify the homepage plus
  one page for each new navigation dimension.

## Increment order

1. Add the asset, homepage structure, and visual theme.
2. Add immutable snapshot indexes and service accessors.
3. Add Razor Pages and links for the new deterministic dimensions.
4. Add route/rendering tests and complete real-clone HTTP verification.
