# B09 UI completion

Status: Done on 2026-09-13. Owner task: `IMPLEMENTATION_PLAN.md` B09. B10 subsequently completed the accessibility and responsive verification described below.

## Contract

Keep the approved minimal, content-first blue interface and all existing routes, model bindings, and editor behavior. Persian (`fa`) and RTL remain the only launch presentation. B09 standardizes visual vocabulary; B10 subsequently completed the accessibility and responsive verification pass.

## Implementation

- Define the Tailwind semantic palette once in `wwwroot/js/tailwind-theme.js`, backed by the CSS variables in `site.css`.
- Use `paper`, `surface`, `ink`, `rule`, `accent`, `success`, `warning`, `error`, and `info` roles in Razor views instead of framework palette names.
- Share `site.css` between public, authentication, and admin layouts so typography, focus, form, state, and alert rules have one source.
- Consolidate repeated controls into `form-label`, `form-control`, and `field-error` components.
- Use `ui-state` modifiers for empty, loading, and error feedback. Preserve visible text or icons so status never relies on color alone.
- Apply the approved light admin navigation surface and semantic active, hover, logout, overlay, and scrollbar colors.

## Verification

- A convention test scans every public/auth/admin Razor view and rejects named raw palette utilities.
- Public and admin layouts must load the same semantic Tailwind configuration and token stylesheet.
- Existing SQL-backed public navigation, authentication, authorization, admin CRUD, wizard, cache, API, and home tests must remain green.
- Browser smoke checks cover representative public, auth, admin list, admin form, empty, validation, and loading states. Full keyboard, contrast, reduced-motion, and viewport evidence is recorded in `docs/B10_PERSIAN_ACCESSIBILITY_PLAN.md`.

## Completion evidence

- The source audit reports zero raw named Tailwind palette utilities in all public, authentication, and admin Razor views; two regression tests enforce the convention and shared layout configuration.
- The common form migration produced 349 `form-control` uses, 227 field-level validation components, and shared empty/loading/error state classes without changing tag-helper bindings.
- Core (5), Data (1), Services (42), and Web (36) passed in Release: 84 tests, zero failures or skips.
- Live browser checks on the home and login pages passed at 1280×720 and 375×812. Both resolved `lang=fa`, `dir=rtl`, Vazirmatn, semantic blue actions and 44px controls, with no horizontal overflow or console errors.
- Authenticated admin routes and writes remain covered by the SQL-backed B04/B07 integration suite. The admin layout consumes the same token sheet and uses the approved light sidebar; B10 completed the interactive accessibility review.
