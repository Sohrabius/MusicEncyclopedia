# B11 — Search operations and full-text verification

Updated: 2026-09-13. Owner: B11. Status: implementation ready; FTS environment verification pending.

## Runtime contract

`SearchService` always supports the portable SQL Server `LIKE` path. It uses ranked `FREETEXTTABLE` fragments only when SQL Server reports that Full-Text Search is installed and enabled and every one of the 14 tables in the public search union has an enabled full-text index. A missing component, insufficient permission, partial index deployment, or probe error leaves the request on `LIKE`.

The readiness probe runs for each uncached search. An operator can apply the indexes while the application is running; the next cache miss can select FTS without a restart. Search response caching still has its normal one-minute lifetime, so verify a new term or wait for that entry to expire after a deployment.

## Setup and deterministic check

1. Migrate and seed a disposable SQL Server database that has the Full-Text Search feature installed.
2. Apply [FullTextSearch.sql](../src/MusicEncyclopedia.Data/Scripts/FullTextSearch.sql).
3. Run the verifier with a seeded word known to occur on an album, replacing the connection settings for the target environment:

```bash
sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d MusicEncyclopedia \
  -i src/MusicEncyclopedia.Data/Scripts/VerifyFullTextSearch.sql \
  -v SearchTerm="known seeded album term"
```

The verifier fails if the service is unavailable, any of the 14 query tables lacks an enabled `AUTO`-tracked index, or the supplied term has no ranked Album result. Its output records the service/index state and ranked results.

## Required application checks

Run these checks against the same seeded database and record the request/response evidence:

| Check | Expected result |
| --- | --- |
| `GET /api/search?q=<known term>&page=1&pageSize=1` | A ranked FTS result and a nonzero `totalItems`. |
| Same query with page 2 when the fixture has multiple matches | Stable `totalItems`; results do not repeat page 1. |
| Disable or omit one required FTS index in a disposable copy | Search still succeeds through `LIKE`; no `FREETEXTTABLE` failure. |
| Restore the index by rerunning `FullTextSearch.sql`, then use a fresh query term or wait one minute | The next cache miss can use FTS without restarting the application. |
| Insert or update a row with a unique test term, then poll the search endpoint | The term appears after SQL Server automatic change tracking completes. Delete it and confirm it disappears after the search cache entry expires. |

`FullTextSearch.sql` uses `CHANGE_TRACKING AUTO`; do not schedule recurring catalog rebuilds. Rerun the idempotent setup script after a schema or indexed-column change, then run the verifier. Investigate a lag only when measured ingestion traffic shows it is operationally significant.

## Evidence status

- Portable `LIKE` search, paging, entity coverage, and deletion visibility: passed in the current LocalDB integration suite (B06).
- FTS-specific environment proof: blocked until an FTS-capable SQL Server instance is available. LocalDB is not accepted as evidence for this part of B11.
