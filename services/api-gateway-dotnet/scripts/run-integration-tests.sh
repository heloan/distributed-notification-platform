#!/usr/bin/env bash
# =============================================================================
# API Gateway — Run Integration Tests Only (Python + pytest)
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GATEWAY_DIR="$(dirname "$SCRIPT_DIR")"
INTEGRATION_DIR="$GATEWAY_DIR/tests/integration"

echo "🧪 Running Python Integration Tests..."
echo ""

cd "$INTEGRATION_DIR"

if [ ! -d "venv" ]; then
  echo "📦 Creating virtual environment..."
  python3 -m venv venv
fi

source venv/bin/activate
pip install -q -r requirements.txt

mkdir -p reports

pytest -v \
  --ignore=test_swagger_ui_selenium.py \
  --html=reports/pytest-report.html \
  --self-contained-html \
  "$@"

deactivate

echo ""
echo "✅ Done. Report: tests/integration/reports/pytest-report.html"
