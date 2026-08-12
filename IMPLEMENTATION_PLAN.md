# MusicEncyclopedia — Implementation Plan

> Plan to close every gap identified in the codebase audit (see conversation history).
> Spec of record: `music-encyclopedia-q.md` (§24 Milestones, §25 DoD, §35 Future Enhancements, §36 Acceptance Criteria).
> Current state: **Phases 0–3 COMPLETE** (build green, core repaired, 4 cultures, full admin CRUD incl. Users/Roles/Lookup Tables/Audit Log/Settings + bulk soft delete). **Remaining: Phases 4–8 below.**

---

## Progress status

| Phase | Status |
| --- | --- |
| 0 — Build & Baseline | ✅ DONE |
| 1 — Repair Broken Core | ✅ DONE |
| 2 — Localization & Multi-Language | ✅ DONE |
| 3 — Complete Admin CRUD | ✅ DONE |
| **4 — Advanced Encyclopedia Features** | ⏳ **REMAINING** |
| **5 — Performance, Search & Caching** | ⏳ **REMAINING** |
| **6 — Hardening & Deployment** | ⏳ **REMAINING** |
| **7 — Testing** | ⏳ **REMAINING** |
| **8 — Acceptance & Polish** | ⏳ **REMAINING** |

> **Next up: Phase 4.** See each phase's **Goal → Touch → Verify** and **Definition of Done** below.

---

## How to use this plan

- **Phases are ordered by dependency and user impact.** Do not skip Phase 0 — nothing can be verified until the build works.
- Each task lists: **Goal → Touch → Verify**. "Touch" names the concrete files to change.
- Every phase has a **Definition of Done**. A phase is complete only when its DoD passes.
- Work is safe to parallelize where noted; everything else is sequential.
- All public pages must respect spec §25 (Definition of Done for Every Feature): parameterized SQL, DTOs only (never EF entities in views/API), server-side validation, authorization, soft delete, localization fallback, SEO tags, no broken links.

---

## ✅ Phase 0 — Build & Baseline (COMPLETE)

**Objective:** Make the solution compile and run on this machine so all later phases can be verified.

### 0.1 Fix the SDK pinning blocker
- **Goal:** `dotnet build` succeeds on this machine (only .NET SDK 9.0.316 / 10.0.302 installed; `global.json` pins 8.0.401 with `rollForward: latestFeature`, which does not cross major versions).
- **Touch:** `global.json` — change `"rollForward": "latestFeature"` → `"latestMajor"` (SDK 9/10 can compile `net8.0` targets; do **not** change any TFM).
  - Alternative (if the team insists on SDK 8.0.x): install .NET SDK 8.0.401 instead. Prefer `latestMajor`.
- **Verify:** `dotnet build MusicEncyclopedia.sln --nologo` → 0 errors.

### 0.2 Baseline run & bug inventory
- **Goal:** App starts, `/health` responds, seed data present; record known-broken behavior to fix in Phase 1.
- **Touch:** none (observation only).
- **Verify:** `dotnet run --project src/MusicEncyclopedia.Web` then check:
  - `GET /health`, `GET /fa/albums`, `GET /fa` (home — expected empty sections), `GET /fa/search?q=x` (expected to throw: schema mismatch), nav links (expected 404s).

### 0.3 Test infrastructure smoke test
- **Goal:** Verify the xUnit runner actually works before writing real tests in Phase 7.
- **Touch:** `tests/MusicEncyclopedia.Core.Tests/UnitTest1.cs` → replace with a real smoke test (e.g., `SlugService` returns a URL-safe slug).
- **Verify:** `dotnet test` passes for Core.Tests.

**DoD:** Solution builds; app runs; one real test executes green.

---

## ✅ Phase 1 — Repair Broken Core (COMPLETE)

**Objective:** Fix the three things that make the public site unusable today.

