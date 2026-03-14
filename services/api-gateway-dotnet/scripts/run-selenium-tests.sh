#!/usr/bin/env bash
# =============================================================================
# API Gateway — Run Selenium Browser Tests
# Runs both Robot Framework Selenium and Python Selenium tests
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GATEWAY_DIR="$(dirname "$SCRIPT_DIR")"
ROBOT_DIR="$GATEWAY_DIR/tests/robot"
INTEGRATION_DIR="$GATEWAY_DIR/tests/integration"

echo "🌐 Running Selenium Browser Tests..."
echo ""

cd "$INTEGRATION_DIR"

if [ ! -d "venv" ]; then
  echo "📦 Creating virtual environment..."
  python3 -m venv venv
fi

source venv/bin/activate
pip install -q -r requirements.txt

# Robot Framework Selenium tests
echo "🤖 [1/2] Robot Framework — Swagger UI Tests..."
cd "$ROBOT_DIR"
mkdir -p reports/selenium

robot \
  --outputdir reports/selenium \
  --include selenium \
  --loglevel INFO \
  --name "Swagger UI - Robot Tests" \
  tests/swagger_ui_tests.robot || true

echo ""

# Python Selenium tests
echo "🐍 [2/2] Python Selenium — Swagger UI Tests..."
cd "$INTEGRATION_DIR"
mkdir -p reports

pytest test_swagger_ui_selenium.py \
  -v \
  --html=reports/selenium-report.html \
  --self-contained-html || true

deactivate

echo ""
echo "✅ Done."
echo "📄 Reports:"
echo "   Robot:  tests/robot/reports/selenium/report.html"
echo "   Python: tests/integration/reports/selenium-report.html"
