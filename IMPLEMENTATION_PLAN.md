# MusicEncyclopedia — Active Implementation Plan

Updated: 2026-09-11. This is the authoritative backlog and execution order.
Product requirements: `music-encyclopedia-q.md` §§21, 36. Historical completion claims are preserved in `docs/archive/IMPLEMENTATION_PLAN_2026-09-11.md`; they are not current verification evidence.

## Status rules

- **Ready**: scoped and actionable once listed dependencies pass.
- **Verify**: implementation exists; validate before changing or declaring done.
- **Done**: acceptance evidence recorded against the current code.
- **Deferred**: outside the release scope.
- **Blocked**: record the actual missing prerequisite, never infer it from old notes.

Do not equate existing files, passing empty tests, or old screenshots with completion. When verification fails, add a focused defect under the owning task, fix it, and rerun the affected checks. Avoid rewriting working features.

## Current baseline

- Six .NET 8 projects: Core, Data, Services, Search, Media, Web; MVC/Razor, SQL Server, EF Core writes, Dapper reads, Identity, Hangfire, in-process cache.
- SDK resolution works: `dotnet --version` reports **8.0.425**, compatible with the 8.0.424 pin. No SDK downgrade or framework upgrade needed.
- 82 passing test cases: pagination/cache, slug handling, localization fallback, validation, SQL migration/seed, application startup, authentication/authorization, antiforgery, protected lyrics, authorized writes, warm-cache edit/delete/restore, public MVC/API route families, filters/search/paging, soft-delete visibility, high-risk admin/wizard workflows, and B08 home boundaries/cache/poem fallbacks.
- The SQL test harness uses a uniquely named disposable LocalDB database on Windows, an explicit `TEST_SQLSERVER_CONNECTION_STRING` when supplied, and a Testcontainers SQL Server on non-Windows hosts. LocalDB migration, lookup seed, application startup, and cleanup are verified. Docker execution is not yet verified.
- Home redesign and album wizard implementation exist. The launch scope is Persian (`fa`) only with RTL layout; `en`, `ar`, and `fr` remain dormant future infrastructure. UI token migration is incomplete.
- Fresh restore and Release build/test succeeded. The latest complete solution run passed all 82 cases with no failures or skips. Four seed/resource warnings are assigned to owning tasks in `docs/BASELINE_2026-09-11.md`; the empty test placeholders have been replaced.

## Ordered backlog

Priority P0 establishes trustworthy execution and protects data/access; P1 closes release requirements; P2 is optional follow-up. IDs remain stable when status changes.

