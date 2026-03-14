# =============================================================================
# Distributed Smart Notification Platform — Notification Service
# Multi-stage Docker build
# =============================================================================

# ---------------------------------------------------------------------------
# Stage 1: Build
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first (layer caching for NuGet restore)
COPY src/NotificationService.Domain/NotificationService.Domain.csproj NotificationService.Domain/
COPY src/NotificationService.Application/NotificationService.Application.csproj NotificationService.Application/
COPY src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj NotificationService.Infrastructure/
COPY src/NotificationService.Worker/NotificationService.Worker.csproj NotificationService.Worker/

# Restore dependencies
RUN dotnet restore NotificationService.Worker/NotificationService.Worker.csproj

# Copy source code
COPY src/ .

# Build release
RUN dotnet build NotificationService.Worker/NotificationService.Worker.csproj -c Release --no-restore

# Publish
RUN dotnet publish NotificationService.Worker/NotificationService.Worker.csproj -c Release -o /app/publish --no-build

# ---------------------------------------------------------------------------
# Stage 2: Runtime
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Security: run as non-root user
RUN adduser --disabled-password --gecos '' appuser
USER appuser

# Copy published output
COPY --from=build /app/publish .

# Expose health check port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "NotificationService.Worker.dll"]
