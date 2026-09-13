# B06 — Public and API Integration Plan

Status: Done 2026-09-11. Owner task: `IMPLEMENTATION_PLAN.md` B06.

## Scope and route families

The SQL-backed application host will verify meaningful results for the four primary catalogs (albums, tracks, people, companies), search, and every implemented public MVC family: artists, poets, poems, sung versions, genres, moods, instruments, sources, locations, publications, sessions, events, awards, and charts. API coverage includes the primary catalogs, poems/sung versions, lookups, advanced catalogs, and search.

The current Persian-only middleware redirects `en`, `ar`, and `fr` routes to `fa`; this is the approved launch policy. B06 verifies database localization overlays through the reachable `fa` route. B10 owns Persian RTL and accessibility verification, while multilingual delivery is deferred.

## Test matrix

| Area | Cases | Required assertion |
|---|---|---|
| Primary detail | Album, track, person, company MVC and API | 200, seeded identity/title, valid success envelope |
| Primary lists | Search text, genre, mood, artist, year, pagination | Correct item set and pagination totals |
| Public route inventory | Every implemented MVC list/detail family | Seeded content renders; missing slug returns 404 |
| API inventory | Each API controller family | Valid success envelope; seeded content or relationship appears |
| Errors | Missing detail and invalid paging/search input | Correct 400/404 status and failure envelope |
| Search | LIKE fallback, type filter, page/pageSize | Seeded match, correct URL/type and totals; no silent SQL failure |
| Localization | Persian title overlay with English base fallback | Requested translation wins; missing localized field keeps base |
| Soft delete | Search after deletion | Deleted entity is absent (completes acceptance criterion 16) |

## Implementation rules

- Seed one uniquely named, relationally valid catalog in the disposable SQL Server database.
- Assert response bodies or JSON fields, not status codes alone.
- Treat a 200 response with unexpectedly empty data as a failure.
- Fix SQL projections and joins at their owning query layer.
- Keep full-text-specific ranking/index checks in B11; B06 exercises the portable SQL Server LIKE path.

## Completion boundary

B06 is complete when the route inventory and focused filters pass on the disposable SQL Server host, primary API envelopes and error responses are asserted, database localization fallback is demonstrated, deleted content is absent from search, and the complete Release suite passes.

## Verification result

Five SQL-backed scenarios seed a uniquely named relational catalog and assert every implemented MVC entity family plus every API controller family. Primary album, track, person, and company pages and API details return their seeded identity; album year/genre/mood/text and track artist/genre/mood/text filters return the intended row; API list metadata and 400/404 failure envelopes are valid. The `fa` album route applies its Persian title localization while unlocalized fields retain base values. Search uses the LIKE fallback on LocalDB, honors entity type and paging, handles unsupported types safely, and drops a warmed album result immediately after an authenticated admin deletion.

The tests exposed and fixed direct-SQL references to nonexistent `RecordingSession.Title`, `PerformanceEvent.Title/EventDate/StartTime/EndTime/AudienceInformation`, `Location.Description`, and `Chart.Description` columns. They also exposed a track artist join that compared `EntityId` with `TrackId`, and an album year filter that searched title text. The complete Release suite passed 66 tests with no failures or skips. Full-text ranking remains B11; Persian UI verification remains B10.
