# B07 — Admin CRUD and Album Wizard Plan

Status: Done on 2026-09-11. Owner task: `IMPLEMENTATION_PLAN.md` B07.

## Persistence policy

EF Core uses its normal `TrackAll` query behavior because admin controllers load entities, mutate them, and call `SaveChangesAsync`. Public catalog reads use Dapper. Read-only EF queries may opt into `AsNoTracking` locally after measurement. This replaces the unsafe global no-tracking policy that allowed admin actions to log success without issuing updates.

## Automated verification

| Area | Scenario | Evidence required |
|---|---|---|
| Wizard graph | Create album with two discs, tracks, classifications, new sung version, album/track credits, company, identifier, tag, link, and related album | Every row persists with correct FK, sequence, contributor, and direction |
| Atomicity | Force a late FK failure after the wizard's first save | No album, track, or base Entity remains |
| Concurrency | Submit a stale album rowversion after a successful edit | Conflict form returns; committed title is preserved |
| Shared tracks | Add one existing track to another album, reject duplicate, update overrides, reorder/remove | Many-to-many row and ordering persist without duplicating Track |
| Contributor rules | Person/company scope and ambiguous contributor input | Invalid combinations rejected; valid credit persists |
| Media | Assignment and invalid upload metadata/file cases | Valid assignment persists; invalid input is rejected without orphan rows |
| Admin inventory | Authenticated GET for each CRUD editor family | Route renders without 500 and contains its expected editor/list marker |

## Result

- The complete wizard graph persists in one transaction: a two-disc album, two tracks, genre/mood/instrument joins, a new sung version, album and musician credits, company, identifier, tag, external link, and bidirectional album relation.
- A deliberately invalid link-type FK fails after the wizard's first save and leaves no Album, Track, or Entity rows.
- Stale album rowversions return the conflict form and preserve the first writer's committed title.
- Adding shared tracks, rejecting a duplicate, saving overrides and flags, moving, renumbering, and removing rows all persist.
- Credit writes validate the global entity/type pair, contributor XOR, role scope, instrument/person dependency, and exact duplicates.
- Media assignment validates the global entity/type pair; an empty upload is rejected without an orphan Media row.
- All 29 primary authenticated admin GET routes return 200. The dead `/admin/attributes` navigation target was replaced with tested attribute-definition list/create/update/delete management.
- Album and track edit tabs now pass the global `Entity.EntityId` to polymorphic credit, alias, localization, link, citation, and attribute editors. Wizard credits, tags, and links use the same convention.

Verification: `dotnet test MusicEncyclopedia.sln -c Release --no-restore` passed Core 5, Services 42, Data 1, Web 26; 74 total, 0 failed, 0 skipped.

## Completion boundary

B07 met this boundary. Browser layout and responsive checks passed under B09/B10; production file persistence remains B13.
