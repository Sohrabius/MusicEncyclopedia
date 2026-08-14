# MusicEncyclopedia

A multilingual encyclopedia of Persian and world music: artists, albums, tracks,
poems, sung versions, recording sessions, live events, awards, charts,
publications, certifications and more — with a full admin area, full-text
search, and per-culture localization (fa / en / ar / fr, RTL-aware).

## Stack

- **.NET 8** / ASP.NET Core MVC (Razor, server-rendered, Tailwind CSS)
- **EF Core 8** + Dapper — SQL Server (all environments)
- **ASP.NET Core Identity** — roles + 21 permission policies, audit logging
- **Hangfire** — background jobs (cache warm-up), SQL Server only
- **In-process cache** with real invalidation on admin writes
- **Serilog** logging, health checks (`/health`, `/health/ready`, `/health/live`),
  per-IP rate limiting, security headers + CSP

## Local development

```bash
dotnet restore MusicEncyclopedia.sln
dotnet run --project src/MusicEncyclopedia.Web
# http://localhost:5000  (or 5090/5178 per launchSettings) — /fa/albums etc.
```

The app requires SQL Server — set `DefaultConnection` in
`appsettings.Development.json` (or `DB_PASSWORD`/env overrides per
`DEPLOYMENT.md` §4) and run against a local SQL Server / LocalDB instance.

Development mode seeds schema + lookup data + sample content automatically on
startup. Register a user in the UI, then grant yourself roles/permissions via the
**Admin → Users / Roles** screens (there is no dev auto-admin).

## Tests

```bash
dotnet test MusicEncyclopedia.sln
```

## Production deployment

Container stack (web + SQL Server 2022), CI, full-text search setup, backup /
restore drill, monitoring and load-test guidance: see **[DEPLOYMENT.md](DEPLOYMENT.md)**.

Quick start:

```bash
cp .env.example .env   # set MSSQL_SA_PASSWORD (+ admin + site URL)
docker compose up -d --build
```

## Documentation

- `music-encyclopedia-q.md` — product spec (requirements, §24 milestones, §36 acceptance criteria)
- `IMPLEMENTATION_PLAN.md` — phased implementation plan and completion records
- `design.md` — design notes
