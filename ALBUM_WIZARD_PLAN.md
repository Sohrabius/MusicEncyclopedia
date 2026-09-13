# Album Creation Wizard — Implementation Plan

> Status reconciliation (2026-09-11): Fresh SQL-backed regression verification is complete under IMPLEMENTATION_PLAN.md task B07. The complete graph, two-disc ordering, global entity assignments, transaction rollback, concurrency, and shared-track editing now have current automated evidence; the old 13-test pass included two empty test methods and is only historical context.

**Current status:** ✅ Implemented and verified under B07; browser layout coverage remains in B09/B10.
**Branch:** redesign/ui-v2
**Audience:** admins with `CanManageAlbums`

## 1. Goal

Replace the "create album, then click through 15 edit tabs" workflow with a single
**step-by-step wizard** (`Next` / `Back` buttons) that creates an album **and all of
its dependencies in one submission**: tracks (with their full nested dependencies),
credits, genres/moods/languages/countries, companies, identifiers, sung versions,
tags, links, and related albums.

## 2. Confirmed design decisions (user)

| Decision | Choice |
| --- | --- |
| Wizard persistence | **Single atomic form (client stepper).** One `<form>`; `Next`/`Back` toggle sections client-side; everything is created in **one POST** at the end. Nothing is persisted until `Finish`. |
| Track dependency depth | **Full per-track editor.** Each track card has genres, moods, instruments, version types, sung versions (with poem picker), and per-track credits. |
| Entry point | **Separate wizard page.** Keep the current flat `/admin/albums/Create`; add `/admin/albums/wizard` with a prominent button on the Albums list. |

What stays on the existing **Edit** page tabs (not in the wizard): media uploads /
assignments, citations, attributes, aliases, localizations, and recording sessions /
events (they need an existing entity or are niche enough to refine later).

## 3. Current state (what exists)

- `AlbumsController.Create` (GET/POST) — flat form, creates `Entity` (type Album)
  + `Album`, then redirects to Edit. Persian RTL UI, `_AdminLayout`, Tailwind.
- `AlbumsController.Edit` — same form + 15 lazy-loaded AJAX tab editors
  (`_RelatedItemsEditor`, `_AlbumTracklistEditor`, `_CreditEditor`, …) operating on
  the **already-created** album via `/admin/related-items/*`, `/admin/album-tracklist/*`,
  `/admin/credits/*`.
- Reusable server pieces:
  - `SlugService.GenerateSlug(...)` + unique-slug loop (see `AlbumTracklistController.CreateInline`).
  - `AlbumValidator` / `TrackValidator` (FluentValidation) — extend, don't duplicate.
  - `EntityTypeConstants` (Album=1, Track=2, SungVersion, Poem …) + `InvalidateEntityCacheAsync`
    / `InvalidateBroadCacheAsync` on `AdminBaseController`.
  - `RelatedItemsController` kinds (`album-genres`, `album-moods`, `album-languages`,
    `album-countries`, `album-companies`, `album-identifiers`) — read them for the
    exact join shapes to replicate.

**Note on entity type ids:** code hardcodes Album=1, Track=2 in places. The wizard
must resolve `EntityTypeId` by looking up the `EntityType` row by
`EntityTypeConstants` code (or reuse the same constants) so it stays correct if
ids ever change.

## 4. Wizard steps

All steps are sections of one form. Stepper header shows 6 steps; `Next` validates
the current step (unobtrusive validation), `Back` goes to the previous step.

### Step 1 — Album basics
- Title (required), TitleSort, OriginalTitle, EnglishTitle
- Slug — auto-generated from Title (client-side slugify, lowercase + hyphens),
  editable; uniqueness enforced server-side at submit
- Category (required dropdown), IsOfficial
- Release date + precision (day/month/year); recording start/end + precision
- Description, CopyrightNotice
- DurationSeconds — with a "sum of track durations" auto-fill button (filled from Step 3)
- CoverMediaId — media picker (simple dropdown of existing media; full media upload
  stays on Edit page)

