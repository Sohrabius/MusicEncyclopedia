# Music Encyclopedia — UI Redesign Plan

> Status: Proposed · Prepared with **ui-ux-pro-max** design intelligence
> Goal: A **clean, minimal, user-friendly** UI — one design language across all 132 views (public + admin), with **Vazirmatn (Vazir) font mandatory in `fa` RTL mode**.

---

## 1. Executive Summary

The app currently has **two competing design languages**:

| Area | Current look | Problem |
|------|--------------|---------|
| Public layout + home + albums + details | Token-based custom CSS (oklch palette, **coral** accent, Geist font) | Solid foundation, but coral accent and floating pill nav feel more "portfolio" than "encyclopedia" |
| Search, Auth, error pages | Raw Tailwind utilities (`blue-600`, `gray-200`) | A completely different visual language |
| Admin (≈100 views) | Dark `gray-900` sidebar, Font Awesome, Tailwind CDN | Third visual language; font config references Vazirmatn but the font is **never loaded** |
| RTL (`fa` / `ar`) | Public layout loads only **Geist** — no Persian font | **Violates the requirement: Vazir font must be used in `fa` RTL mode** |

**The redesign unifies everything onto one token-driven system**, guided by ui-ux-pro-max's recommendation for the *Wiki / Encyclopedia* product type:

> **Primary style: Minimalism + Flat Design · Landing pattern: Search-First + Hierarchical Navigation · Palette: clean white + link blue + heading hierarchy + citation grey**

---

## 2. Design Direction (ui-ux-pro-max analysis)

Product match — **Wiki / Encyclopedia**:

- **Style:** Minimalism + Flat Design (secondary: Swiss Modernism 2.0, Accessible & Ethical)
- **Landing pattern:** Search-First + Hierarchical Navigation → the search box is the hero element
- **Palette:** clean white surfaces, **link blue** accent, strong heading hierarchy, muted citation grey
- **Anti-patterns to avoid:** poor navigation, no search, decorative-only motion, color-only status indicators

Derived design decisions:

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Accent color | **Refined link blue** `#2563EB` (replaces coral) | Encyclopedia convention; blue = actionable links; strongest affordance for a reference site |
| Surfaces | White cards on `#F8FAFC` paper | Knowledge-base palette from ui-ux-pro-max |
| Nav | Clean sticky top bar with hairline border (replaces floating pill) | Encyclopedia-appropriate; predictable; easy scan |
| Search | **Search-first hero** — large search centered at top of home | ui-ux-pro-max: Search-First landing pattern |
| Typography | **Vazirmatn (Vazir) for `fa`/`ar` RTL · Inter for `en`/`fr` LTR** | Vazirmatn is the maintained successor of the Vazir font (same designer, Google Fonts); designed to pair with Inter-like faces |
| Admin | Light sidebar (white, hairline border) instead of dark gray | Flat, minimal, consistent with public site |
| Icons | Single inline SVG set (Lucide/Heroicons style) in public; Font Awesome retained in admin | No emoji-as-icon, consistent stroke weight |
| Motion | 150–300ms, ease-out, `prefers-reduced-motion` respected | ui-ux-pro-max rule 7 |

---

## 3. Target Design System

### 3.1 Color tokens (replaces current oklch coral set)

