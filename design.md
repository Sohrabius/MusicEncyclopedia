# Music Encyclopedia · Design System

> Hallmark v2 · minimal-flat · Encyclopedia · link-blue theme
> Full redesign plan: `UI_REDESIGN_PLAN.md`
> Launch scope: Persian (`fa`) only, RTL. Additional culture assets are dormant future infrastructure.

---

## Color Palette

| Token | Value | Use |
|-------|-------|-----|
| `--color-paper` | `#F8FAFC` | Page background |
| `--color-paper-2` | `#F1F5F9` | Subtle background |
| `--color-surface` | `#FFFFFF` | Card/panel background |
| `--color-ink` | `#1E293B` | Primary text |
| `--color-ink-2` | `#475569` | Secondary text |
| `--color-ink-3` | `#64748B` | Tertiary/muted text (citations) |
| `--color-ink-disabled` | `#94A3B8` | Decorative and disabled content |
| `--color-rule` | `#E2E8F0` | Borders |
| `--color-rule-light` | `#EDF1F5` | Subtle borders |
| `--color-rule-strong` | `#CBD5E1` | Inputs and emphasized dividers |
| `--color-accent` | `#2563EB` | Link blue accent |
| `--color-accent-hover` | `#1D4ED8` | Accent hover |
| `--color-accent-ink` | `#FFFFFF` | Text on accent |
| `--color-accent-soft` | `#EFF6FF` | Accent tint (badges, icons) |
| `--color-success` | `#15803D` | Success |
| `--color-warning` | `#B45309` | Warning |
| `--color-error` | `#DC2626` | Error |
| `--color-info` | `#2563EB` | Info |

All contrast pairs ≥ 4.5:1 (WCAG AA).

## Typography

| Token | Persian launch value |
|-------|------------------------|
| Display | **Vazirmatn (Vazir)** |
| Body | **Vazirmatn (Vazir)** |
| Mono | JetBrains Mono |

- Fonts are **self-hosted** in `wwwroot/fonts/` and declared in `wwwroot/css/fonts.css`.
- RTL font selection is automatic via `html[dir="rtl"]` rules — **no per-view font markup**.
- Vazirmatn is the maintained successor of the Vazir font (same designer). The legacy `Vazir` family name remains in the fallback chain.

Scale (major third 1.25):
- `--text-xs`: 0.64rem · `--text-sm`: 0.8rem · `--text-base`: 1rem
- `--text-md`: 1.25rem · `--text-lg`: 1.5625rem · `--text-xl`: 1.9531rem
- `--text-2xl`: 2.4414rem · `--text-display`: clamp(2rem, 4vw + 0.5rem, 3.25rem)

In RTL: `letter-spacing: 0` on headings.

## Spacing

4pt scale: `--space-3xs` (0.125rem) through `--space-3xl` (6rem), plus `--space-section` (4.5rem).

## Components

- **Nav**: sticky top bar — white/92 blur, hairline bottom border, brand + links + search; no language switcher in the launch UI
- **Hero**: search-first — statement + subtitle + prominent search + hint chips
- **Footer**: single line — brand + copyright + links
- **Cards**: flat — 1px rule border, hover lift with subtle shadow, image scale 1.03
- **Tags**: pill-shaped, paper-2 background; accent variant uses accent-soft
- **Buttons**: pill, ink primary (hover → accent), surface secondary; min-height 40px; visible focus ring
- **Alerts**: shared `_Alerts` partial — success/error/warning/info with dismiss
- **Pagination**: shared `_Pagination` partial — used by public and admin
- **Tables**: muted uppercase header, row hover tint, `overflow-x-auto`

## Motion

- Micro: 150ms (hover, focus) · Short: 220ms (transforms) · Long: 300ms (image zoom)
- Easing: `cubic-bezier(0.2, 0, 0, 1)` out
- Respects `prefers-reduced-motion`

## RTL Strategy (`fa`)

1. **Vazirmatn (Vazir) font** applied automatically via `html[dir="rtl"]` rules in `fonts.css`.
2. Logical properties only: `margin-inline-start`, `inset-inline-end`, `text-align: start`.
3. Directional icons flip with `[dir="rtl"] .icon-directional { transform: scaleX(-1) }`.
4. `dir="auto"` on mixed-language user content.
5. Tabular numbers via `font-variant-numeric: tabular-nums`.

## Voice & Copy

- H1: 7-9 words, concrete nouns, no buzzwords
- CTAs: verb + noun, < 5 words
- Microcopy: 8th-grade reading level
- No exclamation marks

## Slop Rules

1. No italic headers
2. No fake social proof / invented metrics
3. No motion that delays content
4. No color-only status indicators
5. Mobile-first responsive
6. Semantic HTML with ARIA where needed
7. Keyboard navigable
8. RTL-aware spacing and fonts