### 1.1 Fix SearchService schema mismatch (search currently throws)
- **Goal:** Search works against the real schema in both SQLite and SQL Server modes.
- **Current bug:** `src/MusicEncyclopedia.Search/Services/SearchService.cs` generates SQL referencing nonexistent columns/tables: `a.Id`, `t.Id`, `p.Id`, a `Culture` column on every table, and an `AlbumInstrument` join table. Real schema uses `AlbumId/TrackId/PersonId/...` PKs, `EntityId`-based polymorphism, and joins `AlbumGenre`/`AlbumMood`/`TrackGenre`/`TrackMood`/`TrackInstrument`.
- **Touch:** `SearchService.cs` (`BuildSearchFragments`, `MakeFragment`) —
  - Correct PK column per entity (`AlbumId`, `TrackId`, `PersonId`, `CompanyId`, `PoemId`, `SungVersionId`, `GenreId`, `MoodId`, `InstrumentId`, `TagId`, `AliasId`, `LocalizationId`).
  - Remove the `Culture` column from every fragment; filter by culture via `Alias`/`Localization` joins (or `Language` code) instead.
  - Fix facet joins to real tables (`AlbumGenre`, `TrackGenre`, `AlbumMood`, `TrackMood`, `TrackInstrument`).
  - Add missing searchable entities: `SungVersion` (already), plus `Publication`, `RecordingSession`, `PerformanceEvent`, `Location`, `Source` per spec §9.21.
  - Keep the SQL Server `FREETEXTTABLE` path but align the indexed-column list with `src/MusicEncyclopedia.Data/Scripts/FullTextSearch.sql`.
  - Ensure the UNION-ALL count query is valid on SQLite (no `COUNT_BIG` incompatibilities).
- **Verify:** `GET /fa/search?q=<existing seeded title>` returns results in dev (SQLite). Smoke-check the generated SQL shape.

### 1.2 Wire up the home page
- **Goal:** Home page shows real data (currently `HomeController.Index` returns an empty `HomeViewModel`).
- **Touch:**
  - `Controllers/HomeController.cs` — inject query services; populate `FeaturedAlbums`, `LatestAlbums`, `EssentialTracks`, `FeaturedPoems`, and browse-by `Genres/Moods/Instruments`.
  - `Services/` — add `IHomeDataService` (or extend existing query services) returning the home aggregates: featured/latest albums (sort by release/CreatedAt, take N), essential tracks, featured poems, lookup lists.
  - `Views/Home/Index.cshtml` — already renders sections; keep markup, ensure model populated.
- **Verify:** `/fa` renders populated sections; caching per culture (spec §9.1: 5 min).

### 1.3 Seed sample content
- **Goal:** Public pages are browsable with realistic data (currently only lookup tables are seeded).
- **Touch:** `src/MusicEncyclopedia.Data/Seed/DatabaseInitializer.cs` (or new `SeedContent.cs`) — idempotently insert: 1 main artist (Person), 2–3 albums with categories, tracks with `AlbumTrack` ordering (2 discs to exercise grouping), a poem + sung version, credits (primary artist, musician+instrument, producer), a tag, an alias, a couple of citations/sources, minimal media rows (URL placeholders). Must set both `Entity` + subtype rows (polymorphic 1:1).
- **Verify:** `/fa/albums`, `/fa/albums/{slug}` (tracklist grouped by disc), `/fa/tracks/{slug}`, `/fa/poems`, `/fa/people/{slug}`, `/fa/search?q=...` all render data. Re-running the app does not duplicate seed rows.

### 1.4 Fix navigation & routing 404s
- **Goal:** No broken nav links.
- **Current bug:** `Views/Shared/_Layout.cshtml` (desktop + mobile nav) links to `/Albums`, `/Artists`, `/Poets`, `/People` without the culture prefix; controllers are attribute-routed under `{culture}/...`. Also `/Artists` has no controller.
- **Touch:**
  - `_Layout.cshtml` — build links via `Url.Action` (culture-aware) or `/@culture/...`; keep the current culture in links.
  - Decide: either add an `ArtistsController` (filtered People view, `PersonKind`/`PersonType = MAIN_ARTIST`) or retarget the nav to `/people`. Recommend adding the controller (spec routes list people, but "Artists" is a reasonable browse entry).