```css
:root {
  /* Surfaces */
  --color-paper:         #F8FAFC;   /* page background */
  --color-paper-2:       #F1F5F9;   /* subtle background */
  --color-surface:       #FFFFFF;   /* cards, panels */
  --color-surface-2:     #F8FAFC;   /* nested surface */

  /* Ink */
  --color-ink:           #1E293B;   /* primary text */
  --color-ink-2:         #475569;   /* secondary text */
  --color-ink-3:         #64748B;   /* muted / citation text */

  /* Rules */
  --color-rule:          #E2E8F0;   /* borders */
  --color-rule-light:    #EDF1F5;   /* subtle borders */

  /* Accent — link blue */
  --color-accent:        #2563EB;
  --color-accent-hover:  #1D4ED8;
  --color-accent-ink:    #FFFFFF;
  --color-accent-soft:   #EFF6FF;   /* tinted backgrounds (badges, icons) */

  /* Semantic */
  --color-success:       #16A34A;  --color-success-soft: #F0FDF4;
  --color-warning:       #D97706;  --color-warning-soft: #FFFBEB;
  --color-error:         #DC2626;  --color-error-soft:   #FEF2F2;
  --color-info:          #2563EB;  --color-info-soft:    #EFF6FF;

  /* Focus */
  --color-focus-ring:    oklch(0.55 0.2 260 / 0.35);
}
```

All contrast pairs verified ≥ 4.5:1 (WCAG AA): ink on paper, ink on surface, ink-2 on white, accent on white.

### 3.2 Typography tokens — Vazir/Vazirmatn in RTL

```css
:root {
  --font-display-ltr: "Inter", system-ui, -apple-system, sans-serif;
  --font-body-ltr:    "Inter", system-ui, -apple-system, sans-serif;
  --font-display-rtl: "Vazirmatn", "Vazir", Tahoma, sans-serif;   /* fa / ar */
  --font-body-rtl:    "Vazirmatn", "Vazir", Tahoma, sans-serif;
  --font-mono:        "JetBrains Mono", ui-monospace, monospace;
}

/* Direction-aware font selection — THE key RTL rule */
html[dir="rtl"] body,
html[dir="rtl"] button, html[dir="rtl"] input, html[dir="rtl"] select, html[dir="rtl"] textarea {
  font-family: var(--font-body-rtl);
}
html[dir="rtl"] h1, html[dir="rtl"] h2, html[dir="rtl"] h3,
html[dir="rtl"] .font-display {
  font-family: var(--font-display-rtl);
}
```

**Type scale (major third 1.25, base 16px, line-height 1.6):** keep the existing `--text-*` scale. In RTL set `letter-spacing: 0` (negative tracking breaks Persian script).

**Font loading strategy (Phase 1):**

1. **Self-host woff2** into `wwwroot/fonts/vazirmatn/` — weights 400/500/600/700 (download from `https://cdn.jsdelivr.net/npm/vazirmatn@33.0.3/fonts/webfonts/Vazirmatn-Regular.woff2` etc. or the `@fontsource-variable/vazirmatn` package), define `@font-face` in `site.css`.
2. Same for **Inter** (weights 400/500/600/700) in `wwwroot/fonts/inter/`.
3. Remove the Geist Google Fonts link; remove per-page font tags.
4. Admin: replace the current `tailwind.config` `fontFamily.sans` with the same CSS variables so it inherits the RTL switch for free.

> **Note on "Vazir" vs "Vazirmatn":** Vazirmatn is the official maintained successor of the original Vazir font (same author, Ali Tofighi; available on Google Fonts). The plan uses **Vazirmatn** and keeps the legacy `"Vazir"` family name in the fallback chain. If the exact legacy "Vazir" face is required instead, only the `@font-face` src lines change — no view markup changes.

### 3.3 Spacing, radius, motion, shadows

- **Spacing:** keep the 4pt scale (`--space-3xs` … `--space-3xl`); add `--space-section: 4.5rem` for page rhythm.
- **Radius:** `--radius-sm: 6px · --radius-md: 8px · --radius-lg: 12px · --radius-pill: 9999px` (flat design → modest radii).
- **Shadows:** hairline borders + minimal shadows: `--shadow-card: 0 1px 2px rgb(15 23 42 / 0.04)`, `--shadow-pop: 0 8px 24px rgb(15 23 42 / 0.08)`.
- **Motion:** `--dur-micro: 150ms · --dur-short: 220ms · --dur-long: 300ms`; easing `cubic-bezier(0.2, 0, 0, 1)`; hover = subtle lift/border tint; all animations wrapped in `@media (prefers-reduced-motion: reduce)` (already present — keep).

