# ────────────────────────────────────────────────────────────────
# MusicEncyclopedia — multi-stage production image
# Build:   docker build -t music-encyclopedia .
# Run:     see docker-compose.yml for the full web + SQL Server stack
# ────────────────────────────────────────────────────────────────

# ── Build stage ─────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (layer-cached) — solution, project files, and global.json
COPY global.json .
COPY MusicEncyclopedia.sln .
COPY src/MusicEncyclopedia.Core/MusicEncyclopedia.Core.csproj src/MusicEncyclopedia.Core/
COPY src/MusicEncyclopedia.Data/MusicEncyclopedia.Data.csproj src/MusicEncyclopedia.Data/
COPY src/MusicEncyclopedia.Services/MusicEncyclopedia.Services.csproj src/MusicEncyclopedia.Services/
COPY src/MusicEncyclopedia.Search/MusicEncyclopedia.Search.csproj src/MusicEncyclopedia.Search/
COPY src/MusicEncyclopedia.Media/MusicEncyclopedia.Media.csproj src/MusicEncyclopedia.Media/
COPY src/MusicEncyclopedia.Web/MusicEncyclopedia.Web.csproj src/MusicEncyclopedia.Web/
COPY tests/MusicEncyclopedia.Core.Tests/MusicEncyclopedia.Core.Tests.csproj tests/MusicEncyclopedia.Core.Tests/
COPY tests/MusicEncyclopedia.Data.Tests/MusicEncyclopedia.Data.Tests.csproj tests/MusicEncyclopedia.Data.Tests/
COPY tests/MusicEncyclopedia.Services.Tests/MusicEncyclopedia.Services.Tests.csproj tests/MusicEncyclopedia.Services.Tests/
COPY tests/MusicEncyclopedia.Web.Tests/MusicEncyclopedia.Web.Tests.csproj tests/MusicEncyclopedia.Web.Tests/

RUN dotnet restore MusicEncyclopedia.sln

# Copy everything and publish the web project (Release, no restore — already done)
COPY src/ src/
COPY tests/ tests/
RUN dotnet publish src/MusicEncyclopedia.Web/MusicEncyclopedia.Web.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# ── Runtime stage ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Media uploads are written to /app/media (volume-mounted via docker-compose) and
# Serilog rolls into /app/logs. Create both as root and hand them to the non-root
# app user BEFORE dropping privileges — the app user cannot mkdir in a root-owned
# /app, and the media-data volume inherits this ownership.
RUN mkdir -p /app/media /app/logs && chown -R $APP_UID:$APP_UID /app/media /app/logs

# Non-root user (comes pre-created in the aspnet image)
USER $APP_UID

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .

# The aspnet Debian images ship curl; probe the liveness endpoint (no DB dependency)
HEALTHCHECK --interval=30s --timeout=5s --start-period=45s --retries=3 \
    CMD curl -fsS http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "MusicEncyclopedia.Web.dll"]