- **Verify:** click every nav item in desktop + mobile menus on `/fa` → all 200.

**DoD:** Search returns results; home page populated; seed content browsable; zero 404s in primary nav.

---

## ✅ Phase 2 — Localization & Multi-Language (COMPLETE)

**Objective:** Full `fa / en / ar / fr` support with deterministic fallback and RTL.

### 2.1 Enable all four cultures
- **Touch:**
  - `Core/Constants/CultureConstants.cs` — `SupportedCultures = [fa, en, ar, fr]`; **`FallbackCulture = "en"`** (currently wrongly `"fa"`).
  - `Web/Extensions/MvcBuilderExtensions.cs` + `Web/Program.cs` — supported cultures + request localization options.
  - Every controller route regex `regex(^(fa)$)` → `regex(^(fa|en|ar|fr)$)` (albums, tracks, people, companies, genres, moods, instruments, poets, poems, sung-versions, sessions, events, locations, awards, charts, sources, tags, search, home).
  - `Middleware/CultureMiddleware.cs` — set `lang` + `dir` (rtl for `fa`/`ar`, ltr otherwise) for all cultures.
- **Verify:** `/en/albums`, `/ar/albums`, `/fr/albums` render with correct `<html dir/lang>`.

### 2.2 Fix ContentLocalizationService (it is currently broken + unused)
- **Current bug:** `Services/Services/ContentLocalizationService.cs` queries `Localization.LanguageCode` — that column doesn't exist (schema uses `LanguageId` FK → `Language.Code`). It also hardcodes `SqlConnection` (fails in SQLite mode).
- **Touch:** rewrite to join `Language l ON l.LanguageId = loc.LanguageId AND l.Code = @culture`; use provider-agnostic connection (reuse the `SqlDialect`/DI pattern from the query services); add a **batch** method `GetLocalizedValuesAsync(entityId, fields[], culture)` to avoid per-field N+1.
- **Verify:** unit test: requested-culture value → base fallback → `en` fallback → empty (spec §7.3 order).

### 2.3 Apply localized content in read paths
- **Goal:** Query services return localized `Title`/`Description`/`Biography` per culture (spec §7.3). Currently they select base columns only.
- **Touch:** `AlbumQueryService`, `TrackQueryService`, `PersonQueryService`, `CompanyQueryService` — either a SQL `LEFT JOIN Localization` per culture or post-processing with the batch localization service.
- **Verify:** with an `en` localization row inserted, `/fa/...` and `/en/...` show different titles for the same entity.

### 2.4 UI string localization
- **Touch:** add `.resx` under `Web/Resources/` (`SharedResources.en.resx`, `.fa.resx`, `.ar.resx`, `.fr.resx`); `_ViewImports` already has `IViewLocalizer` usage in views; translate nav/labels/buttons.
- **Verify:** switching culture changes UI chrome text.

### 2.5 Language switcher + hreflang
- **Touch:** `_Layout.cshtml` — language switcher that preserves the current path (`/en/albums/{slug}` ↔ `/fa/albums/{slug}`); emit `<link rel="alternate" hreflang>` for all 4 cultures + `x-default` (spec §16.2).
- **Verify:** a detail page's HTML contains 4 hreflang alternates; switcher preserves entity.

**DoD:** 4 cultures render, RTL correct, localization fallback deterministic, hreflang present, no broken links on switch.

---

## ✅ Phase 3 — Complete Admin CRUD (COMPLETE)

**Objective:** Make every admin area functional, especially the related-editor tabs that are currently "(Coming soon)" placeholders.