### 3.4 Component inventory (shared partials)

| Component | New spec |
|-----------|----------|
| **Nav** | Sticky top bar: white/95 blur, hairline bottom border. Start → brand + primary links; end → search icon, language switcher, menu button. Mobile: slide-in drawer (RTL: slides from inline-end). Replace N5 floating pill. |
| **Language switcher** | Keep `select` in a compact pill; ensure native `fa`, `en`, `ar`, `fr` labels render in their own scripts. |
| **Hero (home)** | Search-first: brand statement (short), large centered search input, subtle hint chips (genres/moods/instruments). |
| **Section header** | Title + muted subtitle + "View all" link (logical arrow: flips in RTL via `scaleX(-1)`). |
| **Card** | White surface, 1px `--color-rule` border, radius-md, image 1:1; hover: border → accent, image scale 1.02. No heavy shadow. |
| **Track row** | Number chip + title + meta + duration; hover surface tint. |
| **Tag / badge** | Pill, `--color-paper-2` bg; accent variant `--color-accent-soft` + accent text. |
| **Buttons** | `.btn` primary = ink; `.btn--accent` = link blue; `.btn--secondary` = surface + border; min-height 40px; focus ring visible. |
| **Breadcrumb** | Hairline-separated, `text-sm`, logical separators. |
| **Pagination** | Shared partial (`_Pagination.cshtml`) — used by public **and** admin; logical prev/next arrows; active = ink. |
| **Filter panel** | `.filter` card; labeled fields (visible labels, not placeholder-only); Apply / Clear. |
| **Data table** | Muted header row (uppercase 11px), row hover tint, `overflow-x-auto` wrapper, no zebra. |
| **Detail sidebar** | Sticky `--space` card, label/value pairs, `dir="auto"` on mixed-language values. |
| **Alerts** | Success/Error/Info/Warning banner partial with dismiss button — one implementation for public + admin (replaces 4 inline TempData blocks in admin). |
| **Empty state** | SVG icon + title + text + action link. |
| **Footer** | Single-line: brand + copyright + links; hairline top border. |
| **JSON-LD** | Unchanged (SEO/structured data untouched). |

### 3.5 RTL strategy (`fa`, `ar`)

1. **Font:** Vazirmatn/Vazir automatically applied via the `html[dir="rtl"]` rules (Section 3.2).
2. **Layout:** all spacing/positioning uses **logical properties** (`margin-inline-start`, `inset-inline-end`, `border-inline-start`, `text-align: start`). Sweep every view for physical `left/right/margin-left` etc.
3. **Icons:** directional icons (chevrons, arrows) flip with `[dir="rtl"] .icon-directional { transform: scaleX(-1) }` — replaces the manual `if (isRtl)` SVG-switching in Search view.
4. **Mixed content:** `dir="auto"` on user content (titles, aliases, original titles) — already partially used; standardize.
5. **Numbers:** keep Western digits with `font-variant-numeric: tabular-nums` (standard for Persian web); keep `en`/`fa` digit handling unchanged.
6. **Hero/text:** replace `.hero { text-align: left }` with `text-align: start`.
7. **site.js:** stop hardcoding `fa` — read culture from `document.documentElement.lang`; back-to-top button uses `inset-inline-end`.

---

## 4. Page Inventory & Redesign Scope

### 4.1 Public site (27 views)

