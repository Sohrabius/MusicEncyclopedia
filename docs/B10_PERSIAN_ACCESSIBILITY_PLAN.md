# B10 Persian accessibility and RTL

Status: Done on 2026-09-13. Owner task: IMPLEMENTATION_PLAN.md B10.

## Launch contract

Persian (fa) is the only launch culture. Public and admin pages render right-to-left with the self-hosted Vazirmatn family. The existing en, ar, and fr resources and redirects remain dormant infrastructure; the UI exposes no language switcher and makes no multilingual release claim.

## Implemented hardening

- Added keyboard-visible skip links and focus outlines to the public and admin shells.
- Made public mobile navigation and the admin sidebar report expanded/hidden state, move focus into the opened menu, keep Tab focus inside it, close on Escape or backdrop activation, and return focus to the trigger.
- Corrected the admin sidebar mobile transform so it opens from the RTL inline-start edge.
- Made closed off-canvas navigation inert so keyboard users cannot reach invisible links.
- Added polite live announcements to all 227 field validation messages; validation summaries retain alert semantics.
- Converted view focus utilities to focus-visible behavior and replaced remaining physical margin utilities with logical RTL utilities.
- Hid decorative Font Awesome glyphs from assistive technology and gave icon-only admin buttons Persian accessible names and 44px minimum targets.
- Localized and enlarged the back-to-top button and made scripted scrolling honor reduced-motion preference. The album wizard uses the same motion rule.
- Removed the case-insensitive duplicate Create-account resource keys that produced MSB3568.

## Automated evidence

UiConventionTests now enforce:

- semantic colors and the shared theme across public and admin views;
- Persian RTL shell landmarks, skip links, menu state, Escape support, inert navigation, reduced-motion behavior, and localized controls;
- keyboard-only focus utilities and live validation announcements;
- decorative icon hiding, accessible names, and 44px icon-button targets;
- case-insensitive resource-key uniqueness;
- WCAG AA contrast of normal text tokens against the primary surface.

The full Release solution suite passed on 2026-09-13:

- Core: 5
- Data: 1
- Services: 42
- Web: 40
- Total: 88, with no failures or skips

## Browser evidence

The seeded LocalDB application was checked in the in-app Chromium browser on:

- home: /fa
- public catalog: /fa/albums
- authentication: /auth/login
- admin dashboard: /admin
- album wizard: /admin/albums/wizard

Each surface passed at 375, 768, 1024, and 1440 CSS pixels with lang=fa, dir=rtl, Vazirmatn, a main landmark and skip link, and no horizontal overflow. The admin sidebar was hidden at 375/768 and exposed at 1024/1440. Public and admin mobile menus moved focus inside, closed with Escape, restored focus to their triggers, and updated ARIA state. The browser console contained no warnings or errors.

## Remaining scope

CLS and other performance measurements belong to B12. Container and production-environment accessibility smoke checks belong to B13.
