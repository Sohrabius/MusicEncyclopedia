# Deployment Runbook — MusicEncyclopedia

This runbook covers standing up the production stack (web + SQL Server 2022),
configuring it, and operating it: database backup/restore, monitoring, full-text
search setup, and load testing.

> Dev workflow is unaffected: the default `appsettings.json`/`appsettings.Development.json`
> still run on SQLite with seeded sample content (`dotnet run --project src/MusicEncyclopedia.Web`).

---

## 1. Architecture

```
                  ┌───────────────────────────────────────────────┐
  Users ──HTTPS──►│  Reverse proxy (nginx/Caddy/cloud LB)         │
                  │  TLS termination, static assets (optional)   │
                  └───────────────────┬───────────────────────────┘
                                      │ HTTP :8080
                  ┌───────────────────▼───────────────────────────┐
                  │  web container (mcr.microsoft.com/dotnet/    │
                  │  aspnet:8.0, non-root, healthchecked)        │
                  │  • ASP.NET Core MVC (4 cultures, RTL)        │
                  │  • SQL Server storage, Hangfire (SQL Server) │
                  │  • in-process memory cache                   │
                  └───────────────────┬───────────────────────────┘
                                      │ SQL (TrustServerCertificate)
                  ┌───────────────────▼───────────────────────────┐
                  │  mssql 2022 container                        │
                  │  • music-encyclopedia database               │
                  │  • Full-Text catalog (FTS search)            │
                  │  • volume: mssql-data (survives restarts)    │
                  └───────────────────────────────────────────────┘
```

- **State lives in the `mssql-data` and `media-data` volumes.** Rebuilding the
  images never touches them.
- **Hangfire** (dashboard at `/hangfire`, admin-only) persists its job/tables in
  the same SQL Server database.
- **Search** uses SQL Server Full-Text (`FREETEXTTABLE`). The one-time catalog
  setup is in `src/MusicEncyclopedia.Data/Scripts/FullTextSearch.sql` (see §5).

---

## 2. Prerequisites

- Docker Engine 24+ with compose v2 (`docker compose version`).
- Nothing else — no .NET SDK needed on the host to run the stack (the image is
  multi-stage and self-contained for `net8.0`).

---

## 3. Quick start

```bash
cp .env.example .env        # then edit: set MSSQL_SA_PASSWORD (+ admin + site url)
docker compose up -d --build
docker compose ps           # wait for both containers "healthy"
curl http://localhost:8080/health/ready    # → {"status":"Healthy",...}
```

On first boot the app:
1. waits for SQL Server to be healthy (compose dependency),
2. applies EF migrations (`MigrateAsync` — idempotent),
3. seeds lookup data (`Seed:OnStartup=true`, `Seed:SampleContent=false` → no demo content),
4. seeds roles + Administrator permissions and creates the first admin user —
   `ADMIN_EMAIL`/`ADMIN_PASSWORD` when set; otherwise the built-in seed default
   (`admin@example.com` / `Admin@123456`) only when `SEED_ADMIN_USER=true`
   (the SQLite/dev default — keep false in production).

Log in at `/auth/login` with the configured admin credentials (or the seeded
default on a dev database; change its password after first login).

---

## 4. Configuration reference

### 4.1 Environment variables (`.env` → compose)