| Group | Views | Effort | Notes |
|-------|-------|--------|-------|
| Shell | `_Layout`, `_ViewImports`, `site.css`, `site.js` | High | Foundation work (Phase 1–2) |
| Home | `Home/Index` | High | Search-first hero redesign |
| Lists | `Albums, Tracks, People, Artists, Poets, Poems, Companies, Genres, Moods, Instruments, Locations, Sessions, Events, Charts, Awards, Publications, Sources, SungVersions` — `Index` | Medium | Standardized list header + grid + pagination partial |
| Details | Same groups — `Detail` | Medium–High | Album detail = reference template (tracklist, credits, sidebar, tables, media) |
| Search | `Search/Index` | Medium | Convert Tailwind utilities → tokens; flip icon via CSS; badges via token system |
| Auth | `Login, Register, ForgotPassword, ForgotPasswordConfirmation, AccessDenied` | Low | Convert to token classes; centered card layout |
| Errors | `Error, NotFound` | Low | Minimal centered states |

### 4.2 Admin area (≈100 views)

| Group | Views | Effort | Notes |
|-------|-------|--------|-------|
| Shell | `_AdminLayout` | High | Light sidebar + topbar; token font fix; unified alerts partial |
| Dashboard | `Dashboard/Index` | Medium | Stat cards → flat token cards |
| Lists | All `Index` views (~25 entities) | Medium | Shared table styling + pagination partial + search bar component |
| Forms | All `Create/Edit` (≈50) | High | Standardized field classes via CSS (`.field`, `.input`, `.select`, `.checkbox`) so existing markup only needs class swaps |
| Complex editors | `_AlbumTracklistEditor, _CreditEditor, _AliasEditor, _AttributeValueEditor, _RelatedItemsEditor` | High | Keep behavior; restyle |
| Misc | `AuditLogs, Settings, Users, Roles, LookupTables, Media, Localizations, Links` | Medium | Same table/form treatment |

---

## 5. Implementation Roadmap

> Order matters: tokens → shell → shared partials → public pages → admin. Each phase ends with a **build + visual check**.

### Phase 0 — Baseline & guardrails
- [ ] `git` branch `redesign/ui-v2`; confirm app builds and runs locally.
- [ ] Save screenshots of current key pages (home, albums, album detail, search, admin dashboard) for before/after.

### Phase 1 — Design foundation
- [ ] Rewrite `wwwroot/css/site.css` tokens (Section 3.1–3.3); keep existing class names where possible to minimize view churn.
- [ ] Add self-hosted **Vazirmatn + Inter** `@font-face`; add `html[dir="rtl"]` font rules.
- [ ] Create `wwwroot/css/admin.css` (admin tokens + component classes) so admin stops depending on inline Tailwind for structure.
- [ ] Add shared partials: `_Alerts`, `_Pagination`, `_EmptyState`.
- [ ] Update `design.md` to the new system (or link to this plan).
- **Accept:** build passes; both `fa` and `en` render; Vazirmatn visible in `fa` in DevTools (Computed → font-family).

### Phase 2 — Public shell
- [ ] `_Layout.cshtml`: new top-bar nav + mobile drawer + lang switcher; remove Geist link; keep hreflang/JSON-LD sections.
- [ ] `site.js`: culture from `<html lang>`; directional-icon handling; keep debounce/back-to-top with logical positions.
- [ ] Home `Index`: search-first hero, sections with new cards.
- **Accept:** nav works at 375/768/1024/1440; drawer opens/closes; lang switch round-trips; `fa` shows Vazirmatn.

### Phase 3 — Public content pages
- [ ] List pages: standardized header, card grid, pagination partial, filter panel.
- [ ] Detail pages: Album detail as template; apply to People, Track, Poem, Company, etc.
- [ ] Search page: full token conversion + CSS-flip icons + token badges.
- **Accept:** no `blue-600`/`gray-*` utility remnants in public views (grep check); no horizontal scroll on 375px.

### Phase 4 — Auth & errors
- [ ] Login/Register/ForgotPassword: centered card, token inputs, visible labels.
- [ ] Error/NotFound: minimal centered states.
- **Accept:** keyboard-only login flow; focus rings visible.