### Step 2 — Album classification & distribution
- Genres (multi-select checkboxes) → `AlbumGenre`
- Moods (multi-select) → `AlbumMood`
- Languages (multi-select) → `AlbumLanguage`
- Countries (multi-select) → `AlbumCountry`
- Companies — repeatable rows: Company (dropdown), CompanyRoleType (optional),
  CatalogNumber, Barcode → `AlbumCompany`
- Identifiers — repeatable rows: IdentifierType (optional), Value (required) → `AlbumIdentifier`

### Step 3 — Tracks (full per-track editor)
Repeatable **track cards** ("+ افزودن ترک"). Each card:

- **Album-track row:** DiscNumber (default 1), TrackNumber (auto-sequenced),
  IsBonus, IsHidden (album-track flags)
- **Track basics:** Title (required), TitleSort, OriginalTitle, EnglishTitle,
  DurationSeconds, Release date + precision, Recording dates + precision,
  Description, CopyrightNotice, Slug (auto from title, editable)
- **Music details:** LyricsAvailabilityType, VocalStyle, MusicalKey, BPM, ISRC,
  IsInstrumental, IsExplicit
- **Assigns (multi-selects):** Genres → `TrackGenre`, Moods → `TrackMood`,
  Instruments → `TrackInstrument`, Version types → `TrackVersionTypeAssignment`
- **Sung versions** — repeatable rows ("+ افزودن نسخه خوانده‌شده"), each row:
  - `SungVersionId` (pick an **existing** sung version) **or** create new:
    Title (required), Poem (dropdown of existing poems — required), VocalStyle,
    IsCanonical, Text (optional), Notes (optional)
  - New sung versions are created (Entity + `SungVersion`) during the wizard;
    poems are **selected only** (creating poems inline is out of scope for v1)
  → `TrackSungVersion`
- **Per-track credits** — repeatable rows (same fields as album credits):
  CreditRole, RoleScopeType, Person or Company or Instrument, DisplayOrder,
  IsPrimary, Notes → `Credit` (EntityTypeId = Track)

### Step 4 — Album credits
- Repeatable rows: CreditRole (required), RoleScopeType, Person/Company/Instrument
  (at least one), DisplayOrder, IsPrimary, Notes → `Credit` (EntityTypeId = Album)
- Reuses the same row UI as track credits (one shared partial / JS).

### Step 5 — Tags, links & related albums
- Tags — multi-select of existing tags → `TagAssignment` (EntityTypeId = Album)
  (creating new tags inline is optional stretch)
- Links — repeatable rows: URL (required, validated), LinkType, Label → `EntityLink`
- Related albums — repeatable rows: Album (dropdown), RelationType (e.g. "compilation
  of" / "part of") → `AlbumRelation` (both directions: `AlbumRelations` +
  `RelatedAlbumRelations`)

### Step 6 — Review & submit
- Summary list: album title/category, N tracks, M credits, counts of each assigned
  dependency (read from the bound model client-side)
- `Finish` button → POST `/admin/albums/wizard`
- Any server-side validation errors (duplicate slugs, missing poems, etc.) render
  back with errors preserved on the failing step.

## 5. ViewModels (new)

All in `src/MusicEncyclopedia.Web/ViewModels/Admin/AlbumWizardViewModels.cs`:

