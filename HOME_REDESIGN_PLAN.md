# Music Encyclopedia — Home Page (Landing) Redesign Plan

> Status: **Approved** — 4 clarifying decisions locked with the user · Prepared with **ui-ux-pro-max** design intelligence
> Goal: Turn the public home page into a **clean, minimal, album-first landing page** — simple banner, search box, filterable album catalog with lazy loading → pagination, a random-poem sidebar, and an About Us section.
> The design MUST reuse the existing token system (link blue `#2563EB`, white cards on `#F8FAFC`, **Vazirmatn in `fa`/`ar` RTL**, Inter in `en`/`fr` LTR) already established by `UI_REDESIGN_PLAN.md`.

---

## 0. Locked Decisions (from user)

| # | Question | Decision |
|---|----------|----------|
| 1 | Lazy-loading UX | **"Load more" button** — show 20 albums, click to append the next 20 |
| 2 | More than 100 albums | **Pagination takes over** — lazy-load the first 100 (5 × 20), then numbered pagination for the remaining albums |
| 3 | Filters | **Categories only** — album category chip buttons (Studio, Live, Compilation, …) |
| 4 | Sidebar random poem | **Prefer poems with links** — only pick poems that have ≥1 sung-version track and that track's album; show poem + track + album links |

Constants (single source of truth, `HomeConstants`):
- `InitialPageSize = 20`
- `LazyLoadCap = 100` (5 loads of 20)
- Pagination page size after the cap: `20` (pages start at 6 when catalog > 100)

---

## 1. Executive Summary

The current home page (`Views/Home/Index.cshtml`) is a **content-hero page**: search hero, featured/latest album sections, category cards, essential tracks, featured poems. The new home page is an **album-first landing page**:

```
┌───────────────────────────────────────────────────────────────┐
│  BANNER  (brand statement + search box + hint chips)          │
├───────────────────────────────┬───────────────────────────────┤
│  ALBUM CATALOG                │  SIDEBAR                      │
│  ─ Category filter chips      │  ─ Random Poem card           │
│    (All · Studio · Live · …)  │    (poem + track + album)     │
│  ─ Album card grid (20)       │  ─ About Us card              │
│  ─ [Load more] button → 100   │                               │
│  ─ Numbered pagination (>100) │                               │
└───────────────────────────────┴───────────────────────────────┘
```

Everything is server-rendered Razor (no SPA) to stay consistent with the codebase. Lazy loading uses a lightweight partial-endpoint (`Home/AlbumGrid`) that returns more album cards for the same filters — keeping localization and the existing cache layers intact.

---

## 2. Design Direction (ui-ux-pro-max)

Product type analysis — **Wiki / Encyclopedia / Catalog**:

- **Style:** Minimalism + Flat Design (matches the existing redesign)
- **Landing pattern:** Search-First + Hierarchical Navigation — search box stays prominent in the banner
- **Palette:** clean white surfaces, **link blue** accent `#2563EB`, strong heading hierarchy, muted citation grey — already the live token set in `wwwroot/css/site.css`
- **Fonts:** Vazirmatn (`fa`/`ar` RTL) / Inter (`en`/`fr` LTR) — already self-hosted via `fonts.css`
- **Anti-patterns to avoid:** decorative-only motion, text-heavy sections, color-only status, emoji-as-icons

Derived decisions for this page:

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Banner | Slim brand band: eyebrow + H1 + tagline + search box + hint chips | Search-first; minimal; keeps brand statement without a huge hero |
| Album grid | Reuse existing `.card` / `.card-grid` components | Consistency; zero new visual language |
| Filter chips | Pill buttons (`.tag`-based), active state = accent | Category filter is the ONLY filter — chips beat dropdowns for single-dimension filtering |
| Load more | Full-width secondary button with count hint ("Show 20 more · 40 of 132") | Predictable; no surprise scroll-jacking |
| Pagination | Reuse existing `_Pagination` partial after cap | Already localized + RTL-aware |
| Sidebar | Sticky right rail (inline-end) on desktop, stacks below grid on mobile | Standard 2-column catalog layout |
| Random poem | Server-picked per request, **never cached** | Requirement: "randomly by each page load" |
| About Us | Card with icon, localized heading + 2 short paragraphs + stats line | Simple, honest, low-effort |

---

## 3. Page Anatomy & Spec