### 3.1 Album related-editors (largest admin gap)
- **Goal:** Replace the placeholder tab panels in `Areas/Admin/Views/Albums/Edit.cshtml` with working editors (spec §10.5).
- **Touch:** `Albums/Edit.cshtml` + admin `AlbumsController` (add POST endpoints; use transactions + `RowVersion` concurrency + soft delete):
  - **Tracklist (AlbumTrack) editor** — add existing track, create track inline, set disc/track/sequence numbers, `TrackTitleOverride` + `DurationSecondsOverride`, reorder (drag/drop or up/down), prevent duplicate disc/sequence.
  - **Credit editor** — reuse `Shared/_CreditEditor.cshtml`; person/company selector driven by `RoleScopeType`, instrument shown for musician roles, `IsPrimary`, `DisplayOrder`, duplicate prevention (spec §10.7).
  - Genre / Mood / Language / Country / Company (+catalog number/barcode) / Identifier editors.
  - Alias / Localization / Tag / Link / Citation / Attribute editors (reuse existing partials `_AliasEditor`, `_AttributeValueEditor`).
- **Verify:** create an album with 2-disc tracklist + credits via UI; reopen; all persisted; duplicate prevention works; concurrency error surfaces on stale edit.

### 3.2 Track related-editors
- **Touch:** `Areas/Admin/Views/Tracks/Edit.cshtml` + admin `TracksController` — album appearances, artist credits, genres/moods/instruments, musicians (person + instrument), sung versions, original-poem selector, version types, related tracks, sessions/events, alias/localization/tag/link/media/citation/attribute (spec §10.6).
- **Verify:** same criteria as 3.1.

### 3.3 Missing admin sections (spec §10.3 nav items with no implementation)
Build each as a standard CRUD pair (controller + `EditViewModel`/`ListViewModel` + Create/Edit/Index views), matching the existing admin patterns (base controller, RowVersion, authorization policy):
- **Users & Roles** — Identity management UI: list users, create/edit, assign roles; list/create roles (policies `CanManageUsers`). New `UsersController`, `RolesController`.
- **Certifications** — entity CRUD + `CertificationAssignment` UI (no controller exists anywhere; spec §10 nav + public display). New admin `CertificationsController`.
- **Publications** — CRUD (`Publication`, `PublicationType`). New admin `PublicationsController`.
- **Sources** — admin CRUD (public pages exist; only inline citation creation exists today). New admin `SourcesController`.
- **Lookup Tables** — generic CRUD for lookup types (Language, Country, EntityType, etc.). New `LookupTablesController`.
- **Settings** — site settings page (config-backed initially). New `SettingsController`.
- **Audit Log** — `AuditLog` table + save-on-entity-change + read-only list view. New `AuditLogController`.
- **Attributes** — verify `AttributeValuesController` is complete; wire the attribute editor into entity edit pages if not already.
- **Verify:** every item in the admin nav renders, is authorized, and CRUDs persist.

### 3.4 Bulk soft delete / restore (admin lists)
- **Goal:** Checkbox multi-select + bulk delete/restore (comment in `Admin/Views/Albums/Index.cshtml` says "to be implemented with JavaScript").
- **Touch:** admin list views + controllers: add checkbox column, JS select-all, POST bulk endpoints (`/admin/albums/bulk-delete`, `bulk-restore`), antiforgery token.
- **Verify:** select 3 rows → bulk delete → rows gone; restore returns them; public site hides deleted.

**DoD:** All admin nav items functional; related-editor tabs persist data; validation + concurrency + soft delete + authorization enforced.

---

## ⏳ Phase 4 — Advanced Encyclopedia Features (M5) — NEXT UP

**Objective:** Complete the remaining public-facing content features.

### 4.1 Person timeline (spec §9.6 §3)
- **Touch:** `PersonQueryService`/`PersonDetailViewModel` — aggregate album releases, track releases, recording sessions, live events, awards, chart entries, publications into one date-sorted timeline.
- **Verify:** `/fa/people/{slug}` shows the timeline when a person has such data.

### 4.2 Discography & contribution filters on person page
- **Touch:** person detail — filter albums by category/year/role/instrument; filter track contributions by role/instrument/album/genre (spec §9.6 §4–5).
- **Verify:** filters return correct subsets.