```csharp
public sealed class AlbumWizardViewModel
{
    // Step 1 — reuse the same property set as AlbumEditViewModel
    string Title, TitleSort?, OriginalTitle?, EnglishTitle?, Slug;
    int AlbumCategoryId; bool IsOfficial;
    DateOnly? ReleaseDate; string? ReleaseDatePrecision;
    DateOnly? RecordingStartDate, RecordingEndDate; string? RecordingDatePrecision;
    string? Description; string? CopyrightNotice;
    int? DurationSeconds; int? CoverMediaId;

    // Step 2
    List<int> GenreIds, MoodIds, LanguageIds, CountryIds;
    List<CompanyWizardItem> Companies;
    List<IdentifierWizardItem> Identifiers;

    // Step 3
    List<TrackWizardItem> Tracks;

    // Step 4
    List<CreditWizardItem> AlbumCredits;

    // Step 5
    List<int> TagIds;
    List<LinkWizardItem> Links;
    List<RelatedAlbumWizardItem> RelatedAlbums;

    // Dropdown data (populated on GET; re-populated on validation failure)
    IReadOnlyList<AlbumCategory> Categories;
    IReadOnlyList<Genre> Genres; /* Moods, Languages, Countries, Companies,
        CompanyRoleTypes, IdentifierTypes, Instruments, VocalStyles, MusicalKeys,
        LyricsAvailabilityTypes, TrackVersionTypes, CreditRoles, RoleScopeTypes,
        People, Poems, SungVersions, Tags, LinkTypes, Albums */
}

public sealed class TrackWizardItem
{
    string Title; string? TitleSort, OriginalTitle, EnglishTitle, Description,
        CopyrightNotice, Slug, ReleaseDatePrecision, RecordingDatePrecision;
    DateOnly? ReleaseDate, RecordingStartDate, RecordingEndDate;
    int? DurationSeconds, LyricsAvailabilityTypeId, VocalStyleId, MusicalKeyId;
    short? BPM; string? ISRC; bool IsInstrumental, IsExplicit;
    int DiscNumber, TrackNumber; bool IsBonus, IsHidden; // AlbumTrack flags
    List<int> GenreIds, MoodIds, InstrumentIds, VersionTypeIds;
    List<SungVersionWizardItem> SungVersions;
    List<CreditWizardItem> Credits;
}

public sealed class SungVersionWizardItem
{
    int? SungVersionId;                    // pick existing
    string? Title; int? PoemId;            // create-new branch (PoemId required)
    int? VocalStyleId; bool IsCanonical;
    string? Text, Notes;
    bool IsNew => SungVersionId is null;
}

public sealed class CreditWizardItem
{
    int CreditRoleId, RoleScopeTypeId; int? PersonId, CompanyId, InstrumentId;
    int DisplayOrder; bool IsPrimary; string? Notes;
}

public sealed class CompanyWizardItem
{ int CompanyId; int? CompanyRoleTypeId; string? CatalogNumber, Barcode; }

public sealed class IdentifierWizardItem
{ int? IdentifierTypeId; string Value = ""; }

public sealed class LinkWizardItem
{ string Url = ""; int? LinkTypeId; string? Label; }

public sealed class RelatedAlbumWizardItem
{ int AlbumId; int? AlbumRelationTypeId; }
```

## 6. Server-side changes

### 6.1 New controller: `AlbumWizardController`
`src/MusicEncyclopedia.Web/Controllers/Admin/AlbumWizardController.cs`

- `[Route("/admin/albums/wizard")]`, `[Authorize(Policy = CanManageAlbums)]`,
  derives from `AdminBaseController` (gets cache-invalidation helpers).
- **GET** — load all dropdown data into `AlbumWizardViewModel`; render `Index.cshtml`
  (convention view lookup for `AlbumWizardController` → `Areas/Admin/Views/AlbumWizard/Index.cshtml`).
- **POST** (`[ValidateAntiForgeryToken]`) — build + save everything inside a single
  `DbTransaction` (or rely on one `SaveChangesAsync`):
  1. Resolve `EntityTypeId`s from the `EntityType` table by code.
  2. Validate: `AlbumValidator` for album fields; per-track `TrackValidator` +
     custom checks (at least one track? — decide: tracks are **optional** on v1,
     a title-only album is legal); every sung-version "new" row has a `PoemId`;
     link URLs valid; company/identifier/credit rows complete.
  3. `Entity` (Album) → `Album` (same as `AlbumsController.Create`).
  4. Album joins: `AlbumGenre`, `AlbumMood`, `AlbumLanguage`, `AlbumCountry`,
     `AlbumCompany`, `AlbumIdentifier`.
  5. Per track: `Entity` (Track) → `Track` → `AlbumTrack` (sequence = position
     within disc, renumber 1..N) → `TrackGenre` / `TrackMood` / `TrackInstrument`
     / `TrackVersionTypeAssignment` → sung versions (existing `TrackSungVersion`
     rows, or new `Entity` + `SungVersion` + `TrackSungVersion`) → per-track
     `Credit` rows.
  6. Album `Credit` rows, `TagAssignment`s, `EntityLink`s, `AlbumRelation`s
     (insert both directions), with `CreatedBy`/`CreatedAt` set everywhere.
  7. Cache: `InvalidateEntityCacheAsync` for the album + each new track/sung
     version; `InvalidateBroadCacheAsync()` once.
  8. Success message (Persian, same style as existing controllers) →
     redirect to `AlbumsController.Edit` so the admin can continue refining
     (media, citations, …) on the tabbed page.