### 3.1 Banner (top, full width)
- Eyebrow label: "The Open Music Reference" (localized)
- H1: site name (existing `Music Encyclopedia` key)
- One-line tagline (existing hero subtitle key)
- Search form → `/{culture}/search?q=...` (existing search box markup reused)
- Hint chips: 4 genres + 3 moods (existing `Genres/Moods` data already loaded by the controller)

### 3.2 Album Catalog (main column)
- **Header row:** H2 "All Albums" (localized) + results count ("132 albums")
- **Category filter chips:** `All` + every `AlbumCategory` (Code → Name). Active chip = accent-soft bg + accent border. Chips are links: `/?category={code}`.
- **Grid:** 20 `AlbumListItemDto` cards (reuse `.card` from `Views/Albums/Index.cshtml` — cover, title, original title, category badge, artist, year, genre tags).
- **Load more:** visible while `loadedCount < min(TotalItems, 100)`. Click → JS `fetch` → `GET /{culture}/home/albums?page={n}&category={code}` → returns `_AlbumCardGrid` partial → append. Button text updates the running count. Disabled + spinner while in flight.
- **Pagination:** when `TotalItems > 100`, after the cap is reached the button is replaced by the shared `_Pagination` partial (pages 6…N). Pages 1–5 stay lazy (deep-link to page ≥ 6 loads directly).
- **Empty state:** reuse `.empty` markup ("No albums found in this category").

### 3.3 Sidebar (secondary column)
- **Random Poem card:**
  - Heading: "Poem of the Moment" (localized) + refresh hint
  - Poem title → `/{culture}/poems/{slug}`
  - Poet name (if any)
  - Divider
  - "Featured in" → Track row (title → `/{culture}/tracks/{slug}`) and Album link (title → `/{culture}/albums/{slug}`)
  - Selection rule: `ORDER BY RANDOM() LIMIT 1` over poems that have ≥1 `SungVersion` with a `TrackSungVersion`, joined to that track's first album via `AlbumTrack`. Loaded **per request, outside the home cache**.
- **About Us card:**
  - Icon + "About Us" heading
  - 2 localized short paragraphs (mission + scope)
  - Stats line: "N albums · M tracks · K people" (real counts via 1 aggregated query)
  - Optional "Learn more" → `/about` if the page exists (skip if not; keep card self-contained)

---

## 4. Technical Design

### 4.1 Data layer

**Categories** — `AlbumCategory` table (`AlbumCategoryId`, `Code`, `Name`). New Dapper query in `HomeController` (pattern already used for genres/moods):
```sql
SELECT ac.Code, ac.Name, COUNT(a.AlbumId) AS AlbumCount
FROM AlbumCategory ac
LEFT JOIN Album a ON a.AlbumCategoryId = ac.AlbumCategoryId AND a.IsDeleted = 0
WHERE ... 
GROUP BY ac.Code, ac.Name
ORDER BY ac.Name
```

**Album catalog** — reuse `IAlbumQueryService.GetAlbumsAsync(culture, page, pageSize: 20, category: code, sort: ...)` → already returns `PagedResult<AlbumListItemDto>` with `TotalItems/TotalPages`. **No service changes needed.**

**Random poem + track + album** — new Dapper query (in `HomeController` or a small `HomeQueryService`):
```sql
SELECT p.Slug AS PoemSlug, p.Title AS PoemTitle, pers.FullName AS Poet,
       t.Slug AS TrackSlug, t.Title AS TrackTitle,
       a.Slug AS AlbumSlug, a.Title AS AlbumTitle
FROM Poem p
LEFT JOIN Person pers ON pers.PersonId = p.PersonId AND pers.IsDeleted = 0
INNER JOIN SungVersion sv ON sv.PoemId = p.PoemId AND sv.IsDeleted = 0
INNER JOIN TrackSungVersion tsv ON tsv.SungVersionId = sv.SungVersionId
INNER JOIN Track t ON t.TrackId = tsv.TrackId AND t.IsDeleted = 0
LEFT JOIN AlbumTrack at ON at.TrackId = t.TrackId
LEFT JOIN Album a ON a.AlbumId = at.AlbumId AND a.IsDeleted = 0
WHERE p.IsDeleted = 0
ORDER BY RANDOM()   -- SQL Server: NEWID()
LIMIT 1
```
Fallback chain: (1) poem with track+album; (2) if none, poem with track only (album links hidden); (3) if none, any poem; (4) hide card.

**About stats** — 1 query: `(SELECT COUNT(*) FROM Album WHERE IsDeleted=0) + '·' + (Track) + '·' + (Person)` (three scalar counts).

