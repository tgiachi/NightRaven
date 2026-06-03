# syntax=docker/dockerfile:1.7

# ---------------------------------------------------------------------------
# Web build stage
# ---------------------------------------------------------------------------
FROM node:22-alpine AS web-build
WORKDIR /web

COPY ["web/package.json", "web/package-lock.json", "./"]
RUN npm ci

COPY web/ ./
RUN npm run build

# ---------------------------------------------------------------------------
# Build stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution-wide build props + every referenced csproj first so the
# `dotnet restore` layer caches across source-only changes.
COPY ["Directory.Build.props", "global.json", "./"]
COPY ["src/NightHeaven.Core/NightHeaven.Core.csproj", "src/NightHeaven.Core/"]
COPY ["src/NightHeaven.Hosting/NightHeaven.Hosting.csproj", "src/NightHeaven.Hosting/"]
COPY ["src/NightHeaven.Network/NightHeaven.Network.csproj", "src/NightHeaven.Network/"]
COPY ["src/NightHeaven.Network.UO/NightHeaven.Network.UO.csproj", "src/NightHeaven.Network.UO/"]
COPY ["src/NightHeaven.Persistence/NightHeaven.Persistence.csproj", "src/NightHeaven.Persistence/"]
COPY ["src/NightHeaven.Scripting.Lua/NightHeaven.Scripting.Lua.csproj", "src/NightHeaven.Scripting.Lua/"]
COPY ["src/NightHeaven.Server/NightHeaven.Server.csproj", "src/NightHeaven.Server/"]
RUN dotnet restore "src/NightHeaven.Server/NightHeaven.Server.csproj"

# Now bring in the sources and publish.
COPY src/ src/
RUN dotnet publish "src/NightHeaven.Server/NightHeaven.Server.csproj" \
        -c "$BUILD_CONFIGURATION" \
        -o /app/publish \
        --no-restore \
        /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Runtime stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# The server writes logs, plugins, scripts and save state under NIGHTHEAVEN_ROOT.
# Defaulting to /data keeps runtime state outside /app so the container image
# stays immutable and a single volume mount preserves everything.
ENV NIGHTHEAVEN_ROOT=/data \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_NOLOGO=1 \
    DOTNET_RUNNING_IN_CONTAINER=true

WORKDIR /app
COPY --from=build /app/publish .
COPY --from=web-build /web/dist ./wwwroot

# Pre-create the data dir owned by the non-root APP_UID baked into the
# aspnet base image, then mark it as a persistent volume.
RUN mkdir -p /data && chown -R $APP_UID:$APP_UID /data
VOLUME ["/data"]

USER $APP_UID
EXPOSE 8080
EXPOSE 2593/tcp
EXPOSE 12000/udp

ENTRYPOINT ["dotnet", "NightHeaven.Server.dll"]
