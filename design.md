# Music Encyclopedia · Design System

> Hallmark v1.1.0 · modern-minimal · Ecosystem Index · Coral theme

---

## Color Palette

| Token | Value | Use |
|-------|-------|-----|
| `--color-paper` | `oklch(97% 0.008 70)` | Page background |
| `--color-paper-2` | `oklch(94% 0.010 70)` | Subtle background |
| `--color-surface` | `oklch(100% 0 0)` | Card/panel background |
| `--color-surface-raised` | `oklch(99% 0.004 70)` | Elevated surface |
| `--color-ink` | `oklch(18% 0.010 60)` | Primary text |
| `--color-ink-2` | `oklch(40% 0.008 60)` | Secondary text |
| `--color-ink-3` | `oklch(56% 0.006 60)` | Tertiary/muted text |
| `--color-rule` | `oklch(88% 0.008 70)` | Borders |
| `--color-rule-light` | `oklch(92% 0.006 70)` | Subtle borders |
| `--color-accent` | `oklch(62% 0.17 25)` | Coral accent |
| `--color-accent-ink` | `oklch(100% 0 0)` | Text on accent |
| `--color-focus` | `oklch(62% 0.17 25)` | Focus rings |

## Typography

| Token | Value |
|-------|-------|
| Display | Geist, Inter, system-ui |
| Body | Geist, Inter, system-ui |
| Mono | Geist Mono, JetBrains Mono |

Scale (major third 1.25):
- `--text-xs`: 0.64rem
- `--text-sm`: 0.8rem
- `--text-base`: 1rem
- `--text-md`: 1.25rem
- `--text-lg`: 1.5625rem
- `--text-xl`: 1.9531rem
- `--text-2xl`: 2.4414rem
- `--text-display`: clamp(2rem, 4vw + 0.5rem, 3.5rem)

## Spacing

4pt scale: `--space-3xs` (0.125rem) through `--space-3xl` (6rem).

## Components

- **Nav**: N5 floating pill — sticky, centered, frosted glass
- **Footer**: Ft2 inline single line — minimal, border-top only
- **Cards**: hover lift with subtle shadow
- **Tags**: pill-shaped, muted background
- **Buttons**: pill-shaped, ink primary, paper secondary

## Motion

- Micro: 120ms (hover, focus)
- Short: 220ms (transforms)
- Long: 420ms (image zoom)
- Easing: `cubic-bezier(0.16, 1, 0.3, 1)` out
- Respects `prefers-reduced-motion`

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
8. RTL-aware spacing