### 4.2 View models

`HomeViewModel` changes:
```csharp
public sealed class HomeViewModel
{
    string CurrentCulture, MetaDescription, MetaKeywords, CanonicalUrl;   // keep
    IReadOnlyList<BrowseLink> Genres, Moods, Instruments;                 // keep (banner chips)
    // NEW:
    IReadOnlyList<AlbumCategoryChip> Categories;                          // filter chips + counts
    PagedResult<AlbumListItemDto>? Albums;                                // first 20 (or direct deep page)
    RandomPoemCard? RandomPoem;                                           // per-request, NOT cached
    AboutStats About;                                                     // counts
}

public sealed class RandomPoemCard
{ string PoemSlug, PoemTitle, Poet?, TrackSlug?, TrackTitle?, AlbumSlug?, AlbumTitle?; }

public sealed class AlbumCategoryChip
{ string Code, Name; int AlbumCount; }

public sealed class AboutStats { int Albums, Tracks, People; }
```

### 4.3 Controller & routes

- `GET /{culture}` (existing `Index`) — loads: categories, page-1 albums (or deep-linked page), genres/moods for banner, about stats, **random poem per request**.
- `GET /{culture}/home/albums?page=N&category=C` (NEW) — returns `PartialView("_AlbumCardGrid", ...)`; `[ResponseCache(Duration=300, VaryByQueryKeys=["page","category"])]`.
- `GET /{culture}/home/random-poem` (NEW, optional) — returns JSON for an optional "shuffle" button that re-rolls the poem without a full reload.

### 4.4 Caching strategy (critical)

The current home cache (`home:{culture}`, 5 min) caches the **whole** `HomeViewModel`, which would freeze the random poem. Restructure:

1. **Cache** the deterministic parts only — categories, genres/moods, about stats, and the first-page album result — under a *new* key `home:data:{culture}:{page}:{category}` (or reuse `CacheKeys.List("album", ...)` for the album page itself, which already exists, plus a `CacheKeys.Lookup("home-categories")`).
2. **Never cache** the random poem — resolved on every request (1 indexed-ish query, trivial cost).
3. Keep `[ResponseCache]` off for the home HTML action (random content); rely on the partial/API caches.

### 4.5 JS (site.js or new `home.js`)

- **Load more:** on click → GET partial endpoint → `insertAdjacentHTML` into `#album-grid` → update counter + hide button when `loaded >= cap` → swap in pagination partial if `TotalItems > cap`. Spinner + `disabled` during fetch; `aria-live` region announces "loaded X more".
- **Filter chips:** plain links (no JS needed) — but keep `?page=` reset when category changes.
- **Shuffle poem (optional):** fetch `/home/random-poem` JSON and swap card content.
- All animations guarded by `prefers-reduced-motion` (existing rule).

### 4.6 Views & partials

| File | Action |
|------|--------|
| `Views/Home/Index.cshtml` | Rewrite: banner + 2-column layout (catalog + sidebar) |
| `Views/Home/_AlbumCardGrid.cshtml` | NEW — grid + cards for initial render AND load-more endpoint |
| `Views/Home/_RandomPoemCard.cshtml` | NEW — sidebar poem card |
| `Views/Home/_AboutCard.cshtml` | NEW — about + stats |
| `Views/Home/_CategoryChips.cshtml` | NEW — filter chip row |
| `Views/Shared/_Pagination.cshtml` | Reuse (exists) |

### 4.7 Localization & RTL

New `SharedResources` keys (add to neutral + `fa`/`ar`/`fr` resx; **Persian text in `fa`**):
`All Albums`, `{0} albums`, `Show more`, `Showing {0} of {1}`, `All`, `Poem of the Moment`, `Featured in`, `About Us`, mission + scope paragraphs, `{0} albums · {1} tracks · {2} people`, `Load more albums`, `No albums found in this category`, etc.

RTL: layout uses logical properties (`padding-inline`, `inset-inline-end`) — sidebar sits on the **right in LTR, left in RTL** automatically. Chips wrap naturally. Directional "more" chevron flips via the existing `[dir="rtl"] .icon-directional { scaleX(-1) }` rule. Fonts handled by the existing `html[dir="rtl"]` rules (Vazirmatn).

---

## 5. Implementation Phases

