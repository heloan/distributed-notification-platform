# =============================================================================
# Distributed Smart Notification Platform — API Gateway
# Multi-stage Docker build
# =============================================================================

# ---------------------------------------------------------------------------
# Stage 1: Build
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first (layer caching for NuGet restore)
COPY src/Gateway.Application/Gateway.Application.csproj Gateway.Application/
COPY src/Gateway.Infrastructure/Gateway.Infrastructure.csproj Gateway.Infrastructure/
COPY src/Gateway.Api/Gateway.Api.csproj Gateway.Api/

# Restore dependencies
RUN dotnet restore Gateway.Api/Gateway.Api.csproj

# Copy source code
COPY src/ .

# Build release
RUN dotnet build Gateway.Api/Gateway.Api.csproj -c Release --no-restore

# Publish
RUN dotnet publish Gateway.Api/Gateway.Api.csproj -c Release -o /app/publish --no-build

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

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