- On validation failure: re-populate dropdowns, keep the submitted model, mark the
  **first failing step** in a `ViewData`/model flag so the view opens on the right
  step and shows errors inline.

### 6.2 Validators
- Extend `AlbumValidator` (it already covers Title/Slug/precision) — add nothing
  structural; reuse as-is.
- Reuse `TrackValidator` for track basics; add a small
  `AlbumWizardValidator` (FluentValidation) for wizard-level rules:
  - each `SungVersionWizardItem.IsNew` requires `PoemId`
  - each `CreditWizardItem` requires `CreditRoleId` + `RoleScopeTypeId` + at least
    one of Person/Company/Instrument
  - `LinkWizardItem.Url` is a valid absolute http(s) URL
  - identifiers without a type still require a value

### 6.3 Slug uniqueness
- Client auto-fills slugs from titles; server re-checks and suffixes (`-2`, `-3`, …)
  via the same loop already used in `AlbumTracklistController.CreateInline`.
- Album slug and every track slug get this treatment.

## 7. Client-side changes

### 7.1 `Index.cshtml` (new view)
`src/MusicEncyclopedia.Web/Areas/Admin/Views/AlbumWizard/Index.cshtml` (moved from
`Areas/Admin/Views/Albums/Wizard.cshtml` — see bug #1 below). Persian RTL, Tailwind,
matches existing card styling (`bg-white rounded-lg shadow-sm border border-gray-200 p-5`).

- One `<form method="post" action="/admin/albums/wizard">` with anti-forgery token.
- Stepper header: 6 numbered steps with titles + icons; completed steps show a
  checkmark; current step highlighted; steps are clickable only if already visited.
- Each step is a `<section class="wizard-step">` — all but the active one hidden.
- Footer nav: `قبلی` (Back), `بعدی` (Next), and on the last step `ایجاد آلبوم`
  (Submit). Next runs `$("#form").validate()` scoped to the step's fields.
- `@section Scripts` — jquery validate (already used elsewhere) + wizard JS below.

### 7.2 `_WizardTrackCard.cshtml` (partial, shared by step 3)
`src/MusicEncyclopedia.Web/Areas/Admin/Views/AlbumWizard/_WizardTrackCard.cshtml`.
One track card rendered server-side per existing track on re-render, and fetched
server-side for new rows via `GET /admin/albums/wizard/track-card` (so dropdown
options never drift from the server). Contains the full per-track editor:
- track basics + music details + album-track flags
- genre / mood / instrument / version-type multi-select checkboxes
- **sung-version rows** (`<template class="tpl-sv">`): pick an existing version
  (`SungVersionId`) or create new (Title + Poem dropdown + VocalStyle + Text +
  Notes + IsCanonical)
- **per-track credit rows** (`<template class="tpl-track-credit">`): CreditRole,
  RoleScopeType, Person/Company/Instrument, DisplayOrder, IsPrimary, Notes
  (same row shape as Step 4 album credits)

### 7.3 Wizard JS
Inline `@section Scripts` in `Index.cshtml` (jquery.validate + unobtrusive + the
wizard JS below):

- **Stepper:** step navigation, validation per step, scroll-to-top, restore last
  step after server round-trip (model flag → JS init data).
- **Dynamic rows:** "add / remove" for tracks, sung versions, credits, companies,
  identifiers, links, related albums — clone a `<template>` row, re-index `name`
  attributes (`Tracks[0].Genres` … `Tracks[1].SungVersions[0].PoemId`) so MVC model
  binding binds nested lists, and remove buttons delete the row.
- **Auto-fill:** slug from title (slugify, allow manual override); track numbers
  auto-sequence per disc; album `DurationSeconds` "sum from tracks" button.
- **Step 2/3 multi-selects:** checkboxes bound to hidden `List<int>` fields (or
  `name="GenreIds"` on the checkbox inputs directly — binder collects duplicates).

## 8. Data shapes written per submission (summary)

| Target | From wizard | Created rows |
| --- | --- | --- |
| Album | Step 1 | `Entity`(Album) + `Album` |
| Album classification | Step 2 | `AlbumGenre`×n, `AlbumMood`×n, `AlbumLanguage`×n, `AlbumCountry`×n, `AlbumCompany`×n, `AlbumIdentifier`×n |
| Tracks | Step 3 | per track: `Entity`(Track) + `Track` + `AlbumTrack` + `TrackGenre`×n + `TrackMood`×n + `TrackInstrument`×n + `TrackVersionTypeAssignment`×n + `TrackSungVersion`×n (+ new `Entity`(SungVersion)+`SungVersion` for new ones) + `Credit`×n (Track) |
| Album credits | Step 4 | `Credit`×n (Album) |
| Tags/links/relations | Step 5 | `TagAssignment`×n, `EntityLink`×n, `AlbumRelation`×n (both directions) |

## 9. Implementation order (phases)

All phases are complete (✅ = done & verified end-to-end against LocalDB).

1. ✅ **ViewModels + wizard dropdown DTOs** (`AlbumWizardViewModels.cs`) — nested
   items (`TrackWizardItem`, `SungVersionWizardItem` with computed `IsNew`,
   `CreditWizardItem`, `CompanyWizardItem`, `IdentifierWizardItem`, `LinkWizardItem`,
   `RelatedAlbumWizardItem`) + 21 dropdown option lists + `ActiveStep`.
2. ✅ **Controller GET/POST** with transactional save + cache invalidation
   (`AlbumWizardController.cs`) + `TrackCard` partial action. Verified with a
   LocalDB run: full album + track + sung version + credits POSTed via curl,
   rows confirmed in all join tables (see §10 verification evidence).
3. ✅ **Wizard view + partials** — 6-step stepper layout bound to the model;
   Next/Back works; POST saves atomically. Views live in `Views/AlbumWizard/`
   (`Index.cshtml` + `_WizardTrackCard.cshtml`).
4. ✅ **Dynamic rows + full track editor JS** — add/remove tracks (server-fetched
   partial), sung versions, credits, companies, identifiers, links, relations;
   `renumber()` keeps `Tracks[i].SungVersions[j].*` / `Tracks[i].Credits[j].*`
   indexes contiguous; slug + track-number auto-fill.
5. ✅ **Validation wiring** — client-side per-step (unobtrusive) + server-side
   checks in `RunValidationAsync` (FluentValidation re-use for album/track,
   custom wizard rules for sung versions/credits/links/companies); error
   round-trip opens the first failing step via `ActiveStep`.
6. ✅ **Tests + verification** — full solution build (0 errors) + all 13 tests
   pass; end-to-end curl verification against LocalDB (see §10).
7. ✅ **Polish** — step summary on the review page, `Wizard` button
   («ایجاد با ویزارد») on the Albums list, `view-data` single-quote fix for the
   track-card partial. Not done (optional follow-up): "sum durations" button,
   sidebar link.

## 10. Bugs found & fixed during verification

Recorded so the fixes aren't lost and similar traps are avoided elsewhere:

1. **View lookup mismatch (500 on GET).** `AlbumWizardController` derives its view
   path from the controller name (`Views/AlbumWizard/`), but the views were first
   written to `Views/Albums/` → `InvalidOperationException: The view 'Index' was
   not found`. **Fix:** moved `Wizard.cshtml` → `Areas/Admin/Views/AlbumWizard/Index.cshtml`
   and `_WizardTrackCard.cshtml` into the same folder.

2. **Invalid Razor `view-data` escaping (compile error).** The partial tag helper
   was written as `view-data="new ViewDataDictionary(ViewData) { [\"TrackIndex\"] = i }"`
   — the `\"` escapes break the Razor parser (35 errors). **Fix:** single-quoted
   attribute with the C# expression form:
   `view-data='@(new ViewDataDictionary(ViewData) { { "TrackIndex", i }, { "Shared", Model } })'`.

3. **`InferFailingStep` jumped to the wrong step.** It scanned *all* ModelState
   keys, and bound-but-valid keys (e.g. `Tracks[0].Title`) matched the `Tracks`
   prefix, so a failing step 1 error (Slug) opened step 3. **Fix:** skip keys where
   `modelState[key]?.Errors.Count == 0`.

4. **Retrying execution strategy rejects the transaction (500 on POST).**
   `EnableRetryOnFailure()` (SqlServerRetryingExecutionStrategy) forbids
   `BeginTransactionAsync()` outside the strategy:
   `InvalidOperationException: The configured execution strategy ... does not
   support user-initiated transactions`. **Fix:** split `PersistAsync` into a thin
   wrapper that runs `_db.Database.CreateExecutionStrategy().ExecuteAsync(() =>
   PersistCoreAsync(...))`; `PersistCoreAsync` owns the `BeginTransactionAsync` /
   `SaveChangesAsync` ×2 / `CommitAsync` with `RollbackAsync` on failure.

5. **Test-harness gotchas (not app bugs, but cost time):** Windows curl mangles
   Persian UTF-8 passed on the command line (use `printf '\uXXXX'` into a body
   file); form bodies need `&` separators (a `\n`-separated body parses as one
   field and breaks the antiforgery token); the wizard GET must be fetched with
   `-c` (cookie jar) so the antiforgery cookie from that response is saved
   alongside the extracted hidden token.

### Verification evidence (LocalDB, live run)

| POST exercised | Result | DB proof |
| --- | --- | --- |
| Album + genre + track + genre + album credit | 302 → `/admin/albums/6/Edit` | `Album` 6, `Track` 14, `AlbumGenre`(6,3), `TrackGenre`(14,3), `Credit`(EntityType=Album, role 8 / scope 2 / person 1), `Entity` rows for album+track |
| Track with **new** sung version (Poem 1, canonical) + track credit | 302 → `/admin/albums/7/Edit` | `SungVersion` 1 (Poem 1, IsCanonical), `TrackSungVersion`(track 15 → sv 1, seq 1, primary), `Credit`(EntityType=Track, EntityId=15), `Entity`(type SungVersion) |
| Track with **existing** sung version (id=1) | 302 → `/admin/albums/8/Edit` | `TrackSungVersion`(track 16 → sv 1) and `SV_COUNT=1` — reused, not duplicated |

Also verified: anonymous GET → 302 to `/auth/login` (auth policy works); authenticated
GET renders all 6 steps + 15 populated dropdowns; `GET /admin/albums/wizard/track-card`
returns the partial with correct `Tracks[0].*` names; target Edit page returns 200.
Build: **0 errors / 0 warnings**, all 13 tests pass.

## 11. Risks & edge cases

- **Payload size:** many tracks × many dependencies → large POST. Mitigate with the
  per-step client validation and a `TrackNumber` cap sanity check (no hard limit v1).
- **Nested model binding:** index gaps after row removal (`Tracks[0]`, `Tracks[2]`)
  — the binder tolerates gaps for `List<T>`; keep indexes consistent in JS anyway.
- **Slug collisions:** every new album/track/sung-version slug re-checked on the
  server (shared loop), never trusted from the client.
- **Poem dependency:** creating new poems inline is explicitly out of scope —
  a sung version requires an existing poem; the dropdown is searchable if the poem
  list is large.
- **Atomicity:** all writes happen in one transaction (inside the retrying
  execution strategy — see bug #4); on any failure nothing is persisted and the
  form returns with errors (no orphaned Entities — note the current
  `AlbumsController.Create` saves `Entity` before `Album` in two steps; the wizard
  does not reproduce that split).
- **Localization/attribute/media tabs** intentionally remain on Edit — call this out
  in the wizard's info panel so admins know where to finish those.
