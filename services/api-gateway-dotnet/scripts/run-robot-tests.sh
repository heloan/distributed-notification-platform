#!/usr/bin/env bash
# =============================================================================
# API Gateway — Run Robot Framework Tests Only
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GATEWAY_DIR="$(dirname "$SCRIPT_DIR")"
ROBOT_DIR="$GATEWAY_DIR/tests/robot"
INTEGRATION_DIR="$GATEWAY_DIR/tests/integration"

echo "🤖 Running Robot Framework Tests..."
echo ""

cd "$INTEGRATION_DIR"

if [ ! -d "venv" ]; then
  echo "📦 Creating virtual environment..."
  python3 -m venv venv
fi

source venv/bin/activate
pip install -q -r requirements.txt

cd "$ROBOT_DIR"
mkdir -p reports

# Run API tests (exclude Selenium browser tests by default)
robot \
  --outputdir reports \
  --exclude selenium \
  --loglevel INFO \
  --name "API Gateway - Robot Tests" \
  "$@" \
  tests/

deactivate

echo ""
echo "✅ Done. Report: tests/robot/reports/report.html"
