# Project Context Handoff

_Last updated: 2026-09-19_

## Project state

- Repository: `I:\Git\MusicEncyclopedia`; active branch: `main`.
- Working tree contains accumulated B09, B10, Graphify refresh, and B11 changes. Do not reset, checkout, or discard unrelated work.
- Launch scope is Persian (`fa`) RTL only. English, Arabic, and French localization infrastructure is dormant.
- Graphify executable: `C:\Users\Rira\.local\bin\graphify.exe` (not on `PATH`). `AGENTS.md` requires Graphify queries for codebase questions and `graphify update .` after code changes.
- Graphify 0.9.61 is functional. It warns that `tree_sitter_sql` is missing, so four SQL files provide no AST data.

## User intent and milestone history

The user asked to analyze and organize unfinished work, improve plans freely, and implement B05 through B11. B05, B06, B07, B08/UI review, B09, and B10 were addressed in the prior work. The user approved the B08/UI recommendations, selected Persian RTL only, and chose hybrid pagination. The current focus is B11: search operations and SQL Server Full-Text Search (FTS) readiness.

## B11 implementation completed

### Search readiness

`src/MusicEncyclopedia.Search/Services/SearchService.cs` no longer memoizes FTS availability for the process lifetime. Each uncached search checks that SQL Server FTS is installed and enabled and that all FTS-union tables have enabled indexes: `Album`, `Track`, `Person`, `Company`, `Poem`, `SungVersion`, `Genre`, `Mood`, `Instrument`, `Source`, `Location`, `Publication`, `RecordingSession`, and `PerformanceEvent`.

If the probe fails or the index set is incomplete, the service safely uses the portable `LIKE` fallback. This avoids `FREETEXTTABLE` failures during partial FTS deployments. Cache hits bypass this probe until the normal one-minute search-cache expiry.

### Operations and documentation

Added `src/MusicEncyclopedia.Data/Scripts/VerifyFullTextSearch.sql`, a SQLCMD verifier that checks service availability, the fourteen required enabled `CHANGE_TRACKING AUTO` indexes, catalog/index state, and an `Album` `FREETEXTTABLE` smoke query.

Added `docs/B11_SEARCH_OPERATIONS_PLAN.md`, containing the command, runtime contract, validation matrix, and automatic tracking guidance. Updated `DEPLOYMENT.md`, `IMPLEMENTATION_PLAN.md`, and `docs/ACCEPTANCE_CHECKLIST.md`.

`FullTextSearch.sql` already creates auto-tracked indexes for seventeen tables. B11 verifies the fourteen tables actually used by the FTS union.

## Validation already run

Successful:

- `dotnet test MusicEncyclopedia.sln -c Release --no-restore`
- `dotnet test tests/MusicEncyclopedia.Web.Tests/MusicEncyclopedia.Web.Tests.csproj -c Release --no-restore`
- `dotnet test tests/MusicEncyclopedia.Web.Tests/MusicEncyclopedia.Web.Tests.csproj -c Release --no-build --logger "console;verbosity=normal"`
- `git diff --check`
- `C:\Users\Rira\.local\bin\graphify.exe update .`

Expected non-failing noise: EF Core query-filter and MARS warnings, one PDB copy retry warning, and the Graphify SQL parser warning.

## Remaining B11 evidence

B11 remains **Verify** because the machine has no FTS-capable SQL Server test environment.

- `sqllocaldb info` reports `MSSQLLocalDB`.
- Docker is unavailable.
- `TEST_SQLSERVER_CONNECTION_STRING` is unset.
- LocalDB runs normal tests but cannot prove SQL Server FTS behavior.
- A direct `sqlcmd` LocalDB attempt failed due unavailable client credentials/encryption support.

To close B11, use a disposable SQL Server instance with Full-Text Search installed:

1. Apply `src/MusicEncyclopedia.Data/Scripts/FullTextSearch.sql`.
2. Run `src/MusicEncyclopedia.Data/Scripts/VerifyFullTextSearch.sql` with a known indexed term.
3. Validate FTS response/pagination; stable totals; fallback after removal of a required index; recovery after reapplying an index and cache expiry without restart; and visibility of an inserted or updated unique term through automatic change tracking.

Do not apply migrations or FTS scripts to production without an explicit approved target.

## Git notes

Before a commit or push, inspect `git status --short`, current branch, remotes, and recent commits. Preserve B09/B10 and Graphify changes unless the user narrows the scope. The user previously asked to commit and push all, but check whether it already happened before repeating a network action. No B11 commit was made during the immediately preceding work.

## Likely next work

- Complete B11 verification if an FTS SQL Server target is supplied.
- Otherwise continue B12 performance and SEO, followed by B13 deployment operations and B14 acceptance, while retaining B11 as Verify until FTS evidence exists.