| ID | Priority / category | Status | Work and scope | Depends on | Completion evidence |
|---|---|---|---|---|---|
| B01 | P0 / Baseline | Done | Restore, Release build, run existing tests; record errors/warnings and actual test count. Fix only blockers. | SDK resolved | Passed 2026-09-11; see docs/BASELINE_2026-09-11.md (warnings retained for follow-up). |
| B02 | P0 / Test infrastructure | Done | Add WebApplicationFactory host and isolated SQL Server test database; deterministic fixtures, reset/cleanup, replace empty Data/Web tests. Disable seed/demo jobs only in test host overrides. | B01 | Passed twice on 2026-09-11: unique LocalDB migration/lookup seed and WebApplicationFactory startup; 3 SQL-backed tests, generated-name deletion guard, no app database connection used. |
| B03 | P0 / Domain coverage | Done | Test pure domain rules: slug unicode/safety/length/reserved routes, localization fallback and culture-overlay decisions, album/track/credit validation. Retain pagination/cache tests. Lyrics authorization belongs to B04; SQL search normalization/DTO projections/date rendering belong to B06. | B01 | Passed 2026-09-11: 38 Services cases plus 5 Core cases; requested → base → English → empty asserted; ambiguous dual-contributor credits now rejected. |
| B04 | P0 / Access and content integrity | Done | Integration tests for anonymous admin redirect, authenticated permission denial, permitted admin access/write, antiforgery, registered/restricted lyrics, and soft-delete hiding. Delete/restore cache transitions remain in B05. | B02 | Passed 2026-09-11: 12 Web cases. Fixed availability-name/code mismatch that exposed restricted API lyrics; protected endpoint is no-store and enforces authentication/permission. |
| B05 | P0 / Cache correctness | Done | Exercise admin edit/delete/restore and localization/media/credit writes against warm MVC, home partial and API responses. Audit ResponseCache plus service invalidation; adopt one explicit freshness policy per endpoint. | B02 | Passed 2026-09-11: warm MVC/API/home edit-delete-restore sequence plus culture/query/dependent-key tests. Removed non-evictable positive response caching from mutable endpoints and fixed untracked album writes. |
| B06 | P1 / Public and API integration | Done | Album/track/person detail, all public route families, search/filter/pagination, culture fallback, API success/error/paging envelope. Check direct SQL projections against real schema. | B02, B04 | Passed 2026-09-11: relational catalog exercised every implemented MVC/API family with meaningful content; primary filters/paging/errors, fa localization overlay, LIKE search and deletion visibility verified. Fixed multiple direct-SQL schema defects plus album year and track artist filters. |
| B07 | P1 / Admin and wizard | Done | Verify CRUD inventory and adopt a consistent tracked EF write policy; test two-disc ordering/overrides, shared tracks, credits/person/company/instrument rules, duplicate rejection, concurrency, transaction rollback, media assignment and validation. Add the missing attribute-definition CRUD surface. | B02, B04 | Passed 2026-09-11: eight SQL-backed admin workflow cases, 29 authenticated admin routes, and full 74-case Release suite. See docs/B07_ADMIN_WIZARD_PLAN.md. |
| B08 | P1 / Home redesign | Done | Keep the approved hybrid catalog: render 20, append four pages through item 100, then expose page 6 onward. Add no-JS navigation, visible retry feedback and live-region announcements; fix metadata and the self-linking About CTA. Verify category counts/filtering, 0/20/21/100/101+ boundaries, direct page links, random-poem fallback/shuffle, and cached versus uncached content. Persian (`fa`) only. | B05, B06 | Passed 2026-09-11: eight focused SQL-backed cases, full 82-case Release suite, and live fa/RTL browser checks at 375/768/1024/1440. Deterministic ordering, page-6-only tail, no-JS navigation, home-cache invalidation and poem fallbacks verified. |
| B09 | P1 / UI completion | Ready | Preserve the approved minimal, content-first blue design. Replace raw palette utilities and inconsistent forms with semantic tokens across public/auth/admin views; standardize loading, empty, error and retry states while preserving routes and editor bindings. Current audit: raw palette utilities occur in about 40/52 public and 86/88 admin view files. | B06, B07 | No raw public palette utilities; documented admin exceptions only where intentional; representative CRUD and public navigation still pass. |
| B10 | P1 / Persian accessibility and RTL | Verify | Treat Persian (`fa`) as the only launch culture. Verify Vazirmatn, RTL logical layout, labels, keyboard/focus behavior, errors, contrast, reduced motion and responsive widths 375/768/1024/1440 across home, public pages, auth, admin and wizard. Keep `en`/`ar`/`fr` redirects and dormant localization infrastructure for future work. | B08, B09 | All launch surfaces pass fa/RTL accessibility and viewport checks; no multilingual release claim or language switcher. |
| B11 | P1 / Search operations | Verify | Run FullTextSearch.sql on FTS-capable SQL Server; verify LIKE fallback and FTS results, paging totals, changes reflected after indexing. Existing script uses CHANGE_TRACKING AUTO. | B06; FTS-capable SQL Server | Both search paths tested with seeded queries; indexing delay and maintenance documented. No recurring rebuild unless a demonstrated operational need exists. |
| B12 | P1 / Performance and SEO | Verify | Canonical, fa/x-default hreflang, sitemap and JSON-LD checks; profile representative cold/warm list/detail/search/home requests and images/fonts. Optimize measured bottlenecks. | B08, B10, B11 | LCP <2.5s, CLS <0.1, page weight <2MB; warm p95 <300ms target with dataset, device/network and cache conditions recorded. |
| B13 | P1 / Deployment and operations | Verify | Container build/start, fresh migrations, idempotent seeding/admin bootstrap, health checks, Hangfire, production media, proxy/HTTPS, backup/restore and rollback rehearsal. Correct runbook commands; validate CI with SQL integration tests. | B04–B07, B11; Docker/SQL host | Release artifact, successful CI, readiness, persisted media, restored DB and rollback evidence in a disposable/staging environment. |
| B14 | P1 / Release acceptance | Ready | Close all applicable criteria in docs/ACCEPTANCE_CHECKLIST.md and record product-approved deferrals, including multilingual delivery; reconcile README and design plan status with verified behavior. | B03–B13 | Every launch-scope criterion has evidence; deferred criteria are explicit; no unresolved release-blocking defects. |
| B15 | P2 / Asset tooling | Deferred | Replace Tailwind runtime CDN with generated local CSS if performance/CSP evidence makes it necessary; otherwise follow-up. | B09, B12 | Reproducible build and no visual regressions if promoted into release scope. |