### Phase A — Data & ViewModel
- [ ] Add `HomeConstants` (20 / 100 / 20)
- [ ] Extend `HomeViewModel` + new DTOs (`RandomPoemCard`, `AlbumCategoryChip`, `AboutStats`)
- [ ] Add category-count query, random poem query, about stats query to `HomeController`
- [ ] Split cache: deterministic data under new keys; random poem per request
- **Accept:** `dotnet build` clean; home loads with correct first 20 + categories + a random poem that changes on refresh

### Phase B — Views & partials
- [ ] Rewrite `Home/Index.cshtml` (banner + 2-column layout)
- [ ] Add `_AlbumCardGrid`, `_RandomPoemCard`, `_AboutCard`, `_CategoryChips` partials
- [ ] Category chips wired as `?category=` links; active state
- [ ] Empty state for filtered categories
- **Accept:** page renders in `fa` (Vazirmatn, RTL) and `en` (Inter, LTR); no horizontal scroll at 375px

### Phase C — Lazy load + pagination
- [ ] Add `GET /{culture}/home/albums` partial endpoint (+ response cache)
- [ ] `home.js`: load-more button → fetch → append → counter → cap → pagination swap
- [ ] Deep-link support for page ≥ 6 (server renders page directly)
- **Accept:** with > 100 albums in DB: 5 clicks load 100, then numbered pagination appears and works; with ≤ 100 albums: button disappears at the end, no pagination

### Phase D — Sidebar & About
- [ ] Random poem card (poem + track + album links; fallback chain)
- [ ] About Us card + stats counts
- [ ] (Optional) shuffle-poem JSON endpoint + button
- **Accept:** poem changes on every reload; links open the right detail pages; stats match DB

### Phase E — QA, localization, polish
- [ ] All new strings in 4 resx files; `fa` verified Persian
- [ ] RTL/LTR pass, responsive (375/768/1024/1440), keyboard nav, focus rings, reduced-motion
- [ ] Accessibility (ui-ux-pro-max checklist): aria-live on load-more, aria-label on chips/button, contrast ≥ 4.5:1
- [ ] `dotnet build` + all tests green; SEO meta/hreflang/JSON-LD intact
- [ ] Update `design.md` / plan status

---

## 6. Validation Commands

```bash
dotnet build MusicEncyclopedia.sln
dotnet test tests/MusicEncyclopedia.Core.Tests
dotnet test tests/MusicEncyclopedia.Services.Tests
dotnet test tests/MusicEncyclopedia.Web.Tests
dotnet run --project src/MusicEncyclopedia.Web   # http://localhost:5240/fa and /en
```

Browser checks (browser-use):
1. `/fa` → RTL, Vazirmatn, banner + chips + grid + sidebar; poem changes on refresh
2. Click chips → filtered grid + count + active state
3. Click "Load more" ×5 → 100 albums → pagination appears; page 6 renders server-side
4. Poem card links → correct poem/track/album detail pages
5. No console errors; no horizontal scroll

---

## 7. Acceptance Criteria (Definition of Done)

- [ ] Home shows **all albums** (first 20, no curation sections like "Featured/Latest")
- [ ] **Category chips filter** the grid server-side; active chip visible; counts shown
- [ ] **20 → Load more → 100 → pagination** exactly as decided (incl. >100 and ≤100 cases)
- [ ] **Search box** in banner submits to the search page; hint chips present
- [ ] **Random poem** sidebar card changes per page load and links to a track + album (fallback rules honored)
- [ ] **About Us** card with localized text + live stats
- [ ] `fa`/`ar` render **Vazirmatn** RTL; `en`/`fr` render Inter LTR; logical-property layout
- [ ] Home HTML is **not** fully cached (poem randomness); album grid partial cached
- [ ] Build clean, all tests green, WCAG AA, responsive 375–1440, reduced-motion respected, SEO tags untouched

---

## 8. Risks & Notes

| Risk | Mitigation |
|------|------------|
| Random poem per request hits DB every load | Single cheap query (join + LIMIT 1); SQLite/SQL-Server-safe; optional `ORDER BY RANDOM()` cost acceptable at catalog scale; shuffle endpoint is the only alternative re-roll |
| Lazy load vs. page deep-links | `page` + `category` fully server-rendered; JS only appends; bookmarking and back button work |
| Caching freezes the poem | Poem explicitly excluded from all caches (Section 4.4) |
| >100-catalog edge | Cap + pagination swap logic tested in Phase C; counts come from `TotalItems` |
| Localization completeness | All new keys added to 4 resx files in Phase E with a grep check for hardcoded English in the new views |
