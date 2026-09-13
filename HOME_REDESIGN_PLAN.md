# MusicEncyclopedia — B08 Home Redesign Plan

Updated: 2026-09-11. Status: **Done**. This plan records the implemented product decisions and current B08 evidence. `IMPLEMENTATION_PLAN.md` remains the authoritative backlog.

## Approved direction

- The launch UI is Persian (`fa`) only and uses RTL layout with self-hosted Vazirmatn. There is no language switcher. Existing `en`/`ar`/`fr` resources and redirects remain dormant infrastructure for a future milestone.
- Keep the current minimal, content-first encyclopedia style: white surfaces, quiet borders, strong hierarchy and link blue `#2563EB`. Do not replace it with a vibrant, decorative or horizontal-scroll layout.
- The home page is album-first: a compact search banner, category chips, album grid, random-poem card and an About card.
- Category is the only catalog filter on home.
- Prefer random poems connected to a sung-version track and album; follow the fallback chain below when no complete match exists.
- Use hybrid catalog navigation: render 20 albums, append pages 2–5, then navigate from page 6 onward.

## Current implementation

`HomeController`, `HomeViewModel`, `HomeConstants`, the home Razor views, `home.js`, home CSS and the Persian resource strings already exist. B08 must verify and repair this implementation rather than rebuild it.

Known gaps from the 2026-09-11 audit:

- The shared pagination renders pages 1–5 after those pages have already been appended, creating a confusing and potentially duplicate handoff.
- Fetch failures do not provide a clear retry path, and load results need a reliable live-region announcement.
- JavaScript is required to progress through the first 100 items; a no-JavaScript path is required.
- The Home view writes `ViewData["MetaDescription"]`, while the layout reads `ViewData["Description"]`.
- The About action links back to the current home page. Remove it unless a real About route is added.
- There are no focused SQL-backed home boundary tests or current browser evidence.

## Catalog contract

`HomeConstants` is the single source of truth:

- `InitialPageSize = 20`
- `LazyLoadCap = 100`
- `PageSize = 20`

The exact behavior is:

1. `GET /fa` renders page 1, containing up to 20 albums.
2. Four append actions request pages 2, 3, 4 and 5. Reaching page 5 yields at most 100 loaded albums.
3. If more than 100 filtered albums exist, the tail navigator exposes page 6 through the last page. It must not expose pages 1–5 as new destinations after the append phase.
4. A direct request for `?page=6` or greater renders that page server-side and retains the active category.
5. With JavaScript disabled, standard server-rendered navigation must make every page reachable. This can be a `<noscript>` navigator or a progressively enhanced anchor whose normal target is the next server page.
6. Changing category resets the catalog to page 1 and recalculates totals, counts and the tail-page range.

The shared `_Pagination.cshtml` may be extended with a minimum-page option if that remains useful to other lists. Otherwise add a home-specific tail pagination partial. Do not use the shared partial unchanged for the post-100 handoff.

## Data and cache rules

- Reuse `IAlbumQueryService` for filtered pages and totals.
- Category counts include only visible, non-deleted albums and remain correct for empty categories.
- Cache deterministic catalog, lookup and About-stat data through the existing application cache and B05 invalidation paths.
- Do not add positive ASP.NET `ResponseCache` caching to mutable home or partial responses. It cannot be evicted by the application cache invalidator.
- Resolve the random poem outside deterministic home caching on each request. Random selection can legitimately repeat the same poem on consecutive requests.
- Random poem fallback order is: poem with linked track and album; poem with linked track; any visible poem; hide the card.

## Interaction and accessibility

- Keep category chips as normal links so filtering works without JavaScript. The active chip uses `aria-current` and a visible non-color-only state.
- While loading, disable the append control and show an inline busy state without moving focus.
- After success, announce the number appended and the new shown/total count through one `role="status"` or polite `aria-live` region.
- After failure, preserve the already loaded cards, show a visible Persian error message and make the same control available to retry.
- Preserve visible focus, keyboard operation, reduced-motion behavior and a minimum 44px touch target where practical.
- Avoid horizontal carousels. The album grid wraps responsively and must have no page-level horizontal overflow at 375px.

