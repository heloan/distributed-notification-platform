#!/usr/bin/env bash
# =============================================================================
# Distributed Smart Notification Platform
# Start all services
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
INFRA_DIR="$ROOT_DIR/infrastructure"

echo "🚀 Starting Distributed Smart Notification Platform..."
echo ""

# Start infrastructure first
echo "📦 Starting infrastructure (PostgreSQL, Kafka, Zookeeper, Prometheus, Grafana)..."
docker compose -f "$INFRA_DIR/docker-compose.yml" up -d

echo ""
echo "✅ All services started!"
echo ""
echo "📊 Service URLs:"
echo "   API Gateway:   http://localhost:5000"
echo "   Event Service:  http://localhost:8080"
echo "   Prometheus:     http://localhost:9090"
echo "   Grafana:        http://localhost:3000  (admin/admin)"
echo ""
echo "📝 Send a test event:"
echo '   curl -X POST http://localhost:5000/events \'
echo '     -H "Content-Type: application/json" \'
echo '     -d '\''{"eventType":"USER_REGISTERED","userId":"123","email":"user@email.com"}'\'''
echo ""
