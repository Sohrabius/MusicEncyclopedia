# B05 — Cache Correctness Plan

Status: Done 2026-09-11. Owner task: `IMPLEMENTATION_PLAN.md` B05.

## Policy

Dynamic encyclopedia responses use the evictable `ICacheService` layer where a cached query service exists. Admin writes invalidate keys through the controller and the global audit filter. ASP.NET response caching is removed from dynamic controller actions because it has no key/tag eviction connection to those writes and can return stale or access-sensitive representations.

Direct-Dapper pages remain uncached until B12 profiling justifies tag-aware output caching. Static-file caching is independent. Lyrics responses remain explicitly `no-store` because access varies by principal. Other error actions that already specify `no-store` retain it.

## Implementation checks

| Case | Warm before write | Write | Expected next request |
|---|---|---|---|
| Album detail MVC/API | Same slug | Edit title | Updated title |
| Home album partial/list | Same page/category | Edit title | Updated title |
| Album detail MVC/API | Same slug | Soft delete | 404 |
| Home album partial/list | Same page/category | Soft delete | Album absent |
| Album detail and lists | Deleted slug/page | Restore | Updated album visible |
| Protected lyrics | Anonymous/authenticated | Principal changes | Never shared; `no-store` |
| Cross-entity content | Album/person/track detail | Localization/media/credit edit | Dependent detail keys evicted |
| Cultures and filters | Two cultures/query shapes | Any write | Keys remain separated; affected families evicted |

## Completion boundary

B05 is complete when the edit/delete/restore sequence passes against a warm application host, positive response-cache attributes no longer cover mutable controller data, dependent-key invalidation has focused tests, and the whole Release suite passes. Performance work must measure this policy under B12 before reintroducing any HTTP output cache.

## Verification result

The SQL-backed Web test warms album MVC detail, API detail, and the home album partial, then edits, soft-deletes, and restores through authenticated admin forms with antiforgery tokens. Every next public request sees the committed state. The test exposed no-tracking EF queries in the album write actions; edit/delete/restore and their bulk variants now opt into tracking explicitly. Focused service tests cover localization, media, credit, culture, and query-shape eviction. The complete solution run passed 61 tests with no failures or skips.