| Variable | Default | Purpose |
| --- | --- | --- |
| `MSSQL_SA_PASSWORD` | — (required) | SQL Server sa password; min 8 chars with upper/lower/digit/symbol. Avoid `; $ # @` and quotes — they break the connection string / YAML |
| `MSSQL_PID` | `Express` | Edition: Express / Developer / Standard / Enterprise |
| `ADMIN_EMAIL` | *(empty)* | When set, the first administrator is created on boot (the email is also the login name) |
| `ADMIN_PASSWORD` | *(empty)* | Must satisfy the app password policy (10+ chars, digit, upper, lower, symbol) |
| `SEED_ADMIN_USER` | `false` (prod) / `true` (dev) | When `ADMIN_EMAIL` is empty and this is `true`, a built-in default admin is seeded: `admin@example.com` / `Admin@123456` (change after first login) |
| `SITE_BASE_URL` | `https://example.com` | Canonical base URL (sitemap, hreflang, canonical tags) |
| `CDN_BASE_URL` | *(empty)* | Media CDN prefix; empty = served by the app |
| `BEHIND_PROXY` | `false` | `true` when TLS terminates at an upstream proxy (honors X-Forwarded-For / X-Forwarded-Proto) |
| `ENABLE_HTTPS_REDIRECTION` | `false` | the container serves plain HTTP on 8080 (TLS terminates at the proxy) — set `true` only if the app itself terminates TLS |
| `SEED_SAMPLE_CONTENT` | `false` | `true` only for demo/staging (seeds sample artists/albums/poems) |
| `WEB_PORT` / `MSSQL_PORT` | `8080` / `1433` | Host port mappings |

### 4.2 Runtime configuration (env vars read by the app)

Anything in `appsettings.Production.json` can be overridden with the standard
`Section__Key` env-var syntax, e.g.:

```bash
ConnectionStrings__DefaultConnection="Server=mssql;Database=MusicEncyclopedia;User Id=sa;Password=...;TrustServerCertificate=True"
Site__BaseUrl=https://encyclopedia.example.com
Media__CdnBaseUrl=https://cdn.example.com
IpRateLimiting__GeneralRules__0__Limit=200
```

### 4.3 Security posture (already wired in code)

- Non-root container user; HTTPS-only cookies outside Development; `Secure`,
  `HttpOnly`, `SameSite` auth + antiforgery cookies.
- CSP + X-Content-Type-Options + X-Frame-Options + Referrer-Policy +
  Permissions-Policy headers on every response.
- Per-IP rate limits (login 5/min, search 30/min, API 60/min, global 120/min).
- Hangfire dashboard restricted to users holding any `Permission` claim
  (`HangfireDashboardAuthorizationFilter`).
- SQL Server: `EnableRetryOnFailure` on connections; EF parameterized queries.

---

## 5. Full-Text Search (one-time SQL Server setup)

Migrations create the schema but **not** the full-text catalog. On first deploy
run the script against the `music-encyclopedia` database:

```bash
# From the host (mssql port is mapped), or inside the container:
docker compose exec -T mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d MusicEncyclopedia \
  -i /scripts/FullTextSearch.sql
```

The script creates the catalog + indexes, and can be re-run after schema changes
(EF `MigrateAsync` on startup keeps the schema current; re-run FTS script after
any change to the indexed tables).

### 5.1 Hangfire jobs (automatic, SQL Server only)

- `cache-warm` — nightly 03:00, warms public list caches per culture.
- Search FTS re-indexing, if enabled, is exposed the same way.

Dashboard: `/hangfire` (requires an admin login).

---

## 6. Backups & restore drill

### 6.1 Backup (SQL Server)

Recommended: nightly full backup + 14-day retention. Simplest with `sqlcmd` on a
schedule (cron/systemd or a second container):

```bash
docker compose exec -T mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d master -Q "
    BACKUP DATABASE [MusicEncyclopedia]
    TO DISK = N'/var/opt/mssql/backup/MusicEncyclopedia.bak'
    WITH INIT, COMPRESSION, CHECKSUM;"
```

