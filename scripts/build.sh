#!/usr/bin/env bash
# =============================================================================
# Distributed Smart Notification Platform
# Build all services
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
SERVICES_DIR="$ROOT_DIR/services"

echo "🔨 Building Distributed Smart Notification Platform services..."
echo ""

# Build API Gateway (.NET)
if [ -d "$SERVICES_DIR/api-gateway-dotnet" ] && [ -f "$SERVICES_DIR/api-gateway-dotnet/Dockerfile" ]; then
  echo "📦 Building API Gateway (.NET)..."
  docker build -t dsnp-api-gateway "$SERVICES_DIR/api-gateway-dotnet"
  echo "   ✅ API Gateway built."
  echo ""
else
  echo "⏭️  Skipping API Gateway (not yet created)"
  echo ""
fi

# Build Event Service (Java)
if [ -d "$SERVICES_DIR/event-service-java" ] && [ -f "$SERVICES_DIR/event-service-java/Dockerfile" ]; then
  echo "📦 Building Event Service (Java)..."
  docker build -t dsnp-event-service "$SERVICES_DIR/event-service-java"
  echo "   ✅ Event Service built."
  echo ""
else
  echo "⏭️  Skipping Event Service (not yet created)"
  echo ""
fi

# Build Notification Service (.NET)
if [ -d "$SERVICES_DIR/notification-service-dotnet" ] && [ -f "$SERVICES_DIR/notification-service-dotnet/Dockerfile" ]; then
  echo "📦 Building Notification Service (.NET)..."
  docker build -t dsnp-notification-service "$SERVICES_DIR/notification-service-dotnet"
  echo "   ✅ Notification Service built."
  echo ""
else
  echo "⏭️  Skipping Notification Service (not yet created)"
  echo ""
fi

echo "✅ Build complete!"