## Content and metadata cleanup

- Use one metadata key consistently between Home and `_Layout.cshtml`; prefer `ViewData["Description"]` because the layout already consumes it.
- Keep the About card self-contained. Remove its home self-link unless a separate About page is implemented and tested.
- Keep category chips inline in `Index.cshtml` unless extraction creates real reuse or makes the view materially easier to test. The previously proposed `_CategoryChips.cshtml` is not a required artifact.
- Keep all user-facing B08 strings in the Persian resource file. Other resource files may remain for future localization, but their completeness is outside launch acceptance.
- SEO under B08 preserves canonical metadata. B12 owns verification of canonical plus `fa` and `x-default` alternates.

## Implementation sequence

1. Add focused SQL-backed home tests for 0, 20, 21, 100 and 101+ album datasets, including a filtered category.
2. Repair the hybrid handoff and server-rendered deep links while keeping existing album card markup.
3. Add the no-JavaScript path, visible retry state and live-region announcements.
4. Fix the metadata key and remove the About self-link.
5. Verify the random-poem fallback, shuffle endpoint and deterministic versus uncached boundaries.
6. Run the affected automated suites, then inspect `/fa` at 375, 768, 1024 and 1440 pixels with keyboard and reduced-motion checks.
7. Record evidence in `docs/ACCEPTANCE_CHECKLIST.md` and change B08 to Done only after every item below passes.

## Required automated evidence

| Dataset | Expected behavior |
|---|---|
| 0 albums | Persian empty state; no append control or tail pagination |
| 20 albums | One server-rendered page; no append control or tail pagination |
| 21 albums | Page 1 plus one append; no tail pagination |
| 100 albums | Page 1 plus exactly four appends; no tail pagination |
| 101+ albums | Page 1 plus exactly four appends; tail pagination begins at page 6 |

Also verify category counts and reset, invalid category handling, direct page 6+, soft-delete visibility, cache refresh after an admin write, poem fallback states, shuffle success/failure, and metadata rendering.

## Definition of done

- The first 100 results contain no duplicates or gaps, and page 6 begins with result 101 for the same sort and category.
- Every result remains reachable with JavaScript disabled and by direct URL.
- Loading, success, empty and failure states are visible and accessible in Persian.
- Random-poem selection is uncached while deterministic home data follows B05 invalidation.
- The About card has no self-link, and the rendered meta description is populated.
- `/fa` passes RTL, Vazirmatn, keyboard, reduced-motion and responsive checks at 375/768/1024/1440.
- Relevant automated tests and the solution build pass with no new warnings.

## Completion evidence

- Eight focused SQL-backed tests cover 0/20/21/100/101 albums, category filtering/counts, append-window validation, direct page 6, duplicate/gap prevention, cache refresh after deletion and every random-poem fallback.
- The complete Release suite passes 82 tests: Core 5, Services 42, Data 1 and Web 34, with zero failures or skips.
- A live disposable LocalDB run was inspected at 375, 768, 1024 and 1440 pixels. Every width rendered `lang="fa"`, `dir="rtl"` and Vazirmatn without horizontal overflow. The layout stacks below 960px and uses the sticky 300px sidebar above that breakpoint.
- Search, category chips, mobile navigation and poem shuffle measure 44–48px after explicit protection from the legacy Tailwind runtime reset.
- The existing two EF1002 seed warnings and two duplicate-resource warnings remain assigned to B13/B10; B08 introduced no new warnings.

## Out of scope

- Multilingual UI, a language switcher and LTR acceptance are deferred.
- The broad public/admin token migration belongs to B09.
- Cross-site accessibility and RTL verification belongs to B10.
- Tailwind build tooling belongs to deferred B15 unless B12 performance or CSP evidence promotes it.
