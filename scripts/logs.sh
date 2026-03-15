#!/usr/bin/env bash
# =============================================================================
# Distributed Smart Notification Platform
# Show logs from all running containers
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
INFRA_DIR="$ROOT_DIR/infrastructure"

# Default: follow logs and show last 100 lines
FOLLOW=true
TAIL="100"
SERVICE=""

usage() {
  echo "Usage: $(basename "$0") [OPTIONS] [SERVICE...]"
  echo ""
  echo "Show logs from all running DSNP containers."
  echo ""
  echo "Options:"
  echo "  -n, --tail NUM   Number of lines to show from the end (default: 100)"
  echo "  --no-follow      Print logs and exit (don't follow)"
  echo "  -h, --help       Show this help message"
  echo ""
  echo "Services: postgres, zookeeper, kafka, api-gateway, event-service,"
  echo "          notification-service, prometheus, grafana, jenkins"
  echo ""
  echo "Examples:"
  echo "  $(basename "$0")                  # Follow all logs"
  echo "  $(basename "$0") kafka            # Follow only Kafka logs"
  echo "  $(basename "$0") --no-follow      # Print all logs and exit"
  echo "  $(basename "$0") -n 50 kafka      # Last 50 lines of Kafka logs"
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -n|--tail)
      TAIL="$2"
      shift 2
      ;;
    --no-follow)
      FOLLOW=false
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      SERVICE="$SERVICE $1"
      shift
      ;;
  esac
done

FOLLOW_FLAG=""
if [ "$FOLLOW" = true ]; then
  FOLLOW_FLAG="-f"
fi

echo "📋 Showing logs from DSNP containers (tail=${TAIL})..."
echo "   Press Ctrl+C to stop."
echo ""

docker compose -f "$INFRA_DIR/docker-compose.yml" logs $FOLLOW_FLAG --tail="$TAIL" $SERVICE
