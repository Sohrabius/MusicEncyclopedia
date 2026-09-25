# MusicEncyclopedia - Lapis Archive Redesign Plan

## Status and scope

**Status:** Approved design direction; Phases 2 and 4 are in progress. The Phase 0 banner asset decision remains open.

This plan replaces the visual direction of the completed minimal blue UI work without changing public URLs, Razor view models, controllers, search behavior, SEO metadata, JSON-LD, or the Persian (`fa`) RTL launch scope.

The target is a restrained music archive, not a streaming-app clone: album covers and source imagery do the visual work; the interface makes the catalog easy to search, scan, and connect.

## Approved design brief

- **Theme:** Lapis Archive.
- **Base colors:** paper and ink. Lapis, teal, and amber are reserved for navigation, relationships, interactive states, and small emphasis.
- **Typography:** retain the self-hosted Vazirmatn family for Persian RTL. Do not add a display font merely for decoration.
- **Density:** quiet, content-first, and intentionally sparse. Avoid ornamental patterns, repeated decorative labels, heavy shadows, and decorative motion.
- **Artwork:** album covers carry a meaningful share of the page personality. Cards should not compete with their artwork.
- **Home composition:** a fixed-height photographic banner first, followed by a simple title and search field. Remove the home eyebrow and descriptive tagline. The image is fixed in composition and crop, not `background-attachment: fixed` parallax, which is unreliable and expensive on mobile.
- **Dependencies:** a library may be added only when it resolves a concrete interaction or build problem. The redesign itself should be achievable with the existing Razor, CSS, and inline SVG stack.

## Design system

### Color tokens

Update the existing semantic tokens in `src/MusicEncyclopedia.Web/wwwroot/css/site.css`; do not scatter raw hex values through Razor views.

| Token role | Value | Use |
| --- | --- | --- |
| Paper | `#F4F6F5` | Page background and quiet content bands |
| Surface | `#FFFFFF` | Content panels, inputs, and readable data surfaces |
| Ink | `#17212F` | Primary text, primary buttons, and compact navigation |
| Muted ink | `#5B6A7D` | Secondary text and citation metadata |
| Lapis | `#174A85` | Search, selected navigation, primary links, key calls to action |
| Deep lapis | `#12345F` | Banner image overlay and dark brand moments only |
| Teal | `#176B70` | Secondary relationship states and information accents |
| Amber | `#C77718` | Listening/action emphasis and focus details, used sparingly |
| Rule | `#D5DCE3` | Dividers and quiet control borders |

Semantic success, warning, error, information, and focus tokens remain explicit. Every text/background and focus combination must meet WCAG AA contrast requirements.

### Layout and typography

- Retain the existing four-point spacing scale and CSS logical properties.
- Keep content columns readable: prose under 70 characters, catalog grids driven by image aspect ratio rather than oversized cards.
- Keep border radii modest. Use rules and surface changes before adding shadows.
- Use Vazirmatn weights, sizing, and line height to create hierarchy. Persian headings must retain zero letter-spacing.
- Preserve the existing visible keyboard focus and reduced-motion support.

### Shared visual primitives

Extend the existing `site.css` component layer with reusable semantic classes. The intended primitives are:

- `.page-frame`: maximum width and horizontal page padding.
- `.page-heading`: a consistent list/detail heading block without decorative eyebrows.
- `.home-hero`: photographic banner plus title/search block.
- `.catalog-layout`: responsive catalog and filter layout.
- `.catalog-card`: cover-led result card with metadata below the cover.
- `.metadata-list`: compact label/value presentation for details and sidebars.
- `.content-section`: detail-page section rhythm and heading rule.
- `.media-grid`: responsive, aspect-ratio-safe cover and gallery grid.

The current `.nav`, `.search`, `.card`, `.tag`, `.filter`, `.track-row`, `.data-table`, and alert components should be evolved where their purpose already matches. Avoid parallel components that only differ by color.

## Asset plan

The repository currently has no web image assets for a home banner. Add a local, licensed asset pipeline before implementing the banner:

1. Select one photographic image that communicates Iranian/Persian music and has explicit web-use permission. Do not hotlink a third-party image.
2. Record attribution, license, original source, and any required credit in `docs/ASSET_CREDITS.md`.
3. Create `src/MusicEncyclopedia.Web/wwwroot/images/home/` and store responsive AVIF or WebP derivatives at 768px, 1280px, and 1920px widths.
4. Serve the image with `<picture>`, `width`, `height`, descriptive Persian alt text, and a stable `object-position` chosen after visual review.
5. Use a neutral paper/ink fallback surface when the image fails to load. The text and search must remain readable without the image.

## Home page structure

`Views/Home/Index.cshtml` becomes the first reference implementation.

```text
Nav
Main
  Home hero
    Responsive fixed-height banner image
    Simple title
    Search field
  Album catalog
    Category filters
    Cover-led album grid
    Existing progressive loading and pagination behavior
  Supporting content
    Random poem and archive statistics, only when data is present
Footer
```

Implementation rules:

- The banner has a fixed responsive height, not a viewport-filling hero. At narrow widths it remains a short, purposeful image rather than pushing search below the fold.
- Place the title and search beneath the banner on a paper surface. Do not add an eyebrow, explanatory paragraph, decorative gradient, or hint chip row in this area.
- Keep category filters and progressive album loading exactly functional as they are now.
- Use `loading="eager"` and `fetchpriority="high"` only for the single banner image. Album artwork remains lazy-loaded with intrinsic dimensions.
- Keep the random poem and archive information visually quieter than the album catalog; hide either block when its data is absent.

