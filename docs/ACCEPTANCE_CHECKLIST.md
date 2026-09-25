# Release Acceptance Checklist

Updated: 2026-09-13. Source: music-encyclopedia-q.md §36 plus the user-approved Persian-only launch scope recorded in IMPLEMENTATION_PLAN.md.
Historical claims are not fresh evidence. Owning task IDs refer to IMPLEMENTATION_PLAN.md. Multilingual delivery is a future milestone and is not a release blocker.

| # | Criterion | Tasks | Required evidence | Status |
|---|---|---|---|---|
| 1 | Public pages render | B06, B08 | All implemented entity route families render seeded content; home SQL boundaries and fa/RTL browser checks pass | Pass |
| 2 | Admin CRUD works | B07 | All 29 primary GET routes render; wizard, album edit, shared tracklist, credits, media assignment and attribute definitions have current persistence/validation evidence | Pass |
| 3 | Persian-only content | B03, B06, B10 | Database-backed fa overlay and fallback passed; en/ar/fr redirect to fa by approved launch policy | Pass |
| 4 | RTL layout | B10 | Persian font, logical layout, keyboard and responsive checks in fa | Pass |
| 5 | Album-track many-to-many | B07 | Shared track, two discs, gap-free order, title/duration overrides, duplicate rejection and removal passed | Pass |
| 6 | Credit roles and contributors | B03, B07 | Person/company XOR, role scope, entity/type and exact duplicate checks passed | Pass |
| 7 | Musician instruments | B06, B07 | Wizard saves instrument classification and musician credit; SQL graph and public projection verified | Pass |
| 8 | Poem/sung-version separation | B06, B07 | Relationally distinct poem/sung-version public/API rendering and wizard creation passed | Pass |
| 9 | Lyrics availability | B04, B05 | Registered/restricted API access passed for anonymous and permitted principals; all lyrics responses are no-store | Pass |
| 10 | Media upload/assignment | B07, B13 | Valid assignment and invalid empty upload passed; production file persistence/restart remains B13 | In progress |
| 11 | Citations display | B06, B07 | Admin-created citation appears publicly | Unverified |
| 12 | Tags/aliases/localizations | B05–B07 | Persistence, fallback and refreshed public content | Unverified |
| 13 | Relevant search | B06, B11 | LIKE result/type/paging and deletion visibility passed; `VerifyFullTextSearch.sql` and docs/B11_SEARCH_OPERATIONS_PLAN.md define the required ranked FTS, paging and automatic-change-tracking proof | In progress |
| 14 | SEO metadata | B12 | Canonical, fa/x-default hreflang, sitemap and parseable structured data | Unverified |
| 15 | Admin authorization | B04 | Anonymous redirect, authenticated denial, permitted view/write and antiforgery checks passed | Pass |
| 16 | Soft delete | B04–B06 | Public MVC/API hiding, warm cache deletion/restoration, and warmed search removal passed | Pass |
| 17 | Performance | B12 | Measurements and reproducible dataset/device/load conditions | Unverified |
| 18 | Security requirements | B04, B07, B13 | Spec §17 mapping: input/SQL handling, access, CSRF, uploads, headers, cookies and rate limits | Unverified |
| 19 | Tests pass | B01–B10, B13 | Local Release suite passes 88 substantive cases; CI/container run remains B13 | In progress |
| 20 | Production deployment | B13 | Disposable production-config staging deployment, readiness and recovery rehearsal | Unverified |

## Evidence entry template

- Criterion / task:
- Date / revision (if available):
- Environment / data fixture:
- Command or walkthrough:
- Expected / actual:
- Result: Pass / Fail / Blocked
- Evidence file or CI run:
- Remaining defect and owner task:

Do not mark a criterion passed while its required tests are skipped or its environment is unavailable. Production deployment acceptance can be rehearsed in staging; this checklist does not require publishing to a live user-facing system.