### 4.3 Certifications + related items on public pages
- **Touch:** `Views/Albums/Detail.cshtml`, `Views/Tracks/Detail.cshtml` — render certifications section (DTO already carries them; view doesn't render yet); related tracks/albums sections once 3.1/3.2 editors exist.
- **Verify:** data entered via admin appears on public pages.

**DoD:** Timeline, filters, certifications, and related-entity sections render from DB.

---

## ⏳ Phase 5 — Performance, Search & Caching (spec §14–15, §18; M6)

**Objective:** Fast pages, working full-text search, and real cache invalidation.

### 5.1 Integrate ICacheService into read paths
- **Current state:** `ICacheService` is registered and injected into API controllers but never exercised; MVC relies only on `[ResponseCache]` (HTTP-level); `CacheInvalidationService`/`CacheKeyTracker` exist but nothing invalidates.
- **Touch:**
  - Query services (or a decorator) — wrap results in `ICacheService` using spec §15.1 keys (`album:detail:{culture}:{slug}`, `album:list:{culture}:{hash}`, etc.) and §15.2 durations.
  - Admin write paths — call `CacheInvalidationService.Invalidate(entityType, entityId)` after create/update/soft-delete/restore; raise `EntityChangedEvent` (model exists).
  - Decide on API controllers: either actually use `_cache` or drop the unused dependency.
- **Verify:** second request for same key is a cache hit (log `Cache HIT`); after admin edit, next public request re-queries.

### 5.2 SQL Server full-text search
- **Touch:** apply `src/MusicEncyclopedia.Data/Scripts/FullTextSearch.sql` (catalog + indexes) on a SQL Server instance; align `SearchService` FTS column list (done in 1.1); add a background rebuild job via Hangfire (spec §20).
- **Verify:** `FREETEXTTABLE` path returns ranked results; Hangfire search-index job runs.

### 5.3 Query optimization pass
- **Touch:** audit `AlbumQueryService`/`TrackQueryService`/`PersonQueryService` for N+1 (e.g., per-track credits/lyrics) and apply spec §18.4 batched-query pattern; add missing indexes if profiling shows them.
- **Verify:** server response < 300 ms on detail pages (rough measure); no obvious duplicate queries in logs.

**DoD:** Cache hits + invalidation proven; full-text search works on SQL Server; detail pages meet latency target.

---

## ⏳ Phase 6 — Hardening & Deployment (spec §17, §19–20; M7)

**Objective:** Production-ready packaging, CI, and operations.

### 6.1 Containerization
- **Touch:** add `Dockerfile` (multi-stage: `mcr.microsoft.com/dotnet/sdk:8.0` build → `aspnet:8.0` runtime) and `docker-compose.yml` (web + `mcr.microsoft.com/mssql/server:2022`), healthcheck wiring.
- **Verify:** `docker compose up` → `/health/ready` green against SQL Server.

### 6.2 CI pipeline
- **Touch:** `.github/workflows/ci.yml` — restore, build, `dotnet test`, publish artifact. (No CI exists today.)
- **Verify:** pipeline green on a push (or locally via `act` if available).

### 6.3 Production configuration & secrets
- **Touch:** `appsettings.Production.json` — `DatabaseProvider: SqlServer`, real `DefaultConnection` from env/secrets, HTTPS, `Site.BaseUrl`, CDN base URL; ensure Hangfire + rate limiting + CSP tuned for production; verify `Cookie.SecurePolicy` etc. (already set).
- **Verify:** app boots with Production env + SQL Server.

### 6.4 Monitoring, backups, load
- **Touch (optional but recommended):** Serilog sink (e.g., Application Insights or Seq) behind config; document SQL backup job + restore drill (spec §20); optional k6/`hey` load test against `/fa/albums`.
- **Verify:** structured logs flow; backup/restore procedure documented and rehearsed once.

**DoD:** Containers build, CI green, prod config boots, backup + monitoring documented.

---

## ⏳ Phase 7 — Testing (spec §21; acceptance #19)

**Objective:** Replace all stub test projects with meaningful coverage.

### 7.1 Unit tests
- **Touch:** `tests/MusicEncyclopedia.Core.Tests`, `tests/MusicEncyclopedia.Services.Tests`:
  - `SlugService` (URL-safety, unicode, length), localization fallback chain (2.2), `CreditValidator`/`AlbumValidator`/`TrackValidator`, lyrics-availability rules, `PagedResult` pagination math, date-precision display.

### 7.2 Integration tests
- **Touch:** `tests/MusicEncyclopedia.Web.Tests` (WebApplicationFactory + SQLite or LocalDB):
  - Album list/detail render (200 + expected content), track/person detail, search returns results, home shows content.
  - Auth: `/admin` redirects anonymous; policies enforced; soft-deleted entity returns 404 publicly.
  - Restricted lyrics not exposed when `LyricsAvailabilityType != PUBLIC`.
  - API envelope shape (`success`, `data`, `errors`).

### 7.3 API tests
- **Touch:** `tests/` — albums/people/poems endpoints return correct DTOs + envelope.
- **Verify:** `dotnet test` → all green (run after each phase, not just at the end).

**DoD:** `dotnet test` green; each spec §21.2 scenario covered.

---

## ⏳ Phase 8 — Acceptance & Polish (§36)

**Objective:** Walk the 20 final acceptance criteria and close the last gaps.

### 8.1 Acceptance walkthrough
- **Touch:** audit checklist from `music-encyclopedia-q.md` §36 — for each of the 20 items, verify + fix. Known soft spots to watch: multilingual (2.x), RTL, album-track M:M (3.1), credits people/companies/roles/instruments (3.1/3.2), poems vs sung versions separation, lyrics availability, media upload+assign, citations display, tags/aliases/localizations, search (1.1/5.2), SEO/sitemap/JSON-LD (2.5 + existing), admin authorization, soft delete, performance targets, security, tests, deployment.

### 8.2 Performance targets
- **Verify:** LCP < 2.5 s, CLS < 0.1, page weight < 2 MB (spec §18.2); fix lazy-loading/image issues.

### 8.3 Optional future enhancements (§35) — deferred backlog
- Document as backlog (not MVP): user favorites/ratings/corrections, editorial workflow, version history, timeline visualization, relationship graph, music player, waveform, IIIF, public API keys, GraphQL, metadata imports, duplicate detection, AI search, recommendations.

**DoD:** All 20 acceptance criteria pass; backlog documented.

---

## Execution order & parallelism (remaining work)

```
Phase 4  (advanced features)  → NEXT; depends on 3.1/3.2 (editors) — both done ✓
Phase 5  (perf/search/cache)  → 5.2 needs a SQL Server instance; 5.1 independent
Phase 6  (deployment)         → independent; can start after Phase 5
Phase 7  (tests)              → write incrementally per phase; full pass after each phase
Phase 8  (acceptance)         → last
```

Recommended agents split: one agent per phase (4, 5, 6, 7) after Phase 4 lands.

## Risks & notes

- **No SQL Server instance available locally** → Phases 5.2, 6.1, Hangfire, and the FTS path are unverifiable until one is provisioned (Docker `mssql` image works). All dev work is on SQLite.
- **`ContentLocalizationService` and API controllers pass `culture: "en"`** which is not a supported culture until Phase 2.1 lands — fix ordering so 2.1 ships before any culture-dependent verification.
- **SkiaSharp** (MediaService thumbnails) requires native libs — the copy-fallback path already guards this; keep it.
- **Scope control:** do not start §35 items until §36 passes.
- Every SQL change must stay parameterized and dialect-safe (see `Services/Infrastructure/SqlDialect.cs`).

---

## Definition of Done (whole program)

1. ✅ `dotnet build` + `dotnet test` green on the machine (Phase 0).
2. ✅ Public site browsable with seeded content, search working, home populated (Phase 1).
3. ✅ All 4 cultures render with RTL and localization fallback (Phase 2).
4. ✅ Every admin nav section functional with validation, concurrency, soft delete, authorization (Phase 3).
5. ⏳ Advanced features render from DB (Phase 4).
6. ⏳ Caching + invalidation proven; full-text search works on SQL Server (Phase 5).
7. ⏳ Containers + CI + production config working (Phase 6).
8. ⏳ Tests cover spec §21 (Phase 7).
9. ⏳ Spec §36's 20 acceptance criteria all pass (Phase 8).
