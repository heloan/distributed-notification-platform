#!/usr/bin/env bash
# =============================================================================
# API Gateway — Run All Tests
# Executes unit, integration, Robot Framework, and Selenium tests
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GATEWAY_DIR="$(dirname "$SCRIPT_DIR")"
INTEGRATION_DIR="$GATEWAY_DIR/tests/integration"
ROBOT_DIR="$GATEWAY_DIR/tests/robot"

echo "🧪 API Gateway — Test Runner"
echo "=============================================="
echo ""

# ---------------------------------------------------------------------------
# 1. .NET Unit Tests
# ---------------------------------------------------------------------------
echo "📋 [1/4] Running .NET Unit Tests (xUnit)..."
echo "----------------------------------------------"
if command -v dotnet &> /dev/null; then
  cd "$GATEWAY_DIR"
  dotnet test --verbosity normal
  echo "   ✅ .NET Unit Tests complete."
else
  echo "   ⏭️  Skipping — .NET SDK not installed"
fi
echo ""

# ---------------------------------------------------------------------------
# 2. Python Integration Tests (pytest)
# ---------------------------------------------------------------------------
echo "📋 [2/4] Running Python Integration Tests (pytest)..."
echo "----------------------------------------------"
cd "$INTEGRATION_DIR"

if [ ! -d "venv" ]; then
  echo "   📦 Creating virtual environment..."
  python3 -m venv venv
fi

source venv/bin/activate
pip install -q -r requirements.txt

mkdir -p reports

# Run all pytest tests EXCEPT Selenium (those need a browser)
pytest -v --ignore=test_swagger_ui_selenium.py \
  --html=reports/pytest-report.html \
  --self-contained-html || true

echo "   ✅ Python Integration Tests complete."
echo "   📄 Report: tests/integration/reports/pytest-report.html"
echo ""

# ---------------------------------------------------------------------------
# 3. Robot Framework API Tests
# ---------------------------------------------------------------------------
echo "📋 [3/4] Running Robot Framework API Tests..."
echo "----------------------------------------------"
cd "$ROBOT_DIR"

mkdir -p reports

robot \
  --outputdir reports \
  --exclude selenium \
  --loglevel INFO \
  --name "API Gateway - Automated Tests" \
  tests/ || true

echo "   ✅ Robot Framework Tests complete."
echo "   📄 Report: tests/robot/reports/report.html"
echo ""

# ---------------------------------------------------------------------------
# 4. Selenium UI Tests (Robot Framework + Python)
# ---------------------------------------------------------------------------
echo "📋 [4/4] Running Selenium Browser Tests..."
echo "----------------------------------------------"

# Robot Framework Selenium tests
cd "$ROBOT_DIR"
robot \
  --outputdir reports/selenium \
  --include selenium \
  --loglevel INFO \
  --name "API Gateway - Swagger UI Tests" \
  tests/swagger_ui_tests.robot || true

# Python Selenium tests
cd "$INTEGRATION_DIR"
source venv/bin/activate
pytest test_swagger_ui_selenium.py \
  -v \
  --html=reports/selenium-report.html \
  --self-contained-html || true

echo "   ✅ Selenium Tests complete."
echo "   📄 Reports: tests/robot/reports/selenium/ & tests/integration/reports/"
echo ""

deactivate 2>/dev/null || true

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------
echo "=============================================="
echo "✅ All test suites complete!"
echo ""
echo "📄 Reports:"
echo "   .NET Unit Tests:        dotnet test results"
echo "   Python Integration:     tests/integration/reports/pytest-report.html"
echo "   Robot Framework API:    tests/robot/reports/report.html"
echo "   Selenium UI:            tests/robot/reports/selenium/report.html"
echo ""