## Reference page templates

Build these pages before migrating the rest of the public catalog. They define the component contract for every subsequent view.

### 1. Home

- Files: `Views/Home/Index.cshtml`, home partials, `wwwroot/css/site.css`, and `wwwroot/js/home.js` only where existing interaction needs markup updates.
- Proves: banner asset handling, primary search, catalog grid, sidebar hierarchy, responsive behavior, and home loading states.

### 2. Album list

- File: `Views/Albums/Index.cshtml`.
- Proves: `.page-heading`, responsive filters, cover-led results, empty state, and shared pagination.
- Keep search/filter query parameters and canonical metadata unchanged.

### 3. Album detail

- File: `Views/Albums/Detail.cshtml`.
- Proves: artwork-first header, metadata, track rows, source tables, related-cover grid, and a sticky detail sidebar.
- Replace inline visual styles with shared classes while retaining mixed-language `dir="auto"` handling and all JSON-LD output.

## Implementation phases

### Phase 0 - Baseline and asset decision

1. Record screenshots of home, album list, album detail, artist list/detail, search, auth, and admin at 375, 768, 1024, and 1440 pixels.
2. Select and license the home banner image; create local responsive derivatives and the asset-credit record.
3. Confirm current build and test baseline before styling changes.

**Exit criteria:** approved banner asset, documented image license, known before-state screenshots, and no pre-existing build/test regression.

### Phase 1 - Theme foundation

1. Replace the token values in `wwwroot/css/site.css` with the Lapis Archive semantic palette.
2. Add the shared layout and component primitives described above.
3. Adjust the public shell in `Views/Shared/_Layout.cshtml` and shared navigation/footer styling so the new tokens are consistently visible.
4. Preserve the current mobile drawer behavior, skip link, focus treatment, and RTL logical spacing.

**Exit criteria:** navigation, forms, alerts, tables, empty states, and focus states render from semantic tokens with no raw palette values added to views.

### Phase 2 - Build the three reference pages

1. Build the new photographic home hero and simplify the title/search area.
2. Convert album list markup to the catalog template.
3. Convert album detail markup to shared content, metadata, and media primitives.
4. Remove the relevant inline visual styling from these reference views rather than overriding it with more specific CSS.

**Exit criteria:** the three reference pages are visually coherent, preserve existing behavior, and are approved at desktop and mobile widths before broader migration starts.

### Phase 3 - Public catalog migration

1. Migrate list pages in batches: people/artists/poets, tracks/poems/sung versions, then events/charts/awards/publications/sources and lookup catalogs.
2. Migrate detail pages using the album-detail component vocabulary, not a one-off page style for each entity.
3. Convert search, authentication, and error pages after the public content templates are stable.
4. Sweep inline `style` attributes and structural Tailwind utilities only where a shared semantic class can replace them without changing behavior.

**Exit criteria:** public views use the Lapis Archive primitives, cover and image layouts are responsive, and no migrated view creates horizontal overflow.

### Phase 4 - Admin alignment

1. Update `wwwroot/css/admin.css` to consume the same palette and form/table primitives while retaining admin-specific density.
2. Restyle admin shell, dashboard, tables, forms, and complex editor partials in that order.
3. Preserve every CRUD binding, validation message, permission behavior, and keyboard path.

**Exit criteria:** admin reads as part of the same product but remains fast and dense enough for editorial work.

### Phase 5 - Hardening and release evidence

1. Verify Persian RTL and mixed-language content on all reference templates.
2. Test keyboard navigation, skip link, drawer focus behavior, labels, errors, and reduced-motion behavior.
3. Test 375, 768, 1024, and 1440 pixel widths with no horizontal scroll.
4. Measure image weight, LCP, and CLS on home and an album detail page. Address measured regressions before adding unrelated libraries.
5. Update `design.md`, `docs/ACCEPTANCE_CHECKLIST.md`, and the active implementation backlog with evidence.

**Exit criteria:** the redesign has recorded build, test, accessibility, RTL, responsive, and performance evidence.

## Dependency and library policy

- Keep Razor, self-hosted fonts, existing JavaScript, and inline SVG as the default implementation stack.
- Replace the Tailwind CDN with a local generated build only if Phase 5 records a CSP or performance reason. This is already an existing deferred concern.
- Add an image gallery, lightbox, or icon library only after a specific page requires functionality the current stack cannot provide. Pin the version, add it to the project asset process, and include it in performance checks.
- Do not add a UI component framework merely to recolor existing components.

## Regression safeguards

- Preserve all culture-prefixed routes, current query-string filters, canonical URLs, hreflang output, and JSON-LD.
- Do not change the home catalog's existing hybrid loading and pagination contract.
- Keep image dimensions to prevent layout shift and lazy-load non-hero media.
- Retain `dir="auto"` on user-provided mixed-language titles, aliases, and metadata.
- Do not remove visible focus styles, minimum touch targets, or `prefers-reduced-motion` behavior.

## Validation commands

```powershell
dotnet build MusicEncyclopedia.sln
dotnet test tests/MusicEncyclopedia.Core.Tests
dotnet test tests/MusicEncyclopedia.Services.Tests
dotnet test tests/MusicEncyclopedia.Web.Tests
dotnet run --project src/MusicEncyclopedia.Web
```

Use browser inspection on `/fa`, `/fa/albums`, `/fa/albums/{slug}`, `/fa/artists`, `/fa/search`, the authentication routes, and representative admin pages. Record screenshots and measured results with the release evidence.