## Implementation batches

1. **Foundation:** B01, B02, B03 are complete.
2. **Correctness:** B04 through B07 are complete.
3. **Experience:** B08, B09, B10. Follow existing design direction; do not redo the home or wizard from scratch.
4. **Release:** B11, B12, B13, B14. Provision an FTS-capable test environment when needed; LocalDB alone must not be treated as proof of FTS support.

B11 environment preparation can happen early. This is a dependency guide, not an instruction to spawn agents.

## Test implementation notes

- Use real SQL Server semantics for integration coverage: migrations, rowversion, SQL-specific Dapper queries and transactions. Do not revive the obsolete SQLite path to make tests pass.
- Own each test database with a unique generated name, verify its identity before cleanup, and never point test cleanup at the application's normal connection string.
- Expose the application entry point to WebApplicationFactory if needed. Keep test configuration overrides in the test host; preserve production startup behavior.
- Use fast unit tests for pure rules and SQL-backed tests for query/persistence correctness. Do not add tests that only restate CSS or implementation internals.
- Separate FTS-dependent tests from portable unit tests; CI must report missing required infrastructure clearly, not count skipped release checks as passed.
- Record command, date, code revision when available, environment, expected/actual result and evidence path. Existing substantive tests are retained, not rewritten for counts.

## Changes from the old plans

- Replaced conflicting phase completion claims and stale execution order with one backlog.
- Removed obsolete SQLite requirements and the already-resolved SDK installation task.
- Converted implemented home/wizard work to verification rather than rebuilding it.
- Replaced unconditional FTS rebuild scheduling with automatic-change-tracking verification and maintenance review.
- Added cache freshness, wizard rollback/concurrency, SQL test isolation, direct-query schema checks and operational rehearsal.
- Kept runtime CDN replacement optional unless evidence promotes it to a release blocker.
- Deferred §35 favorites/ratings/corrections, editorial workflow/history, graph/timeline visualization, player/waveform, IIIF, API keys/GraphQL, imports/deduplication and AI discovery/recommendations until acceptance passes.
- Replaced the four-culture launch requirement with the user-approved Persian-only (`fa`) RTL scope. Existing localization infrastructure remains available for a future milestone.
- Retained the user-approved hybrid home catalog and clarified its handoff: initial 20, four append actions through 100, then page 6 onward.

## Related plans

- `HOME_REDESIGN_PLAN.md`: approved B08 implementation contract and verification matrix; B08/B10 govern remaining work.
- `UI_REDESIGN_PLAN.md`: visual specifications; B09/B10 govern remaining work.
- `ALBUM_WIZARD_PLAN.md`: implementation record; B07 governs regression verification.
- `DEPLOYMENT.md`: operational instructions to validate under B13.
- `docs/ACCEPTANCE_CHECKLIST.md`: release evidence matrix.
- `docs/BASELINE_2026-09-11.md`: current build/test evidence and warning ownership.