### Phase 5 — Admin shell & dashboard
- [ ] `_AdminLayout`: light sidebar (hairline border, token colors), topbar with breadcrumb, Vazirmatn loads in RTL, alerts via `_Alerts` partial, Font Awesome retained.
- [ ] Dashboard: flat stat cards + tables.
- **Accept:** admin usable in `fa` (Vazirmatn) and `en`; sidebar toggle works in both directions.

### Phase 6 — Admin CRUD
- [ ] Table pages: shared `.data-table` styling, pagination partial, search bar, bulk actions bar.
- [ ] Forms: define `.field/.label/.input/.select/.checkbox` in admin.css; sweep all Create/Edit views to swap utility classes for tokens.
- [ ] Complex editors (`_AlbumTracklistEditor` etc.): restyle while preserving markup/behavior.
- **Accept:** every admin list/form view passes a token-consistency grep; CRUD round-trip works for Albums.

### Phase 7 — QA & hardening
- [ ] **RTL pass:** verify every page in `fa` and `ar` — fonts, alignment, arrows, spacing, drawer.
- [ ] **LTR pass:** `en` and `fr`.
- [ ] **Accessibility:** WCAG AA contrast, keyboard navigation, focus states, `prefers-reduced-motion`, aria-labels (ui-ux-pro-max checklist).
- [ ] **Responsive:** 375 / 768 / 1024 / 1440; no horizontal scroll; CLS < 0.1.
- [ ] **Performance:** lazy-load images (already present), preload fonts, remove Geist request.
- [ ] (Optional, follow-up) Move Tailwind CDN → built `tailwind.css` via npm/Tailwind CLI for production.
- [ ] Update `design.md`; capture after-screenshots.

---

## 6. Validation Commands

```bash
# Build
dotnet build MusicEncyclopedia.sln

# Tests
dotnet test tests/MusicEncyclopedia.Core.Tests
dotnet test tests/MusicEncyclopedia.Services.Tests
dotnet test tests/MusicEncyclopedia.Web.Tests

# Run locally (verify visually)
dotnet run --project src/MusicEncyclopedia.Web
# → http://localhost:5000/fa  (default RTL — expect Vazirmatn)
# → http://localhost:5000/en  (LTR — expect Inter)
```

Consistency greps (run at end of Phases 3 & 6):
```bash
grep -rn "blue-600\|gray-200\|text-gray" src/MusicEncyclopedia.Web/Views --include=*.cshtml | grep -v Areas/Admin
# Admin area may keep neutral grays only via tokens; no hard-coded palette hexes.
grep -rn "font-family:.*Geist" src/MusicEncyclopedia.Web/Views
```

---

## 7. Acceptance Criteria (Definition of Done)

- [ ] One token system drives **all 132 views**; no raw Tailwind color utilities remain in public views.
- [ ] **`fa` and `ar` render in Vazirmatn (Vazir)** — verified in DevTools and visually; `en`/`fr` render in Inter.
- [ ] Search-first home; hierarchical nav; predictable pagination.
- [ ] WCAG AA contrast, keyboard navigable, focus rings, reduced-motion respected.
- [ ] Responsive at 375–1440px with no horizontal scroll.
- [ ] Admin redesigned to the same flat/minimal language; all CRUD flows functional.
- [ ] All tests green; `dotnet build` clean; SEO tags (hreflang, canonical, JSON-LD) untouched.

---

## 8. Risks & Notes

| Risk | Mitigation |
|------|------------|
| 100 admin views = large surface | Token classes + shared partials; sweep in batches; CSS-first so most changes are class swaps |
| Persian font affects layout height | Vazirmatn metrics are Inter-compatible; test line clamps (`-webkit-line-clamp`) in `fa` |
| Keeping Tailwind CDN | Retained initially for speed; optional Phase 7.7 moves to a built CSS file |
| Design-system drift | `design.md` updated in Phases 1 and 7; greps as regression checks |