Copy `/var/opt/mssql/backup/*.bak` off the host (the volume `mssql-data` is not
a backup — it's the live data directory).

### 6.2 Restore drill (rehearse quarterly)

1. Stop the web container so nothing writes: `docker compose stop web`.
2. Restore into the existing DB (or a scratch name for practice):

```bash
docker compose exec -T mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d master -Q "
    RESTORE DATABASE [MusicEncyclopedia]
    FROM DISK = N'/var/opt/mssql/backup/MusicEncyclopedia.bak'
    WITH REPLACE, RECOVERY;"
```

3. Restart web: `docker compose start web`, then verify
   `curl http://localhost:8080/health/ready` and a known page (e.g. `/fa/albums`).

> `RESTORE ... WITH REPLACE` is destructive — practice on a scratch DB name and
> confirm media files (volume `media-data`) match the restored DB.

### 6.3 Media volume

`media-data` holds uploads. Back it up alongside the DB (they must be
restored together to stay consistent).

---

## 7. Monitoring

| Endpoint | Meaning |
| --- | --- |
| `/health/live` | Liveness — process up, no dependencies touched (used by the container healthcheck) |
| `/health/ready` | Readiness — SQL Server reachable (DB tag) |
| `/health` | Full report incl. SQL Server check; JSON with per-check duration/tags |

Wire these into your orchestrator / uptime monitor (e.g. `curl -f` every 30 s).

Logs (Serilog): console (stdout, captured by Docker) + rolling files in
`/app/logs` (31-day retention). View with:

```bash
docker compose logs -f web
```

### 7.1 What to alert on

- `/health/ready` non-200 for > 3 consecutive probes.
- `429` spike (rate limiting) or repeated `HttpStatusCode=500` in logs.
- Hangfire failures: `Log.Warning("Failed to register Hangfire recurring jobs")`
  or recurring-job exceptions in the dashboard.

---

## 8. Load testing

The app caches heavily (5 m lists/home, 10 m details, 60 s on direct-DB pages),
so warm throughput is well above cold-start. Sample `hey` test (install from
https://github.com/rakyll/hey):

```bash
hey -n 5000 -c 50 -z 30s http://localhost:8080/fa/albums
```

Targets: p95 < 300 ms warm; error rate 0%; `429` rate-limit responses expected
only for search/login endpoints under abuse, not for browse pages (limits 120/min
global).

---

## 9. TLS & reverse proxy

Recommended topology: the app speaks plain HTTP on :8080; TLS terminates at an
upstream proxy (nginx/Caddy/cloud LB). When doing so set in `.env`:

```bash
BEHIND_PROXY=true
ENABLE_HTTPS_REDIRECTION=false
SITE_BASE_URL=https://your-domain
```

and have the proxy forward `X-Forwarded-For` + `X-Forwarded-Proto`. For stricter
environments, restrict `KnownProxies` in `Program.cs` (the forwarded-headers
block) to the proxy's IP.

---

## 10. Rollout & rollback

- **Deploy:** pull new image → `docker compose up -d web` → app runs EF
  migrations on boot (idempotent) → verify `/health/ready` + a browse page.
- **Rollback:** `docker compose up -d web --build` with the previous image tag /
  `docker compose rollback web` (compose v2.23+) — DB schema is forward-only, so
  a rollback is safe only for code; a schema-rollback requires the §6 restore
  drill.
- **Zero-downtime:** run two replicas behind the proxy with `restart: unless-stopped`
  and readiness-gated traffic; Hangfire `DisableGlobalLocks` already supports
  multiple web instances sharing one SQL Server.

---

## 11. Troubleshooting

| Symptom | Likely cause / fix |
| --- | --- |
| `web` never becomes healthy | SQL Server not ready or `MSSQL_SA_PASSWORD` mismatch — check `docker compose logs mssql` |
| Login fails with weak-password errors | `ADMIN_PASSWORD` must meet policy (10+, digit, upper, lower, symbol) |
| Search returns nothing | FTS script (§5) not run yet — the LIKE fallback only exists on SQLite |
| `429 Too Many Requests` | Rate limits hit — tune `IpRateLimiting` or verify `BEHIND_PROXY` is set so client IPs are real |
| HTTPS redirect loop | `ENABLE_HTTPS_REDIRECTION=false` when the proxy already terminates TLS |
