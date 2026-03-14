#!/usr/bin/env bash
# =============================================================================
# Distributed Smart Notification Platform
# Stop all services
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
INFRA_DIR="$ROOT_DIR/infrastructure"

echo "🛑 Stopping Distributed Smart Notification Platform..."
echo ""

docker-compose -f "$INFRA_DIR/docker-compose.yml" down

echo ""
echo "✅ All services stopped."
